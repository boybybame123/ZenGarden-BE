using System.Text;
using System.Text.Json.Serialization;
using Amazon.Extensions.NETCore.Setup;
using AspNetCoreRateLimit;
using FluentValidation.AspNetCore;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.AspNetCore.OData;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using ZenGarden.API.Filters;
using ZenGarden.API.Middleware;
using ZenGarden.API.Services;
using ZenGarden.API.Validations;
using ZenGarden.Core.Hubs;
using ZenGarden.Core.Interfaces.IRepositories;
using ZenGarden.Core.Interfaces.IServices;
using ZenGarden.Core.Mappings;
using ZenGarden.Core.Services;
using ZenGarden.Domain.Config;
using ZenGarden.Domain.DTOs;
using ZenGarden.Infrastructure.BackgroundJobs;
using ZenGarden.Infrastructure.Persistence;
using ZenGarden.Infrastructure.Repositories;

var builder = WebApplication.CreateBuilder(args);

// Cấu hình các dịch vụ
ConfigureServices(builder);
ConfigureRepositories(builder);
ConfigureDbContext(builder);
ConfigureAuthentication(builder);
ConfigureCors(builder);
ConfigureSwagger(builder);
ConfigureRateLimiting(builder);
ConfigureBackgroundJobs(builder);

// Xây dựng ứng dụng
var app = builder.Build();

// Cấu hình pipeline
ConfigurePipeline(app);

app.Run();

public static partial class Program
{
    private static void ConfigureServices(WebApplicationBuilder builder)
    {
        builder.Services.AddControllers()
            .AddOData(options => options.Select().Filter().OrderBy().Count().SetMaxTop(100).Expand().Filter())
            .AddJsonOptions(options =>
            {
                options.JsonSerializerOptions.ReferenceHandler = ReferenceHandler.IgnoreCycles;
                options.JsonSerializerOptions.MaxDepth = 64;
            });

        builder.Services.Configure<FormOptions>(options =>
        {
            options.MultipartBodyLengthLimit = 104857600; // 100MB
        });

        builder.Services.AddEndpointsApiExplorer();
        builder.Services.AddAutoMapper(typeof(MappingProfile));

        // Cấu hình các dịch vụ chính
        builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();
        builder.Services.AddScoped<IEmailService, EmailService>();
        builder.Services.AddScoped<IFocusMethodService, FocusMethodService>();
        builder.Services.AddScoped<IBagService, BagService>();
        builder.Services.AddScoped<IPackagesService, PackagesService>();
        builder.Services.AddScoped<IItemDetailService, ItemDetailService>();
        builder.Services.AddScoped<IItemService, ItemService>();
        builder.Services.AddScoped<ITaskService, TaskService>();
        builder.Services.AddScoped<IUserService, UserService>();
        builder.Services.AddScoped<ITradeHistoryService, TradeHistoryService>();
        builder.Services.AddScoped<ITreeService, TreeService>();
        builder.Services.AddScoped<ITokenService, TokenService>();
        builder.Services.AddScoped<IUserTreeService, UserTreeService>();
        builder.Services.AddScoped<IUserXpConfigService, UserXpConfigService>();
        builder.Services.AddScoped<IChallengeService, ChallengeService>();
        builder.Services.AddSingleton<IS3Service, S3Service>();
        builder.Services.AddScoped<IUserXpLogService, UserXpLogService>();
        builder.Services.AddScoped<IPurchaseService, PurchaseService>();
        builder.Services.AddScoped<ITaskTypeService, TaskTypeService>();
        builder.Services.AddScoped<IXpConfigService, XpConfigService>();
        builder.Services.AddScoped<INotificationService, NotificationService>();
        builder.Services.AddScoped<IWalletService, WalletService>();
        builder.Services.AddScoped<ITradeTreeService, TradeTreeService>();
        builder.Services.AddScoped<IUserChallengeService, UserChallengeService>();
        builder.Services.AddScoped<IUseItemService, UseItemService>();
        builder.Services.AddScoped<ITransactionsService, TransactionsService>();
        builder.Services.AddScoped<IBagItemService, BagItemService>();
        builder.Services.AddScoped<IBagItemService, BagItemService>();
        builder.Services.AddScoped<ITreeXpLogService, TreeXpLogService>();
        builder.Services.AddSingleton<IUserIdProvider, CustomUserIdProvider>();
        builder.Services.AddScoped<IRedisService, RedisService>();
        builder.Services.AddScoped<PaymentService>();
        builder.Services.AddScoped<IFocusTrackingService, FocusTrackingService>();

        builder.Services.AddScoped<ZenGardenContext>();

        // SignalR và realtime
        builder.Services.AddSignalR();

        builder.Services.AddSingleton<RealtimeBackgroundService>();
        builder.Services.AddHostedService(provider => provider.GetRequiredService<RealtimeBackgroundService>());
        builder.Services.AddScoped<TaskRealtimeService>();

        // FluentValidation
        builder.Services.AddControllers()
            .AddFluentValidation(fv => { fv.RegisterValidatorsFromAssemblyContaining<LoginValidator>(); });

        // Bảo vệ dữ liệu
        builder.Services.AddDataProtection()
            .PersistKeysToDbContext<ZenGardenContext>();

        // Cấu hình OpenAI và DeepInfra
        ConfigureAi(builder);

        // Cấu hình health checks
        builder.Services.AddHealthChecks()
            .AddDbContextCheck<ZenGardenContext>()
            .AddRedis(builder.Configuration.GetConnectionString("Redis") ?? 
                $"redis://{builder.Configuration["Redis:Host"]}:{builder.Configuration["Redis:Port"]},password={builder.Configuration["Redis:Password"]},ssl={builder.Configuration["Redis:UseSSL"]}", 
                name: "redis",
                failureStatus: HealthStatus.Degraded,
                tags: ["redis", "cache"]);
    }

