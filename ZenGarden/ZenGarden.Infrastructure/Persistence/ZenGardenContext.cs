using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using ZenGarden.Domain.Entities;
using ZenGarden.Domain.Enums;

namespace ZenGarden.Infrastructure.Persistence;

public class ZenGardenContext : DbContext
{
    public ZenGardenContext()
    {
    }

    public ZenGardenContext(DbContextOptions<ZenGardenContext> options)
        : base(options)
    {
    }

    public virtual DbSet<Bag> Bag { get; set; }
    public virtual DbSet<Item> Item { get; set; }
    public virtual DbSet<BagItem> BagItem { get; set; }
    public virtual DbSet<Challenge> Challenge { get; set; }
    public virtual DbSet<ChallengeTask> ChallengeTask { get; set; }
    public virtual DbSet<ChallengeType> ChallengeType { get; set; }
    public virtual DbSet<FocusMethod> FocusMethod { get; set; }
    public virtual DbSet<ItemDetail> ItemDetail { get; set; }
    public virtual DbSet<Packages> Packages { get; set; }
    public virtual DbSet<PurchaseHistory> PurchaseHistory { get; set; }
    public virtual DbSet<Roles> Roles { get; set; }
    public virtual DbSet<XpConfig> XpConfigs { get; set; }
    public virtual DbSet<Tasks> Tasks { get; set; }
    public virtual DbSet<TaskType> TaskType { get; set; }
    public virtual DbSet<TradeHistory> TradeHistory { get; set; }
    public virtual DbSet<Transactions> Transactions { get; set; }
    public virtual DbSet<Tree> Tree { get; set; }
    public virtual DbSet<TreeXpConfig> TreeXpConfig { get; set; }
    public virtual DbSet<TreeXpLog> TreeXpLog { get; set; }
    public virtual DbSet<UserChallenge> UserChallenges { get; set; }
    public virtual DbSet<UserConfig> UserConfig { get; set; }
    public virtual DbSet<UserExperience> UserExperience { get; set; }
    public virtual DbSet<Users> Users { get; set; }
    public virtual DbSet<UserTree> UserTree { get; set; }
    public virtual DbSet<UserXpConfig> UserXpConfig { get; set; }
    public virtual DbSet<UserXpLog> UserXpLog { get; set; }
    public virtual DbSet<Wallet> Wallet { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        if (!optionsBuilder.IsConfigured)
        {
            var connectionString = GetConnectionString();
            optionsBuilder.UseMySql(connectionString, ServerVersion.AutoDetect(connectionString));
        }
    }

