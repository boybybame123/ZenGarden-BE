﻿// <auto-generated> This file has been auto generated by EF Core Power Tools. </auto-generated>
#nullable disable
using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Pomelo.EntityFrameworkCore.MySql.Scaffolding.Internal;
using ZenGarden.Domain.Entities;

namespace ZenGarden.Infrastructure.Persistence;

public partial class ZenGardenContext : DbContext
{
    public ZenGardenContext()
    {
    }

    public ZenGardenContext(DbContextOptions<ZenGardenContext> options)
        : base(options)
    {
    }

    public virtual DbSet<Bag> Bag { get; set; }

    public virtual DbSet<Bagitem> Bagitem { get; set; }

    public virtual DbSet<Dailyreward> Dailyreward { get; set; }

    public virtual DbSet<Dailyrewardclaim> Dailyrewardclaim { get; set; }

    public virtual DbSet<Deposittransaction> Deposittransaction { get; set; }

    public virtual DbSet<Focusmethod> Focusmethod { get; set; }

    public virtual DbSet<Item> Item { get; set; }

    public virtual DbSet<Itemdetail> Itemdetail { get; set; }

    public virtual DbSet<Leaderboard> Leaderboard { get; set; }

    public virtual DbSet<Purchasehistory> Purchasehistory { get; set; }

    public virtual DbSet<Roles> Roles { get; set; }

    public virtual DbSet<Tasks> Tasks { get; set; }

    public virtual DbSet<Taskstatus> Taskstatus { get; set; }

    public virtual DbSet<Tradehistory> Tradehistory { get; set; }

    public virtual DbSet<Tradestatus> Tradestatus { get; set; }

    public virtual DbSet<Transactions> Transactions { get; set; }

    public virtual DbSet<Treeprogress> Treeprogress { get; set; }

    public virtual DbSet<Treetype> Treetype { get; set; }

    public virtual DbSet<Useractivity> Useractivity { get; set; }

    public virtual DbSet<Userexperience> Userexperience { get; set; }

    public virtual DbSet<Users> Users { get; set; }

    public virtual DbSet<Usertree> Usertree { get; set; }

    public virtual DbSet<Wallet> Wallet { get; set; }

    public virtual DbSet<Workspace> Workspace { get; set; }

    public virtual DbSet<Workspaceitem> Workspaceitem { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        if (!optionsBuilder.IsConfigured)
        {
            var connectionString = GetConnectionString();
            optionsBuilder.UseMySql("server=localhost;database=zengarden;uid=root;pwd=10112003", Microsoft.EntityFrameworkCore.ServerVersion.Parse("9.2.0-mysql"));
            //optionsBuilder.UseMySql(connectionString, ServerVersion.AutoDetect(connectionString));
        }
    }