    private static void ConfigureAi(WebApplicationBuilder builder)
    {
        builder.Services.Configure<OpenAiSettings>(builder.Configuration.GetSection("OpenAI"));
        builder.Services.AddHttpClient<FocusMethodRepository>();

        var deepInfraApiKey = Environment.GetEnvironmentVariable("DEEPINFRA_API_KEY")
                              ?? builder.Configuration["DeepInfra:ApiKey"]
                              ?? throw new InvalidOperationException("DeepInfra API Key is missing.");

        builder.Services.Configure<OpenAiSettings>(options => { options.ApiKey = deepInfraApiKey; });
    }

    private static void ConfigureRepositories(WebApplicationBuilder builder)
    {
        // Register Generic Repository with Redis
        builder.Services.AddScoped(typeof(IGenericRepository<>), typeof(GenericRepository<>));

        // Register specific repositories
        builder.Services.AddScoped<ITradeHistoryRepository, TradeHistoryRepository>();
        builder.Services.AddScoped<IBagRepository, BagRepository>();
        builder.Services.AddScoped<IPackagesRepository, PackagesRepository>();
        builder.Services.AddScoped<IUserExperienceRepository, UserExperienceRepository>();
        builder.Services.AddScoped<IUserXpConfigRepository, UserXpConfigRepository>();
        builder.Services.AddScoped<IWalletRepository, WalletRepository>();
        builder.Services.AddScoped<IFocusMethodRepository, FocusMethodRepository>();
        builder.Services.AddScoped<ITreeRepository, TreeRepository>();
        builder.Services.AddScoped<IItemDetailRepository, ItemDetailRepository>();
        builder.Services.AddScoped<IUserTreeRepository, UserTreeRepository>();
        builder.Services.AddScoped<ITreeXpLogRepository, TreeXpLogRepository>();
        builder.Services.AddScoped<ITreeXpConfigRepository, TreeXpConfigRepository>();
        builder.Services.AddScoped<ITaskTypeRepository, TaskTypeRepository>();
        builder.Services.AddScoped<IItemRepository, ItemRepository>();
        builder.Services.AddScoped<ITaskRepository, TaskRepository>();
        builder.Services.AddScoped<IUserRepository, UserRepository>();
        builder.Services.AddScoped<IXpConfigRepository, XpConfigRepository>();
        builder.Services.AddScoped<IChallengeRepository, ChallengeRepository>();
        builder.Services.AddScoped<IUserXpLogRepository, UserXpLogRepository>();
        builder.Services.AddScoped<IBagItemRepository, BagItemRepository>();
        builder.Services.AddScoped<IPurchaseHistoryRepository, PurchaseHistoryRepository>();
        builder.Services.AddScoped<IUserChallengeRepository, UserChallengeRepository>();
        builder.Services.AddScoped<IChallengeTaskRepository, ChallengeTaskRepository>();
        builder.Services.AddScoped<IUserConfigRepository, UserConfigRepository>();
        builder.Services.AddScoped<ITransactionsRepository, TransactionsRepository>();
        builder.Services.AddScoped<IPackageRepository, PackageRepository>();
        builder.Services.AddScoped<INotificationRepository, NotificationRepository>();
        builder.Services.AddScoped<IFocusTrackingRepository, FocusTrackingRepository>();
        builder.Services.AddScoped<IFocusActivityRepository, FocusActivityRepository>();
    }

