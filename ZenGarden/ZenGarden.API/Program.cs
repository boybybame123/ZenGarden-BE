using System.Text;
using System.Text.Json.Serialization;
using Amazon.Extensions.NETCore.Setup;
using AspNetCoreRateLimit;
using FluentValidation.AspNetCore;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.AspNetCore.OData;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using ZenGarden.API.Hubs;
using ZenGarden.API.Middleware;
using ZenGarden.API.Services;
using ZenGarden.API.Validations;
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

builder.Services.AddControllers()
    .AddOData(options => options.Select().Filter().OrderBy().Count().SetMaxTop(100).Expand().Filter())
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.ReferenceHandler =
            ReferenceHandler.IgnoreCycles;
    });
builder.Services.Configure<FormOptions>(options =>
{
    options.MultipartBodyLengthLimit = 104857600; // 100MB
});
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

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
builder.Services.AddScoped<IXpConfigService, XpConfigService>();
builder.Services.AddScoped<IUserChallengeRepository, UserChallengeRepository>();
builder.Services.AddScoped<IChallengeTaskRepository, ChallengeTaskRepository>();
builder.Services.AddScoped<IUserConfigRepository, UserConfigRepository>();
builder.Services.Configure<JwtSettings>(builder.Configuration.GetSection("Jwt"));

var connectionString = Environment.GetEnvironmentVariable("DATABASE_URL")
                       ?? builder.Configuration.GetConnectionString("ZenGardenDB");

if (string.IsNullOrEmpty(connectionString))
    throw new InvalidOperationException("Database connection string is missing.");

builder.Services.AddDbContext<ZenGardenContext>(options =>
    options.UseMySql(connectionString, ServerVersion.AutoDetect(connectionString),
        x => x.UseQuerySplittingBehavior(QuerySplittingBehavior.SplitQuery))
);

var jwtSettings = builder.Configuration.GetSection("Jwt").Get<JwtSettings>();
if (jwtSettings == null || string.IsNullOrEmpty(jwtSettings.Key))
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
            ValidateIssuerSigningKey = true,
            ValidIssuer = jwtSettings.Issuer,
            ValidAudience = jwtSettings.Audience,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSettings.Key)),
            ClockSkew = TimeSpan.Zero
        };
    });

builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo { Title = "ZenGarden API", Version = "v1" });


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


builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll",
        policy =>
        {
            policy.WithOrigins("http://localhost:5173", "https://localhost:7262", "http://localhost:5153", "https://zengarden-fe.vercel.app")
                .AllowAnyMethod()
                .AllowAnyHeader()
                .AllowCredentials();
        });
});

builder.Services.AddMemoryCache();
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
builder.Services.AddScoped<IEmailService, EmailService>();
builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();
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

builder.Services.AddDataProtection()
    .PersistKeysToDbContext<ZenGardenContext>();

builder.Services.AddControllers()
    .AddFluentValidation(fv => { fv.RegisterValidatorsFromAssemblyContaining<LoginValidator>(); });
builder.Services.AddScoped<IXpConfigService, XpConfigService>();
builder.Services.AddHostedService<AutoPauseTaskJob>();
builder.Services.AddHostedService<OverdueTaskJob>();

builder.Services.AddSingleton<IProcessingStrategy, AsyncKeyLockProcessingStrategy>();


builder.Services.AddAutoMapper(typeof(MappingProfile));
builder.Services.Configure<OpenAiSettings>(builder.Configuration.GetSection("OpenAI"));
builder.Services.AddHttpClient<FocusMethodRepository>();

var deepInfraApiKey = Environment.GetEnvironmentVariable("DEEPINFRA_API_KEY")
                      ?? builder.Configuration["DeepInfra:ApiKey"];

if (string.IsNullOrEmpty(deepInfraApiKey))
    throw new InvalidOperationException("DeepInfra API Key is missing.");

builder.Services.Configure<OpenAiSettings>(options => { options.ApiKey = deepInfraApiKey; });
builder.Services.AddSignalR();
builder.Services.AddSingleton<IUserIdProvider, CustomUserIdProvider>();
builder.Services.AddScoped<INotificationService, NotificationService>();
builder.Services.AddSingleton<RealtimeBackgroundService>();
builder.Services.AddHostedService(provider => provider.GetRequiredService<RealtimeBackgroundService>());

builder.Logging.AddConsole();
builder.Logging.SetMinimumLevel(LogLevel.Debug);
builder.Services.Configure<AWSOptions>(builder.Configuration.GetSection("AWS"));
var app = builder.Build();

// Configure the HTTP request pipeline.
app.UseDeveloperExceptionPage();
app.UseSwagger();
app.UseSwaggerUI();

app.UseCors("AllowAll");
app.MapHub<NotificationHub>("/hubs/notification");
app.UseRouting();
app.UseMiddleware<ExceptionHandlingMiddleware>();
app.UseMiddleware<JwtMiddleware>();
app.UseMiddleware<UserContextMiddleware>();
app.UseMiddleware<PerformanceMiddleware>();

app.UseMiddleware<LoggingMiddleware>();
app.UseMiddleware<ValidationMiddleware>();
app.UseIpRateLimiting();

app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();

app.Run();

public partial class Program
{
}