    private string GetConnectionString()
    {
        var configuration = new ConfigurationBuilder()
            .SetBasePath(Directory.GetCurrentDirectory())
            .AddJsonFile("appsettings.Development.json", optional: true, reloadOnChange: true)
            .Build();

        return configuration.GetConnectionString("ZenGardenDB");
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder
            .UseCollation("utf8mb4_0900_ai_ci")
            .HasCharSet("utf8mb4");

        modelBuilder.Entity<Bag>(entity =>
        {
            entity.HasKey(e => e.BagId).HasName("PRIMARY");

            entity.ToTable("bag");

            entity.HasIndex(e => e.UserId, "idx_bag_user");

            entity.Property(e => e.BagId).HasColumnName("BagID");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnType("timestamp");
            entity.Property(e => e.UpdatedAt)
                .ValueGeneratedOnAddOrUpdate()
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnType("timestamp");
            entity.Property(e => e.UserId).HasColumnName("UserID");

            entity.HasOne(d => d.User).WithMany(p => p.Bag)
                .HasForeignKey(d => d.UserId)
                .HasConstraintName("bag_ibfk_1");
        });

        modelBuilder.Entity<Bagitem>(entity =>
        {
            entity.HasKey(e => e.BagItemId).HasName("PRIMARY");

            entity.ToTable("bagitem");

            entity.HasIndex(e => e.BagId, "BagID");

            entity.HasIndex(e => e.ItemId, "ItemID");

            entity.Property(e => e.BagItemId).HasColumnName("BagItemID");
            entity.Property(e => e.AddedAt)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnType("timestamp");
            entity.Property(e => e.BagId).HasColumnName("BagID");
            entity.Property(e => e.ItemId).HasColumnName("ItemID");

            entity.HasOne(d => d.Bag).WithMany(p => p.Bagitem)
                .HasForeignKey(d => d.BagId)
                .HasConstraintName("bagitem_ibfk_1");

            entity.HasOne(d => d.Item).WithMany(p => p.Bagitem)
                .HasForeignKey(d => d.ItemId)
                .HasConstraintName("bagitem_ibfk_2");
        });

        modelBuilder.Entity<Dailyreward>(entity =>
        {
            entity.HasKey(e => e.DailyId).HasName("PRIMARY");

            entity.ToTable("dailyreward");

            entity.HasIndex(e => e.ItemId, "ItemID1");

            entity.Property(e => e.DailyId).HasColumnName("DailyID");
            entity.Property(e => e.ConditionType).HasMaxLength(50);
            entity.Property(e => e.ConditionValue).HasMaxLength(100);
            entity.Property(e => e.ItemId).HasColumnName("ItemID");
            entity.Property(e => e.Reward).HasMaxLength(100);
            entity.Property(e => e.RewardType).HasMaxLength(50);

            entity.HasOne(d => d.Item).WithMany(p => p.Dailyreward)
                .HasForeignKey(d => d.ItemId)
                .HasConstraintName("dailyreward_ibfk_1");
        });

        modelBuilder.Entity<Dailyrewardclaim>(entity =>
        {
            entity.HasKey(e => e.ClaimId).HasName("PRIMARY");

            entity.ToTable("dailyrewardclaim");

            entity.HasIndex(e => e.TaskId, "TaskID");

            entity.HasIndex(e => e.UserId, "idx_daily_reward_claim_user");

            entity.Property(e => e.ClaimId).HasColumnName("ClaimID");
            entity.Property(e => e.ClaimedAt)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnType("timestamp");
            entity.Property(e => e.DailyId).HasColumnName("DailyID");
            entity.Property(e => e.TaskId).HasColumnName("TaskID");
            entity.Property(e => e.UserId).HasColumnName("UserID");

            entity.HasOne(d => d.Task).WithMany(p => p.Dailyrewardclaim)
                .HasForeignKey(d => d.TaskId)
                .HasConstraintName("dailyrewardclaim_ibfk_2");

            entity.HasOne(d => d.User).WithMany(p => p.Dailyrewardclaim)
                .HasForeignKey(d => d.UserId)
                .HasConstraintName("dailyrewardclaim_ibfk_1");
        });

        modelBuilder.Entity<Deposittransaction>(entity =>
        {
            entity.HasKey(e => e.DepositId).HasName("PRIMARY");

            entity.ToTable("deposittransaction");

            entity.HasIndex(e => e.UserId, "UserID");

            entity.Property(e => e.DepositId).HasColumnName("DepositID");
            entity.Property(e => e.Amount).HasPrecision(10, 2);
            entity.Property(e => e.CompletedAt)
                .ValueGeneratedOnAddOrUpdate()
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnType("timestamp");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnType("timestamp");
            entity.Property(e => e.PaymentMethod).HasMaxLength(50);
            entity.Property(e => e.PaymentStatus).HasMaxLength(20);
            entity.Property(e => e.TransactionReference).HasMaxLength(100);
            entity.Property(e => e.UserId).HasColumnName("UserID");

            entity.HasOne(d => d.User).WithMany(p => p.Deposittransaction)
                .HasForeignKey(d => d.UserId)
                .HasConstraintName("deposittransaction_ibfk_1");
        });

        modelBuilder.Entity<Focusmethod>(entity =>
        {
            entity.HasKey(e => e.FocusMethodId).HasName("PRIMARY");

            entity.ToTable("focusmethod");

            entity.Property(e => e.FocusMethodId).HasColumnName("FocusMethodID");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnType("timestamp");
            entity.Property(e => e.Name).HasMaxLength(100);
        });

        modelBuilder.Entity<Item>(entity =>
        {
            entity.HasKey(e => e.ItemId).HasName("PRIMARY");

            entity.ToTable("item");

            entity.HasIndex(e => new { e.Type, e.Rarity }, "idx_item_type_rarity");

            entity.Property(e => e.ItemId).HasColumnName("ItemID");
            entity.Property(e => e.Cost).HasPrecision(10, 2);
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnType("timestamp");
            entity.Property(e => e.Name).HasMaxLength(100);
            entity.Property(e => e.Rarity).HasMaxLength(50);
            entity.Property(e => e.Type).HasMaxLength(50);
        });

        modelBuilder.Entity<Itemdetail>(entity =>
        {
            entity.HasKey(e => e.ItemDetailId).HasName("PRIMARY");

            entity.ToTable("itemdetail");

            entity.HasIndex(e => e.ItemId, "idx_item_detail_itemid").IsUnique();

            entity.Property(e => e.ItemDetailId).HasColumnName("ItemDetailID");
            entity.Property(e => e.Description).HasColumnType("text");
            entity.Property(e => e.DurationType).HasMaxLength(50);
            entity.Property(e => e.ImageUrl).HasMaxLength(255);
            entity.Property(e => e.ItemId).HasColumnName("ItemID");
            entity.Property(e => e.Requirements).HasColumnType("json");
            entity.Property(e => e.SpecialEffects).HasColumnType("text");
            entity.Property(e => e.Stats).HasColumnType("json");
            entity.Property(e => e.Tags).HasColumnType("json");
            entity.Property(e => e.UpdatedAt)
                .ValueGeneratedOnAddOrUpdate()
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnType("timestamp");

            entity.HasOne(d => d.Item).WithOne(p => p.Itemdetail)
                .HasForeignKey<Itemdetail>(d => d.ItemId)
                .OnDelete(DeleteBehavior.Cascade)
                .HasConstraintName("itemdetail_ibfk_1");
        });

        modelBuilder.Entity<Leaderboard>(entity =>
        {
            entity.HasKey(e => e.LeaderboardId).HasName("PRIMARY");

            entity.ToTable("leaderboard");

            entity.HasIndex(e => e.UserId, "UserID1");

            entity.Property(e => e.LeaderboardId).HasColumnName("LeaderboardID");
            entity.Property(e => e.LastUpdated)
                .ValueGeneratedOnAddOrUpdate()
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnType("timestamp");
            entity.Property(e => e.UserId).HasColumnName("UserID");

            entity.HasOne(d => d.User).WithMany(p => p.Leaderboard)
                .HasForeignKey(d => d.UserId)
                .HasConstraintName("leaderboard_ibfk_1");
        });

        modelBuilder.Entity<Purchasehistory>(entity =>
        {
            entity.HasKey(e => e.PurchaseId).HasName("PRIMARY");

            entity.ToTable("purchasehistory");

            entity.HasIndex(e => e.ItemId, "ItemID2");

            entity.HasIndex(e => e.UserId, "UserID2");

            entity.Property(e => e.PurchaseId).HasColumnName("PurchaseID");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnType("timestamp");
            entity.Property(e => e.ItemId).HasColumnName("ItemID");
            entity.Property(e => e.Status).HasMaxLength(20);
            entity.Property(e => e.TotalPrice).HasPrecision(10, 2);
            entity.Property(e => e.UserId).HasColumnName("UserID");

            entity.HasOne(d => d.Item).WithMany(p => p.Purchasehistory)
                .HasForeignKey(d => d.ItemId)
                .HasConstraintName("purchasehistory_ibfk_2");

            entity.HasOne(d => d.User).WithMany(p => p.Purchasehistory)
                .HasForeignKey(d => d.UserId)
                .HasConstraintName("purchasehistory_ibfk_1");
        });

        modelBuilder.Entity<Roles>(entity =>
        {
            entity.HasKey(e => e.RoleId).HasName("PRIMARY");

            entity.ToTable("roles");

            entity.Property(e => e.RoleId).HasColumnName("RoleID");
            entity.Property(e => e.RoleName).HasMaxLength(50);
        });

        modelBuilder.Entity<Tasks>(entity =>
        {
            entity.HasKey(e => e.TaskId).HasName("PRIMARY");

            entity.ToTable("tasks");

            entity.HasIndex(e => e.StatusId, "StatusID");

            entity.HasIndex(e => e.UserId, "idx_task_user");

            entity.Property(e => e.TaskId).HasColumnName("TaskID");
            entity.Property(e => e.AiprocessedDescription)
                .HasColumnType("text")
                .HasColumnName("AIProcessedDescription");
            entity.Property(e => e.CompletedAt)
                .ValueGeneratedOnAddOrUpdate()
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnType("timestamp");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnType("timestamp");
            entity.Property(e => e.StatusId).HasColumnName("StatusID");
            entity.Property(e => e.TaskDescription).HasColumnType("text");
            entity.Property(e => e.TaskName).HasMaxLength(255);
            entity.Property(e => e.UserId).HasColumnName("UserID");
            entity.Property(e => e.Xpreward).HasColumnName("XPReward");

            entity.HasOne(d => d.Status).WithMany(p => p.Tasks)
                .HasForeignKey(d => d.StatusId)
                .HasConstraintName("tasks_ibfk_2");

            entity.HasOne(d => d.User).WithMany(p => p.Tasks)
                .HasForeignKey(d => d.UserId)
                .HasConstraintName("tasks_ibfk_1");
        });

        modelBuilder.Entity<Taskstatus>(entity =>
        {
            entity.HasKey(e => e.StatusId).HasName("PRIMARY");

            entity.ToTable("taskstatus");

            entity.HasIndex(e => e.StatusName, "StatusName").IsUnique();

            entity.Property(e => e.StatusId).HasColumnName("StatusID");
            entity.Property(e => e.StatusName).HasMaxLength(50);
        });

        modelBuilder.Entity<Tradehistory>(entity =>
        {
            entity.HasKey(e => e.TradeId).HasName("PRIMARY");

            entity.ToTable("tradehistory");

            entity.HasIndex(e => e.UserBid, "UserBID");

            entity.HasIndex(e => e.UserTreeAid, "UserTreeAID");

            entity.HasIndex(e => e.UserTreeBid, "UserTreeBID");

            entity.HasIndex(e => e.StatusId, "idx_tradehistory_statusid");

            entity.HasIndex(e => new { e.UserAid, e.UserBid }, "idx_tradehistory_user");

            entity.Property(e => e.TradeId).HasColumnName("TradeID");
            entity.Property(e => e.CompletedAt).HasColumnType("timestamp");
            entity.Property(e => e.RequestedAt)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnType("timestamp");
            entity.Property(e => e.StatusId).HasColumnName("StatusID");
            entity.Property(e => e.TradeFee)
                .HasPrecision(10, 2)
                .HasDefaultValueSql("'0.00'");
            entity.Property(e => e.UserAid).HasColumnName("UserAID");
            entity.Property(e => e.UserBid).HasColumnName("UserBID");
            entity.Property(e => e.UserTreeAid).HasColumnName("UserTreeAID");
            entity.Property(e => e.UserTreeBid).HasColumnName("UserTreeBID");

            entity.HasOne(d => d.Status).WithMany(p => p.Tradehistory)
                .HasForeignKey(d => d.StatusId)
                .HasConstraintName("tradehistory_ibfk_5");

            entity.HasOne(d => d.UserA).WithMany(p => p.TradehistoryUserA)
                .HasForeignKey(d => d.UserAid)
                .HasConstraintName("tradehistory_ibfk_1");

            entity.HasOne(d => d.UserB).WithMany(p => p.TradehistoryUserB)
                .HasForeignKey(d => d.UserBid)
                .HasConstraintName("tradehistory_ibfk_2");

            entity.HasOne(d => d.UserTreeA).WithMany(p => p.TradehistoryUserTreeA)
                .HasForeignKey(d => d.UserTreeAid)
                .HasConstraintName("tradehistory_ibfk_3");

            entity.HasOne(d => d.UserTreeB).WithMany(p => p.TradehistoryUserTreeB)
                .HasForeignKey(d => d.UserTreeBid)
                .HasConstraintName("tradehistory_ibfk_4");
        });

        modelBuilder.Entity<Tradestatus>(entity =>
        {
            entity.HasKey(e => e.StatusId).HasName("PRIMARY");

            entity.ToTable("tradestatus");

            entity.Property(e => e.StatusId).HasColumnName("StatusID");
            entity.Property(e => e.StatusName).HasMaxLength(50);
        });

        modelBuilder.Entity<Transactions>(entity =>
        {
            entity.HasKey(e => e.TransactionId).HasName("PRIMARY");

            entity.ToTable("transactions");

            entity.HasIndex(e => e.WalletId, "WalletID");

            entity.HasIndex(e => e.Status, "idx_transaction_status");

            entity.HasIndex(e => e.UserId, "idx_transaction_user");

            entity.Property(e => e.TransactionId).HasColumnName("TransactionID");
            entity.Property(e => e.Amount).HasPrecision(10, 2);
            entity.Property(e => e.CommissionFee).HasPrecision(10, 2);
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnType("timestamp");
            entity.Property(e => e.Status).HasMaxLength(20);
            entity.Property(e => e.TransactionType).HasMaxLength(50);
            entity.Property(e => e.UserId).HasColumnName("UserID");
            entity.Property(e => e.WalletId).HasColumnName("WalletID");

            entity.HasOne(d => d.User).WithMany(p => p.Transactions)
                .HasForeignKey(d => d.UserId)
                .HasConstraintName("transactions_ibfk_1");

            entity.HasOne(d => d.Wallet).WithMany(p => p.Transactions)
                .HasForeignKey(d => d.WalletId)
                .HasConstraintName("transactions_ibfk_2");
        });

        modelBuilder.Entity<Treeprogress>(entity =>
        {
            entity.HasKey(e => e.ProgressId).HasName("PRIMARY");

            entity.ToTable("treeprogress");

            entity.HasIndex(e => e.UserTreeId, "UserTreeID");

            entity.Property(e => e.ProgressId).HasColumnName("ProgressID");
            entity.Property(e => e.PeriodType).HasColumnType("enum('Daily','Weekly','Monthly')");
            entity.Property(e => e.UserTreeId).HasColumnName("UserTreeID");
            entity.Property(e => e.Xprequired).HasColumnName("XPRequired");

            entity.HasOne(d => d.UserTree).WithMany(p => p.Treeprogress)
                .HasForeignKey(d => d.UserTreeId)
                .HasConstraintName("treeprogress_ibfk_1");
        });

        modelBuilder.Entity<Treetype>(entity =>
        {
            entity.HasKey(e => e.TreeTypeId).HasName("PRIMARY");

            entity.ToTable("treetype");

            entity.Property(e => e.TreeTypeId).HasColumnName("TreeTypeID");
            entity.Property(e => e.BasePrice).HasPrecision(10, 2);
            entity.Property(e => e.CreatedAt).HasColumnType("timestamp");
            entity.Property(e => e.Name).HasMaxLength(100);
            entity.Property(e => e.Rarity).HasMaxLength(50);
        });

        modelBuilder.Entity<Useractivity>(entity =>
        {
            entity.HasKey(e => e.ActivityId).HasName("PRIMARY");

            entity.ToTable("useractivity");

            entity.HasIndex(e => e.FocusMethodId, "FocusMethodID");

            entity.HasIndex(e => e.TaskId, "TaskID1");

            entity.HasIndex(e => e.UserId, "idx_user_activity_user");

            entity.Property(e => e.ActivityId).HasColumnName("ActivityID");
            entity.Property(e => e.EndTime).HasColumnType("timestamp");
            entity.Property(e => e.FocusMethodId).HasColumnName("FocusMethodID");
            entity.Property(e => e.Keystrokes).HasDefaultValueSql("'0'");
            entity.Property(e => e.MouseClicks).HasDefaultValueSql("'0'");
            entity.Property(e => e.MouseScrolls).HasDefaultValueSql("'0'");
            entity.Property(e => e.StartTime).HasColumnType("timestamp");
            entity.Property(e => e.TaskId).HasColumnName("TaskID");
            entity.Property(e => e.UserId).HasColumnName("UserID");

            entity.HasOne(d => d.FocusMethod).WithMany(p => p.Useractivity)
                .HasForeignKey(d => d.FocusMethodId)
                .HasConstraintName("useractivity_ibfk_2");

            entity.HasOne(d => d.Task).WithMany(p => p.Useractivity)
                .HasForeignKey(d => d.TaskId)
                .HasConstraintName("useractivity_ibfk_3");

            entity.HasOne(d => d.User).WithMany(p => p.Useractivity)
                .HasForeignKey(d => d.UserId)
                .HasConstraintName("useractivity_ibfk_1");
        });

        modelBuilder.Entity<Userexperience>(entity =>
        {
            entity.HasKey(e => e.UserExperienceId).HasName("PRIMARY");

            entity.ToTable("userexperience");

            entity.HasIndex(e => e.UserId, "UserID3");

            entity.Property(e => e.UserExperienceId).HasColumnName("UserExperienceID");
            entity.Property(e => e.TotalXp).HasColumnName("TotalXP");
            entity.Property(e => e.UpdatedAt)
                .ValueGeneratedOnAddOrUpdate()
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnType("timestamp");
            entity.Property(e => e.UserId).HasColumnName("UserID");
            entity.Property(e => e.XptoNextLevel).HasColumnName("XPToNextLevel");

            entity.HasOne(d => d.User).WithMany(p => p.Userexperience)
                .HasForeignKey(d => d.UserId)
                .HasConstraintName("userexperience_ibfk_1");
        });

        modelBuilder.Entity<Users>(entity =>
        {
            entity.HasKey(e => e.UserId).HasName("PRIMARY");

            entity.ToTable("users");

            entity.HasIndex(e => e.Email, "Email").IsUnique();

            entity.HasIndex(e => e.RoleId, "RoleID");

            entity.Property(e => e.UserId).HasColumnName("UserID");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnType("timestamp");
            entity.Property(e => e.Email)
                .IsRequired()
                .HasMaxLength(100);
            entity.Property(e => e.FullName)
                .IsRequired()
                .HasMaxLength(100);
            entity.Property(e => e.OtpExpiry).HasMaxLength(6);
            entity.Property(e => e.Password)
                .IsRequired()
                .HasMaxLength(255);
            entity.Property(e => e.Phone)
                .IsRequired()
                .HasMaxLength(20);
            entity.Property(e => e.RefreshTokenExpiry).HasMaxLength(6);
            entity.Property(e => e.RoleId).HasColumnName("RoleID");
            entity.Property(e => e.Status)
                .IsRequired()
                .HasMaxLength(20);
            entity.Property(e => e.UpdatedAt)
                .ValueGeneratedOnAddOrUpdate()
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnType("timestamp");

            entity.HasOne(d => d.Role).WithMany(p => p.Users)
                .HasForeignKey(d => d.RoleId)
                .HasConstraintName("users_ibfk_1");
        });

        modelBuilder.Entity<Usertree>(entity =>
        {
            entity.HasKey(e => e.UserTreeId).HasName("PRIMARY");

            entity.ToTable("usertree");

            entity.HasIndex(e => e.FinalTreeId, "FinalTreeID");

            entity.HasIndex(e => e.UserId, "UserID4");

            entity.Property(e => e.UserTreeId).HasColumnName("UserTreeID");
            entity.Property(e => e.FinalTreeId).HasColumnName("FinalTreeID");
            entity.Property(e => e.FinalTreeRarity).HasMaxLength(50);
            entity.Property(e => e.LastUpdated)
                .ValueGeneratedOnAddOrUpdate()
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnType("timestamp");
            entity.Property(e => e.PlantedAt).HasColumnType("timestamp");
            entity.Property(e => e.TreeStatus)
                .HasDefaultValueSql("'Growing'")
                .HasColumnType("enum('Growing','Mature','MaxLevel')");
            entity.Property(e => e.UserId).HasColumnName("UserID");
            entity.Property(e => e.Xp).HasColumnName("XP");

            entity.HasOne(d => d.FinalTree).WithMany(p => p.Usertree)
                .HasForeignKey(d => d.FinalTreeId)
                .HasConstraintName("usertree_ibfk_2");

            entity.HasOne(d => d.User).WithMany(p => p.Usertree)
                .HasForeignKey(d => d.UserId)
                .OnDelete(DeleteBehavior.Cascade)
                .HasConstraintName("usertree_ibfk_1");
        });

        modelBuilder.Entity<Wallet>(entity =>
        {
            entity.HasKey(e => e.WalletId).HasName("PRIMARY");

            entity.ToTable("wallet");

            entity.HasIndex(e => e.UserId, "UserID5");

            entity.Property(e => e.WalletId).HasColumnName("WalletID");
            entity.Property(e => e.Balance)
                .HasPrecision(10, 2)
                .HasDefaultValueSql("'0.00'");
            entity.Property(e => e.UpdatedAt)
                .ValueGeneratedOnAddOrUpdate()
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnType("timestamp");
            entity.Property(e => e.UserId).HasColumnName("UserID");

            entity.HasOne(d => d.User).WithMany(p => p.Wallet)
                .HasForeignKey(d => d.UserId)
                .HasConstraintName("wallet_ibfk_1");
        });

        modelBuilder.Entity<Workspace>(entity =>
        {
            entity.HasKey(e => e.WorkspaceId).HasName("PRIMARY");

            entity.ToTable("workspace");

            entity.HasIndex(e => e.UserId, "UserID6");

            entity.Property(e => e.WorkspaceId).HasColumnName("WorkspaceID");
            entity.Property(e => e.Configuration).HasColumnType("json");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnType("timestamp");
            entity.Property(e => e.UpdatedAt)
                .ValueGeneratedOnAddOrUpdate()
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnType("timestamp");
            entity.Property(e => e.UserId).HasColumnName("UserID");

            entity.HasOne(d => d.User).WithMany(p => p.Workspace)
                .HasForeignKey(d => d.UserId)
                .HasConstraintName("workspace_ibfk_1");
        });

        modelBuilder.Entity<Workspaceitem>(entity =>
        {
            entity.HasKey(e => e.WorkspaceItemId).HasName("PRIMARY");

            entity.ToTable("workspaceitem");

            entity.HasIndex(e => e.ItemId, "ItemID3");

            entity.HasIndex(e => e.WorkspaceId, "WorkspaceID");

            entity.HasIndex(e => e.UserId, "idx_workspace_item_user");

            entity.Property(e => e.WorkspaceItemId).HasColumnName("WorkspaceItemID");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnType("timestamp");
            entity.Property(e => e.Effect).HasColumnType("json");
            entity.Property(e => e.ItemId).HasColumnName("ItemID");
            entity.Property(e => e.UserId).HasColumnName("UserID");
            entity.Property(e => e.WorkspaceId).HasColumnName("WorkspaceID");

            entity.HasOne(d => d.Item).WithMany(p => p.Workspaceitem)
                .HasForeignKey(d => d.ItemId)
                .HasConstraintName("workspaceitem_ibfk_2");

            entity.HasOne(d => d.User).WithMany(p => p.Workspaceitem)
                .HasForeignKey(d => d.UserId)
                .HasConstraintName("workspaceitem_ibfk_3");

            entity.HasOne(d => d.Workspace).WithMany(p => p.Workspaceitem)
                .HasForeignKey(d => d.WorkspaceId)
                .HasConstraintName("workspaceitem_ibfk_1");
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}