    private static void ConfigureDbContext(WebApplicationBuilder builder)
    {
        var connectionString = Environment.GetEnvironmentVariable("DATABASE_URL")
                               ?? builder.Configuration.GetConnectionString("ZenGardenDB")
                               ?? throw new InvalidOperationException("Database connection string is missing.");

        builder.Services.AddDbContext<ZenGardenContext>(options =>
        {
            options.UseMySql(
                connectionString,
                ServerVersion.AutoDetect(connectionString),
                mySqlOptions => mySqlOptions
                    .EnableRetryOnFailure(
                        maxRetryCount: 5,
                        maxRetryDelay: TimeSpan.FromSeconds(30),
                        errorNumbersToAdd: null)
                    .CommandTimeout(60)
            );
        });
    }

    private static void ConfigureAuthentication(WebApplicationBuilder builder)
    {
        builder.Services.Configure<JwtSettings>(builder.Configuration.GetSection("Jwt"));

        var jwtSettings = builder.Configuration.GetSection("Jwt").Get<JwtSettings>()
                          ?? throw new InvalidOperationException("JWT settings are missing in configuration.");

        if (string.IsNullOrEmpty(jwtSettings.Key))
            throw new InvalidOperationException("JWT Key is missing in configuration.");

        builder.Services.AddAuthentication(options =>
            {
                options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
            })
            .AddJwtBearer(options =>
            {
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidateAudience = true,
                    ValidateLifetime = true,
                    ValidateIssuerSigningKey = true,
                    ValidIssuer = jwtSettings.Issuer,
                    ValidAudience = jwtSettings.Audience,
                    IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSettings.Key)),
                    ClockSkew = TimeSpan.Zero
                };
            });
    }

    private static void ConfigureCors(WebApplicationBuilder builder)
    {
        var allowedOrigins = builder.Configuration.GetSection("AllowedOrigins").Get<string[]>()
                             ??
                             [
                                 "http://localhost:5173", "https://localhost:7262", "http://localhost:5153",
                                 "https://zengarden-fe.vercel.app"
                             ];

        builder.Services.AddCors(options =>
        {
            options.AddPolicy("AllowAll",
                policy =>
                {
                    policy.WithOrigins(allowedOrigins)
                        .AllowAnyMethod()
                        .AllowAnyHeader()
                        .AllowCredentials();
                });
        });
    }

    private static void ConfigureSwagger(WebApplicationBuilder builder)
    {
        builder.Services.AddSwaggerGen(c =>
        {
            c.SwaggerDoc("v1", new OpenApiInfo { Title = "ZenGarden API", Version = "v1" });
            c.OperationFilter<SwaggerFileOperationFilter>();

            c.MapType<FileObject>(() => new OpenApiSchema
            {
                Type = "object",
                Properties = new Dictionary<string, OpenApiSchema>
                {
                    ["fileName"] = new() { Type = "string" },
                    ["fileBase64"] = new() { Type = "string", Format = "byte" },
                    ["path"] = new() { Type = "string" }
                }
            });

            c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
            {
                Name = "Authorization",
                Type = SecuritySchemeType.Http,
                Scheme = "bearer",
                BearerFormat = "JWT",
                In = ParameterLocation.Header,
                Description = "Enter your Bearer token in the format: Bearer {your token}"
            });

            c.AddSecurityRequirement(new OpenApiSecurityRequirement
            {
                {
                    new OpenApiSecurityScheme
                    {
                        Reference = new OpenApiReference { Type = ReferenceType.SecurityScheme, Id = "Bearer" }
                    },
                    []
                }
            });
        });
    }

    private static void ConfigureRateLimiting(WebApplicationBuilder builder)
    {
        builder.Services.AddMemoryCache();

        // Cấu hình rate limiting từ file nếu có, nếu không sử dụng cấu hình mặc định
        if (builder.Configuration.GetSection("IpRateLimiting").Exists())
            builder.Services.Configure<IpRateLimitOptions>(builder.Configuration.GetSection("IpRateLimiting"));
        else
            builder.Services.Configure<IpRateLimitOptions>(options =>
            {
                options.GeneralRules =
                [
                    new RateLimitRule
                    {
                        Endpoint = "*",
                        Limit = 100,
                        Period = "1m"
                    }
                ];
            });

        builder.Services.AddSingleton<IRateLimitCounterStore, MemoryCacheRateLimitCounterStore>();
        builder.Services.AddSingleton<IIpPolicyStore, MemoryCacheIpPolicyStore>();
        builder.Services.AddSingleton<IRateLimitConfiguration, RateLimitConfiguration>();
        builder.Services.AddSingleton<IProcessingStrategy, AsyncKeyLockProcessingStrategy>();
    }

    private static void ConfigureBackgroundJobs(WebApplicationBuilder builder)
    {
        // builder.Services.AddHostedService<AutoPauseTaskJob>();
        builder.Services.AddHostedService<OverdueTaskJob>();
        builder.Services.AddHostedService<HandleExpiredChallengesJob>();
        builder.Services.AddHostedService<WeeklyTaskResetJob>();
        builder.Services.AddHostedService<TaskNotifierService>();

        // Cấu hình logging
        builder.Logging.AddConsole();
        builder.Logging.SetMinimumLevel(LogLevel.Debug);

        // Cấu hình AWS
        builder.Services.Configure<AWSOptions>(builder.Configuration.GetSection("AWS"));
    }

    private static void ConfigurePipeline(WebApplication app)
    {
        // Middleware exception & logging
        if (app.Environment.IsDevelopment()) app.UseDeveloperExceptionPage();
        app.UseMiddleware<ExceptionHandlingMiddleware>();
        app.UseMiddleware<LoggingMiddleware>();
        app.UseMiddleware<ValidationMiddleware>();
        app.UseMiddleware<PerformanceMiddleware>();


        app.UseRouting();
        // CORS & Routing
        app.UseCors("AllowAll");
        // Authentication & Authorization
        app.UseAuthentication();
        app.UseAuthorization();

        app.UseMiddleware<UserContextMiddleware>();

        // Rate limiting
        app.UseIpRateLimiting();

        // Auth middleware
        app.UseMiddleware<JwtMiddleware>();


        // Endpoints
        app.MapControllers();


        app.MapHub<NotificationHub>("/hubs/notification")
              .RequireCors("AllowAll");
        app.MapHub<TaskHub>("/hubs/task")
             .RequireCors("AllowAll");
        // Swagger
        app.UseSwagger();
        app.UseSwaggerUI();

        // Health checks
        app.MapHealthChecks("/health", new HealthCheckOptions
        {
            ResponseWriter = async (context, report) =>
            {
                context.Response.ContentType = "application/json";
                var response = new
                {
                    status = report.Status.ToString(),
                    checks = report.Entries.Select(x => new
                    {
                        name = x.Key,
                        status = x.Value.Status.ToString(),
                        description = x.Value.Description,
                        duration = x.Value.Duration.ToString()
                    })
                };
                await context.Response.WriteAsJsonAsync(response);
            }
        });
    }
}