    private static string GetConnectionString()
    {
        var environment = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") ?? "Production";

        var configuration = new ConfigurationBuilder()
            .SetBasePath(Path.Combine(Directory.GetCurrentDirectory(), "../ZenGarden.API"))
            .AddJsonFile($"appsettings.{environment}.json", false, true)
            .Build();

        return configuration.GetConnectionString("ZenGardenDB") ??
               throw new InvalidOperationException("Connection string 'ZenGardenDB' not found.");
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder
            .UseCollation("utf8mb4_general_ci")
            .HasCharSet("utf8mb4");

        modelBuilder.Entity<Bag>(entity =>
        {
            entity.HasKey(e => e.BagId).HasName("PRIMARY");

            entity.Property(e => e.BagId)
                .ValueGeneratedOnAdd()
                .HasColumnName("BagID");

            entity.Property(e => e.UserId)
                .HasColumnName("UserID");

            entity.Property(e => e.CreatedAt)
                .HasColumnType("timestamp")
                .ValueGeneratedOnAdd()
                .HasDefaultValueSql("CURRENT_TIMESTAMP");
            entity.Property(e => e.UpdatedAt)
                .HasColumnType("timestamp")
                .ValueGeneratedOnUpdate()
                .HasDefaultValueSql("CURRENT_TIMESTAMP ON UPDATE CURRENT_TIMESTAMP");

            modelBuilder.Entity<Bag>()
                .HasOne(b => b.User)
                .WithOne(u => u.Bag)
                .HasForeignKey<Bag>(b => b.UserId)
                .OnDelete(DeleteBehavior.Cascade);
        });


        modelBuilder.Entity<BagItem>(entity =>
        {
            entity.HasKey(e => e.BagItemId).HasName("PRIMARY");

            entity.HasIndex(e => e.BagId, "BagID");

            entity.HasIndex(e => e.ItemId, "ItemID");

            entity.Property(e => e.BagItemId).HasColumnName("BagItemID").ValueGeneratedOnAdd();

            entity.Property(e => e.BagId).HasColumnName("BagID");
            entity.Property(e => e.ItemId).HasColumnName("ItemID");

            entity.HasOne(d => d.Bag).WithMany(p => p.BagItem)
                .HasForeignKey(d => d.BagId)
                .HasConstraintName("bagitem_ibfk_1");

            entity.HasOne(d => d.Item).WithMany(p => p.BagItem)
                .HasForeignKey(d => d.ItemId)
                .HasConstraintName("bagitem_ibfk_2");
        });

        modelBuilder.Entity<FocusMethod>(entity =>
        {
            entity.HasKey(e => e.FocusMethodId).HasName("PRIMARY");
            entity.Property(e => e.Name).HasMaxLength(100);
            entity.Property(e => e.Description).HasColumnType("text");
            entity.Property(e => e.FocusMethodId).HasColumnName("FocusMethodID").ValueGeneratedOnAdd();
            entity.Property(e => e.XpMultiplier)
                .HasColumnType("double")
                .HasDefaultValue(1);
            entity.Property(e => e.DefaultDuration)
                .HasColumnType("int")
                .HasDefaultValue(25);
            entity.Property(e => e.DefaultBreak)
                .HasColumnType("int")
                .HasDefaultValue(5);
            entity.Property(e => e.MinDuration)
                .HasColumnType("int")
                .HasDefaultValue(25);
            entity.Property(e => e.MaxDuration)
                .HasColumnType("int")
                .HasDefaultValue(60);
            entity.Property(e => e.MinBreak)
                .HasColumnType("int")
                .HasDefaultValue(5);
            entity.Property(e => e.MaxBreak)
                .HasColumnType("int")
                .HasDefaultValue(15);
            entity.Property(e => e.IsActive)
                .HasColumnType("bit")
                .HasDefaultValue(true);
            entity.Property(e => e.CreatedAt)
                .HasColumnType("timestamp")
                .ValueGeneratedOnAdd()
                .HasDefaultValueSql("CURRENT_TIMESTAMP");
            entity.Property(e => e.UpdatedAt)
                .HasColumnType("timestamp")
                .ValueGeneratedOnUpdate()
                .HasDefaultValueSql("CURRENT_TIMESTAMP ON UPDATE CURRENT_TIMESTAMP");
            entity.HasMany(d => d.Tasks)
                .WithOne(t => t.FocusMethod)
                .HasForeignKey(t => t.FocusMethodId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<Challenge>(entity =>
        {
            entity.HasKey(e => e.ChallengeId).HasName("PRIMARY");

            entity.Property(e => e.ChallengeId).HasColumnName("ChallengeID").ValueGeneratedOnAdd();

            entity.Property(e => e.ChallengeTypeId).HasColumnName("ChallengeTypeID");

            entity.Property(e => e.ChallengeName).HasMaxLength(255);

            entity.Property(e => e.Reward).HasColumnType("int");

            entity.Property(e => e.Status)
                .HasConversion<int>()
                .IsRequired();
            entity.Property(e => e.Description)
                .HasColumnType("varchar(1000)")
                .HasMaxLength(1000)
                .IsUnicode(false);

            entity.Property(e => e.CreatedAt)
                .HasColumnType("timestamp")
                .ValueGeneratedOnAdd()
                .HasDefaultValueSql("CURRENT_TIMESTAMP");
            entity.Property(e => e.UpdatedAt)
                .HasColumnType("timestamp")
                .ValueGeneratedOnUpdate()
                .HasDefaultValueSql("CURRENT_TIMESTAMP ON UPDATE CURRENT_TIMESTAMP");

            entity.HasOne(d => d.ChallengeType)
                .WithMany(p => p.Challenges)
                .HasForeignKey(d => d.ChallengeTypeId)
                .OnDelete(DeleteBehavior.Cascade)
                .HasConstraintName("challenge_ibfk_1");
        });

        modelBuilder.Entity<ChallengeTask>(entity =>
        {
            entity.HasKey(e => e.ChallengeTaskId).HasName("PRIMARY");

            entity.Property(e => e.ChallengeTaskId).HasColumnName("ChallengeTaskID").ValueGeneratedOnAdd();

            entity.Property(e => e.ChallengeId).HasColumnName("ChallengeID");

            entity.Property(e => e.TaskId).HasColumnName("TaskID");

            entity.Property(e => e.CreatedAt)
                .HasColumnType("timestamp")
                .ValueGeneratedOnAdd()
                .HasDefaultValueSql("CURRENT_TIMESTAMP");
            entity.Property(e => e.UpdatedAt)
                .HasColumnType("timestamp")
                .ValueGeneratedOnUpdate()
                .HasDefaultValueSql("CURRENT_TIMESTAMP ON UPDATE CURRENT_TIMESTAMP");

            entity.HasOne(ct => ct.Challenge)
                .WithMany(c => c.ChallengeTasks)
                .HasForeignKey(ct => ct.ChallengeId)
                .OnDelete(DeleteBehavior.Cascade)
                .HasConstraintName("challengetask_ibfk_1");
            entity.HasOne(d => d.Tasks)
                .WithMany(p => p.ChallengeTasks)
                .HasForeignKey(d => d.ChallengeId)
                .OnDelete(DeleteBehavior.NoAction);
        });

        modelBuilder.Entity<ChallengeType>(entity =>
        {
            entity.HasKey(e => e.ChallengeTypeId).HasName("PK_ChallengeType");

            entity.Property(e => e.ChallengeTypeId)
                .HasColumnName("ChallengeTypeID")
                .ValueGeneratedOnAdd();

            entity.Property(e => e.ChallengeTypeName)
                .HasMaxLength(255)
                .HasColumnName("ChallengeTypeName");

            entity.Property(e => e.CreatedAt)
                .HasColumnType("timestamp")
                .HasDefaultValueSql("CURRENT_TIMESTAMP");

            entity.Property(e => e.UpdatedAt)
                .HasColumnType("timestamp")
                .HasDefaultValueSql("CURRENT_TIMESTAMP ON UPDATE CURRENT_TIMESTAMP");
            modelBuilder.Entity<ChallengeType>()
                .HasMany(ct => ct.Challenges)
                .WithOne(c => c.ChallengeType)
                .HasForeignKey(c => c.ChallengeTypeId)
                .OnDelete(DeleteBehavior.Cascade);
        });


        modelBuilder.Entity<Item>(entity =>
        {
            entity.HasKey(e => e.ItemId).HasName("PRIMARY");

            entity.HasIndex(e => new { e.Type, e.Rarity }, "idx_item_type_rarity");

            entity.Property(e => e.ItemId).HasColumnName("ItemID").ValueGeneratedOnAdd();
            entity.Property(e => e.Cost).HasPrecision(10, 2);
            entity.Property(e => e.CreatedAt)
                .HasColumnType("timestamp")
                .ValueGeneratedOnAdd()
                .HasDefaultValueSql("CURRENT_TIMESTAMP");
            entity.Property(e => e.UpdatedAt)
                .HasColumnType("timestamp")
                .ValueGeneratedOnUpdate()
                .HasDefaultValueSql("CURRENT_TIMESTAMP ON UPDATE CURRENT_TIMESTAMP");
            entity.Property(e => e.Name).HasMaxLength(100);
            entity.Property(e => e.Rarity).HasMaxLength(50);
            entity.Property(e => e.Type).HasMaxLength(50);

            entity.Property(e => e.Status)
                .HasConversion<int>()
                .IsRequired();
        });

        modelBuilder.Entity<ItemDetail>(entity =>
        {
            entity.HasKey(e => e.ItemDetailId).HasName("PRIMARY");

            entity.Property(e => e.ItemDetailId).ValueGeneratedOnAdd().HasColumnName("ItemDetailID");

            entity.Property(e => e.ItemId)
                .HasColumnName("ItemID");

            entity.Property(e => e.Description).HasColumnType("text");
            entity.Property(e => e.Type).HasMaxLength(50);
            entity.Property(e => e.MediaUrl).HasMaxLength(255);
            entity.Property(e => e.Effect).HasColumnType("text");
            entity.Property(e => e.Duration).HasColumnType("text");
            entity.Property(e => e.CreatedAt)
                .HasColumnType("timestamp")
                .ValueGeneratedOnAdd()
                .HasDefaultValueSql("CURRENT_TIMESTAMP");
            entity.Property(e => e.UpdatedAt)
                .HasColumnType("timestamp")
                .ValueGeneratedOnUpdate()
                .HasDefaultValueSql("CURRENT_TIMESTAMP ON UPDATE CURRENT_TIMESTAMP");
            entity.Property(e => e.Sold).HasColumnType("int");
            entity.HasOne(d => d.Item)
                .WithOne(p => p.ItemDetail)
                .HasForeignKey<ItemDetail>(d => d.ItemId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<Packages>(entity =>
        {
            entity.HasKey(e => e.PackageId).HasName("PRIMARY");

            entity.Property(e => e.PackageId).HasColumnName("PackageID");

            entity.Property(e => e.Name)
                .IsRequired()
                .HasMaxLength(255);

            entity.Property(e => e.Price)
                .HasPrecision(10, 2);

            entity.Property(e => e.Amount)
                .HasPrecision(10, 2);

            entity.Property(e => e.IsActive)
                .HasDefaultValue(true);

            entity.Property(e => e.CreatedAt)
                .HasColumnType("timestamp")
                .HasDefaultValueSql("CURRENT_TIMESTAMP");

            entity.Property(e => e.UpdatedAt)
                .HasColumnType("timestamp")
                .HasDefaultValueSql("CURRENT_TIMESTAMP ON UPDATE CURRENT_TIMESTAMP");
        });

        modelBuilder.Entity<PurchaseHistory>(entity =>
        {
            entity.HasKey(e => e.PurchaseId).HasName("PRIMARY");

            entity.HasIndex(e => e.ItemId, "ItemID2");

            entity.HasIndex(e => e.UserId, "UserID2");

            entity.Property(e => e.PurchaseId).HasColumnName("PurchaseID");
            entity.Property(e => e.CreatedAt)
                .HasColumnType("timestamp")
                .HasDefaultValueSql("CURRENT_TIMESTAMP");
            entity.Property(e => e.ItemId).HasColumnName("ItemID");
            entity.Property(e => e.Status).HasMaxLength(20);
            entity.Property(e => e.TotalPrice).HasPrecision(10, 2);
            entity.Property(e => e.UserId).HasColumnName("UserID");

            entity.HasOne(d => d.Item).WithMany(p => p.PurchaseHistory)
                .HasForeignKey(d => d.ItemId)
                .HasConstraintName("purchasehistory_ibfk_2");

            entity.HasOne(d => d.User).WithMany(p => p.PurchaseHistory)
                .HasForeignKey(d => d.UserId)
                .HasConstraintName("purchasehistory_ibfk_1");
        });

        modelBuilder.Entity<Roles>(entity =>
        {
            entity.HasKey(e => e.RoleId).HasName("PRIMARY");

            entity.Property(e => e.RoleId).HasColumnName("RoleID");
            entity.Property(e => e.RoleName).HasMaxLength(50);
        });

        modelBuilder.Entity<Tasks>(entity =>
        {
            entity.HasKey(e => e.TaskId).HasName("PRIMARY");

            entity.Property(e => e.TaskId)
                .HasColumnName("TaskID")
                .ValueGeneratedOnAdd();

            entity.Property(e => e.TaskTypeId)
                .HasColumnName("TaskTypeID");

            entity.Property(e => e.UserTreeId)
                .HasColumnName("UserTreeID");

            entity.Property(e => e.FocusMethodId)
                .HasColumnName("FocusMethodID");

            entity.Property(e => e.TaskName)
                .HasMaxLength(255)
                .IsRequired();

            entity.Property(e => e.TaskDescription)
                .HasMaxLength(1000);

            entity.Property(e => e.TaskNote)
                .HasMaxLength(500);

            entity.Property(e => e.TaskResult)
                .HasMaxLength(2083)
                .HasDefaultValue(string.Empty);

            entity.Property(e => e.WorkDuration)
                .HasDefaultValue(25);

            entity.Property(e => e.TotalDuration)
                .HasColumnType("int")
                .HasDefaultValue(0);

            entity.Property(e => e.CreatedAt)
                .HasColumnType("timestamp")
                .ValueGeneratedOnAdd()
                .HasDefaultValueSql("CURRENT_TIMESTAMP");

            entity.Property(e => e.UpdatedAt)
                .HasColumnType("timestamp")
                .ValueGeneratedOnUpdate()
                .HasDefaultValueSql("CURRENT_TIMESTAMP ON UPDATE CURRENT_TIMESTAMP");

            entity.Property(e => e.StartedAt)
                .HasColumnType("timestamp");

            entity.Property(e => e.CompletedAt)
                .HasColumnType("timestamp");

            entity.Property(e => e.StartDate)
                .HasColumnType("timestamp");

            entity.Property(e => e.EndDate)
                .HasColumnType("timestamp");

            entity.Property(e => e.Status)
                .HasConversion<int>()
                .IsRequired();

            entity.Property(e => e.BreakTime)
                .HasDefaultValue(5);

            entity.Property(e => e.IsSuggested)
                .HasDefaultValue(true)
                .IsRequired();

            entity.HasOne(d => d.FocusMethod)
                .WithMany(p => p.Tasks)
                .HasForeignKey(d => d.FocusMethodId)
                .OnDelete(DeleteBehavior.SetNull);

            entity.HasOne(d => d.TaskType)
                .WithMany(p => p.Tasks)
                .HasForeignKey(d => d.TaskTypeId)
                .OnDelete(DeleteBehavior.SetNull);

            entity.HasOne(d => d.UserTree)
                .WithMany(p => p.Tasks)
                .HasForeignKey(d => d.UserTreeId)
                .OnDelete(DeleteBehavior.SetNull);
        });


        modelBuilder.Entity<TaskType>(entity =>
        {
            entity.HasKey(e => e.TaskTypeId).HasName("PRIMARY");

            entity.Property(e => e.TaskTypeId).HasColumnName("TaskTypeID");

            entity.Property(e => e.TaskTypeName)
                .IsRequired()
                .HasMaxLength(100);

            entity.Property(e => e.TaskTypeDescription)
                .HasMaxLength(255);
            entity.Property(e => e.XpMultiplier)
                .HasColumnType("double")
                .HasDefaultValue(1);
            entity.Property(e => e.CreatedAt)
                .HasColumnType("timestamp")
                .ValueGeneratedOnAdd()
                .HasDefaultValueSql("CURRENT_TIMESTAMP");

            entity.Property(e => e.UpdatedAt)
                .HasColumnType("timestamp")
                .ValueGeneratedOnUpdate()
                .HasDefaultValueSql("CURRENT_TIMESTAMP ON UPDATE CURRENT_TIMESTAMP");

            entity.HasMany(e => e.Tasks)
                .WithOne(t => t.TaskType)
                .HasForeignKey(t => t.TaskTypeId)
                .OnDelete(DeleteBehavior.Cascade);
            entity.HasMany(e => e.XpConfigs)
                .WithOne(x => x.TaskType)
                .HasForeignKey(x => x.TaskTypeId)
                .OnDelete(DeleteBehavior.Cascade);
        });


        modelBuilder.Entity<TradeHistory>(entity =>
        {
            entity.HasKey(e => e.TradeId).HasName("PRIMARY");

            entity.HasIndex(e => e.TreeAid, "TreeAid");

            entity.HasIndex(e => e.TreeOwnerAid, "TreeOwnerAID");

            entity.HasIndex(e => e.TreeOwnerBid, "TreeOwnerBID");

            entity.HasIndex(e => e.DesiredTreeAID, "DesiredTreeAID");

            entity.Property(e => e.TradeId).HasColumnName("TradeID");

            entity.Property(e => e.Status)
                  .HasConversion<int>()
                  .IsRequired();

            entity.Property(e => e.CompletedAt)
                  .HasColumnType("timestamp")
                  .HasDefaultValueSql("CURRENT_TIMESTAMP ON UPDATE CURRENT_TIMESTAMP");

            entity.Property(e => e.RequestedAt)
                  .HasColumnType("timestamp")
                  .HasDefaultValueSql("CURRENT_TIMESTAMP");
            entity.Property(e => e.TradeFee)
                  .HasPrecision(10, 2)
                  .HasDefaultValueSql("'0.00'");

            entity.HasOne(th => th.TreeA)
                  .WithMany(th => th.TradeHistoryTreeA)
                  .HasForeignKey(th => th.TreeAid)
                  .OnDelete(DeleteBehavior.SetNull);

            // Desired Tree
            entity.HasOne(th => th.DesiredTree)
                  .WithMany(th => th.TradeHistoryDesiredTree)
                  .HasForeignKey(th => th.DesiredTreeAID)
                  .OnDelete(DeleteBehavior.SetNull);

            entity.HasOne(th => th.TreeOwnerA)
                  .WithMany(u => u.TradeHistoryUserA) // Nếu Users.cs có ICollection
                  .HasForeignKey(th => th.TreeOwnerAid)
                  .HasConstraintName("FK_TradeHistory_TreeOwnerA")
                  .OnDelete(DeleteBehavior.Restrict);

            // ✅ Map TreeOwnerB → Users.UserId
            entity.HasOne(th => th.TreeOwnerB)
                  .WithMany(u => u.TradeHistoryUserB) // Nếu Users.cs có ICollection
                  .HasForeignKey(th => th.TreeOwnerBid)
                  .HasConstraintName("FK_TradeHistory_TreeOwnerB")
                  .OnDelete(DeleteBehavior.Restrict);


        });

        modelBuilder.Entity<Transactions>(entity =>
        {
            entity.HasKey(e => e.TransactionId).HasName("PRIMARY");

            entity.Property(e => e.TransactionId).HasColumnName("TransactionID");
            entity.Property(e => e.Amount).HasPrecision(10, 2);

            entity.Property(e => e.UserId).HasColumnName("UserID");
            entity.Property(e => e.WalletId).HasColumnName("WalletID");
            entity.Property(e => e.PackageId).HasColumnName("PackageID");

            entity.Property(e => e.Type)
                .HasConversion<int>()
                .IsRequired();

            entity.Property(e => e.Status)
                .HasConversion<int>()
                .IsRequired();


            entity.Property(e => e.CreatedAt)
                .HasColumnType("timestamp")
                .HasDefaultValueSql("CURRENT_TIMESTAMP");

            entity.Property(e => e.TransactionTime)
                .HasColumnType("timestamp")
                .HasDefaultValueSql("CURRENT_TIMESTAMP ON UPDATE CURRENT_TIMESTAMP");

            entity.Property(e => e.PaymentMethod).HasMaxLength(50);
            entity.Property(e => e.TransactionRef).HasMaxLength(255);

            entity.HasIndex(e => e.UserId, "idx_transaction_user");
            entity.HasIndex(e => e.WalletId, "idx_transaction_wallet");
            entity.HasIndex(e => e.PackageId, "idx_transaction_package");

            entity.HasOne(d => d.User)
                .WithMany(p => p.Transactions)
                .HasForeignKey(d => d.UserId)
                .HasConstraintName("transactions_ibfk_1");

            entity.HasOne(d => d.Wallet)
                .WithMany(p => p.Transactions)
                .HasForeignKey(d => d.WalletId)
                .HasConstraintName("transactions_ibfk_2");

            entity.HasOne(d => d.Package)
                .WithMany()
                .HasForeignKey(d => d.PackageId)
                .OnDelete(DeleteBehavior.SetNull)
                .HasConstraintName("transactions_ibfk_3");
        });

        modelBuilder.Entity<Tree>(entity =>
        {
            entity.HasKey(e => e.TreeId).HasName("PRIMARY");

            entity.Property(e => e.TreeId)
                .ValueGeneratedOnAdd()
                .HasColumnName("TreeID");

            entity.Property(e => e.Name)
                .HasColumnName("Name")
                .HasMaxLength(100);

            entity.Property(e => e.Rarity)
                .HasColumnName("Rarity")
                .HasMaxLength(100);

            entity.Property(e => e.IsActive)
                .HasDefaultValue(true);

            entity.Property(e => e.CreatedAt)
                .HasColumnType("timestamp")
                .ValueGeneratedOnAdd()
                .HasDefaultValueSql("CURRENT_TIMESTAMP");
            entity.Property(e => e.UpdatedAt)
                .HasColumnType("timestamp")
                .ValueGeneratedOnUpdate()
                .HasDefaultValueSql("CURRENT_TIMESTAMP ON UPDATE CURRENT_TIMESTAMP");
        });

        modelBuilder.Entity<TreeXpConfig>(entity =>
        {
            entity.HasKey(e => e.LevelId).HasName("PRIMARY");

            entity.Property(e => e.LevelId)
                .HasColumnName("LevelID")
                .ValueGeneratedOnAdd();

            entity.Property(e => e.XpThreshold)
                .HasColumnType("int")
                .HasColumnName("XpThreshold");

            entity.Property(e => e.CreatedAt)
                .HasColumnType("timestamp")
                .HasDefaultValueSql("CURRENT_TIMESTAMP");

            entity.Property(e => e.UpdatedAt)
                .HasColumnType("timestamp")
                .HasDefaultValueSql("CURRENT_TIMESTAMP ON UPDATE CURRENT_TIMESTAMP");
        });
        modelBuilder.Entity<TreeXpLog>(entity =>
        {
            entity.HasKey(e => e.LogId).HasName("PRIMARY");

            entity.Property(e => e.LogId)
                .ValueGeneratedOnAdd()
                .HasColumnName("LogID");


            entity.Property(e => e.TaskId)
                .HasColumnName("TaskID")
                .IsRequired(false);

            entity.Property(e => e.ActivityType)
                .HasConversion<int>()
                .HasColumnName("ActivityType");

            entity.Property(e => e.XpAmount)
                .HasColumnType("double")
                .HasColumnName("XpAmount");

            entity.Property(e => e.CreatedAt)
                .HasColumnType("timestamp")
                .HasDefaultValueSql("CURRENT_TIMESTAMP");

            entity.Property(e => e.UpdatedAt)
                .HasColumnType("timestamp")
                .HasDefaultValueSql("CURRENT_TIMESTAMP ON UPDATE CURRENT_TIMESTAMP");


            entity.HasOne(t => t.Tasks)
                .WithMany(task => task.TreeXpLog)
                .IsRequired()
                .HasForeignKey(t => t.TaskId)
                .OnDelete(DeleteBehavior.SetNull);
        });

        modelBuilder.Entity<UserChallenge>(entity =>
        {
            entity.HasKey(e => e.UserChallengeId).HasName("PRIMARY");

            entity.Property(e => e.UserChallengeId).HasColumnName("UserChallengeID");
            entity.Property(e => e.ChallengeId).HasColumnName("ChallengeID");
            entity.Property(e => e.UserId).HasColumnName("UserID");
            entity.Property(e => e.Progress).HasColumnType("int").HasDefaultValue(0);
            entity.Property(e => e.Status)
                .HasConversion<int>()
                .HasColumnName("Status")
                .IsRequired();
            entity.Property(e => e.ChallengeRole)
                .HasConversion<int>()
                .HasColumnName("ChallengeRole")
                .IsRequired();
            entity.Property(e => e.JoinedAt)
                .HasColumnType("timestamp")
                .HasDefaultValueSql("CURRENT_TIMESTAMP");
            entity.Property(e => e.CreatedAt)
                .HasColumnType("timestamp")
                .HasDefaultValueSql("CURRENT_TIMESTAMP");
            entity.Property(e => e.UpdatedAt)
                .HasColumnType("timestamp")
                .HasDefaultValueSql("CURRENT_TIMESTAMP ON UPDATE CURRENT_TIMESTAMP");

            entity.HasOne(uc => uc.Challenge)
                .WithMany(c => c.UserChallenges)
                .HasForeignKey(uc => uc.ChallengeId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(uc => uc.User)
                .WithMany(t => t.UserChallenges)
                .HasForeignKey(uc => uc.UserId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<UserConfig>(entity =>
        {
            entity.HasKey(e => e.UserConfigId).HasName("PRIMARY");

            entity.Property(e => e.UserConfigId)
                .ValueGeneratedOnAdd()
                .HasColumnName("UserConfigID");
            entity.Property(e => e.ImageUrl)
                .IsRequired()
                .HasMaxLength(255);

            entity.Property(e => e.UserId).HasColumnName("UserID");

            entity.Property(e => e.BackgroundConfig)
                .HasMaxLength(255)
                .HasColumnName("BackgroundConfig");

            entity.Property(e => e.SoundConfig)
                .HasMaxLength(255)
                .HasColumnName("SoundConfig");

            entity.Property(e => e.CreatedAt)
                .HasColumnType("timestamp")
                .HasDefaultValueSql("CURRENT_TIMESTAMP");

            entity.Property(e => e.UpdatedAt)
                .HasColumnType("timestamp")
                .HasDefaultValueSql("CURRENT_TIMESTAMP ON UPDATE CURRENT_TIMESTAMP");
            entity.HasOne(uc => uc.User)
                .WithOne(u => u.UserConfig)
                .HasForeignKey<UserConfig>(uc => uc.UserId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<UserExperience>(entity =>
        {
            entity.HasKey(e => e.UserExperienceId).HasName("PRIMARY");

            entity.Property(e => e.UserExperienceId)
                .ValueGeneratedOnAdd()
                .HasColumnName("UserExperienceID");

            entity.Property(e => e.UserId)
                .HasColumnName("UserID");

            entity.Property(e => e.TotalXp).HasColumnName("TotalXP");

            entity.Property(e => e.XpToNextLevel)
                .HasColumnName("XpToNextLevel");

            entity.Property(e => e.LevelId)
                .HasColumnName("LevelID");

            entity.Property(e => e.TotalXp)
                .HasColumnType("double")
                .HasDefaultValue(0)
                .IsRequired();

            entity.Property(e => e.IsMaxLevel)
                .HasColumnType("bit")
                .HasDefaultValue(false)
                .IsRequired();

            entity.Property(e => e.StreakDays)
                .HasColumnName("StreakDays");

            entity.Property(e => e.CreatedAt)
                .HasColumnType("timestamp")
                .ValueGeneratedOnAdd()
                .HasDefaultValueSql("CURRENT_TIMESTAMP");

            entity.Property(e => e.UpdatedAt)
                .HasColumnType("timestamp")
                .ValueGeneratedOnUpdate()
                .HasDefaultValueSql("CURRENT_TIMESTAMP ON UPDATE CURRENT_TIMESTAMP");

            entity.HasOne(e => e.User)
                .WithOne(u => u.UserExperience)
                .HasForeignKey<UserExperience>(e => e.UserId)
                .OnDelete(DeleteBehavior.Cascade);
            entity.HasOne(ue => ue.UserXpConfig)
                .WithMany(ux => ux.UserExperiences)
                .HasForeignKey(ue => ue.LevelId)
                .OnDelete(DeleteBehavior.Restrict);
        });


        modelBuilder.Entity<Users>(entity =>
        {
            entity.HasKey(e => e.UserId).HasName("PRIMARY");

            entity.HasIndex(e => e.Email, "Email").IsUnique();

            entity.HasIndex(e => e.RoleId, "RoleID");

            entity.Property(u => u.UserId)
                .HasColumnType("int")
                .ValueGeneratedOnAdd()
                .IsRequired();

            entity.Property(e => e.CreatedAt)
                .HasColumnType("timestamp")
                .HasDefaultValueSql("CURRENT_TIMESTAMP");

            entity.Property(e => e.UpdatedAt)
                .HasColumnType("timestamp")
                .HasDefaultValueSql("CURRENT_TIMESTAMP ON UPDATE CURRENT_TIMESTAMP");

            entity.Property(e => e.Email)
                .IsRequired()
                .HasMaxLength(100);

            entity.Property(e => e.UserName)
                .IsRequired()
                .HasMaxLength(100);

            entity.Property(e => e.Password)
                .IsRequired()
                .HasMaxLength(255);

            entity.Property(e => e.Phone)
                .IsRequired()
                .HasMaxLength(20);


            entity.Property(e => e.RoleId).HasColumnName("RoleID");

            entity.Property(e => e.Status)
                .HasConversion<int>()
                .IsRequired();


            entity.Property(e => e.RefreshTokenHash)
                .HasMaxLength(255)
                .IsUnicode(false);

            entity.Property(e => e.RefreshTokenExpiry)
                .HasColumnType("timestamp")
                .HasDefaultValueSql("CURRENT_TIMESTAMP");

            entity.Property(e => e.OtpCodeHash)
                .HasMaxLength(255)
                .IsUnicode(false);

            entity.Property(e => e.OtpExpiry)
                .HasColumnType("timestamp")
                .HasDefaultValueSql("CURRENT_TIMESTAMP");

            entity.HasOne(u => u.Bag)
                .WithOne(b => b.User)
                .HasForeignKey<Bag>(b => b.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(u => u.Wallet)
                .WithOne(w => w.User)
                .HasForeignKey<Wallet>(w => w.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(d => d.Role)
                .WithMany(p => p.Users)
                .HasForeignKey(d => d.RoleId)
                .HasConstraintName("users_ibfk_1");
            entity.HasMany(u => u.UserTree)
                .WithOne(ut => ut.User)
                .HasForeignKey(ut => ut.UserId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<UserTree>(entity =>
        {
            entity.HasKey(e => e.UserTreeId).HasName("PRIMARY");

            entity.Property(e => e.UserTreeId)
                .HasColumnName("UserTreeID")
                .ValueGeneratedOnAdd();

            entity.Property(e => e.UserId)
                .HasColumnName("UserID");

            entity.Property(e => e.TreeOwnerId)
                .HasColumnName("OwnerID");


            entity.Property(e => e.LevelId)
                .HasColumnName("LevelID");

            entity.Property(e => e.FinalTreeId)
                .HasColumnName("FinalTreeID");

            entity.Property(e => e.Name)
                .HasMaxLength(100)
                .IsRequired();

            entity.Property(e => e.TotalXp)
                .HasColumnType("double")
                .HasDefaultValue(0)
                .IsRequired();

            entity.Property(e => e.IsMaxLevel)
                .HasColumnType("bit")
                .HasDefaultValue(false)
                .IsRequired();

            entity.Property(e => e.TreeStatus)
                .HasConversion<int>()
                .HasDefaultValue(TreeStatus.Growing);

            entity.Property(e => e.CreatedAt)
                .HasColumnType("timestamp")
                .ValueGeneratedOnAdd()
                .HasDefaultValueSql("CURRENT_TIMESTAMP");

            entity.Property(e => e.UpdatedAt)
                .HasColumnType("timestamp")
                .ValueGeneratedOnAddOrUpdate()
                .HasDefaultValueSql("CURRENT_TIMESTAMP ON UPDATE CURRENT_TIMESTAMP");

            entity.HasOne(d => d.User)
                .WithMany(p => p.UserTree)
                .HasForeignKey(d => d.UserId)
                .OnDelete(DeleteBehavior.Cascade)
                .HasConstraintName("usertree_ibfk_1");

            entity.HasOne(d => d.FinalTree)
                .WithMany(t => t.UserTree)
                .HasForeignKey(d => d.FinalTreeId)
                .OnDelete(DeleteBehavior.SetNull)
                .HasConstraintName("usertree_ibfk_2");
            entity.HasOne(ut => ut.TreeOwner)
                .WithMany()
                .HasForeignKey(ut => ut.TreeOwnerId)
                .IsRequired()
                .OnDelete(DeleteBehavior.Cascade);


            entity.HasOne(d => d.TreeXpConfig)
                .WithMany(p => p.UserTrees)
                .HasForeignKey(d => d.LevelId)
                .OnDelete(DeleteBehavior.Restrict);
        });


        modelBuilder.Entity<UserXpConfig>(entity =>
        {
            entity.HasKey(e => e.LevelId).HasName("PRIMARY");
            entity.Property(e => e.LevelId).HasColumnName("LevelID");
            entity.Property(e => e.XpThreshold).HasColumnName("XpThreshold").HasColumnType("int").IsRequired();
            entity.Property(e => e.CreatedAt)
                .HasColumnType("timestamp")
                .HasDefaultValueSql("CURRENT_TIMESTAMP");
            entity.Property(e => e.UpdatedAt)
                .HasColumnType("timestamp")
                .HasDefaultValueSql("CURRENT_TIMESTAMP ON UPDATE CURRENT_TIMESTAMP");
        });

        modelBuilder.Entity<UserXpLog>(entity =>
        {
            entity.HasKey(e => e.LogId).HasName("PRIMARY");

            entity.Property(e => e.XpAmount)
                .HasColumnType("double")
                .HasDefaultValue(0)
                .HasColumnName("XpAmount");

            entity.Property(e => e.XpSource)
                .HasColumnType("int")
                .IsRequired()
                .HasColumnName("XpSource");

            entity.Property(e => e.CreatedAt)
                .HasColumnType("timestamp")
                .HasDefaultValueSql("CURRENT_TIMESTAMP");

            entity.Property(e => e.UpdatedAt)
                .HasColumnType("timestamp")
                .HasDefaultValueSql("CURRENT_TIMESTAMP ON UPDATE CURRENT_TIMESTAMP");

            entity.HasOne(xp => xp.User)
                .WithMany(u => u.UserXpLog)
                .HasForeignKey(xp => xp.UserId)
                .OnDelete(DeleteBehavior.Cascade);
        });


        modelBuilder.Entity<Wallet>(entity =>
        {
            entity.HasKey(e => e.WalletId).HasName("PRIMARY");

            entity.Property(e => e.WalletId)
                .ValueGeneratedOnAdd()
                .HasColumnName("WalletID");

            entity.Property(e => e.UserId)
                .HasColumnName("UserID");

            entity.Property(e => e.Balance)
                .HasPrecision(10, 2)
                .HasDefaultValueSql("'0.00'");

            entity.Property(e => e.UpdatedAt)
                .HasColumnType("timestamp")
                .HasDefaultValueSql("CURRENT_TIMESTAMP ON UPDATE CURRENT_TIMESTAMP");

            entity.HasOne(d => d.User)
                .WithOne(p => p.Wallet)
                .HasForeignKey<Wallet>(d => d.UserId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<XpConfig>(entity =>
        {
            entity.HasKey(e => e.XpConfigId).HasName("PRIMARY");

            entity.Property(e => e.XpConfigId)
                .HasColumnName("XPConfigID")
                .ValueGeneratedOnAdd();

            entity.Property(e => e.FocusMethodId)
                .HasColumnName("FocusMethodID");

            entity.Property(e => e.TaskTypeId)
                .HasColumnName("TaskTypeID");

            entity.Property(e => e.BaseXp)
                .HasColumnType("double");

            entity.Property(e => e.XpMultiplier)
                .HasColumnType("double")
                .HasDefaultValue(1);

            entity.Property(e => e.CreatedAt)
                .HasColumnType("timestamp")
                .ValueGeneratedOnAdd()
                .HasDefaultValueSql("CURRENT_TIMESTAMP");

            entity.Property(e => e.UpdatedAt)
                .HasColumnType("timestamp")
                .ValueGeneratedOnUpdate()
                .HasDefaultValueSql("CURRENT_TIMESTAMP ON UPDATE CURRENT_TIMESTAMP");

            entity.HasOne(d => d.FocusMethod)
                .WithMany(p => p.XPConfigs)
                .HasForeignKey(d => d.FocusMethodId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(d => d.TaskType)
                .WithMany(p => p.XpConfigs)
                .HasForeignKey(d => d.TaskTypeId)
                .OnDelete(DeleteBehavior.Cascade);
        });


        base.OnModelCreating(modelBuilder);
    }
}