﻿// <auto-generated> This file has been auto generated by EF Core Power Tools. </auto-generated>
#nullable disable
using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Pomelo.EntityFrameworkCore.MySql.Scaffolding.Internal;
using ZenGarden.Domain.Entities;
using ZenGarden.Domain.Enums;

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

    public virtual DbSet<BagItem> BagItem { get; set; }

    public virtual DbSet<FocusMethod> FocusMethod { get; set; }

    public virtual DbSet<Item> Item { get; set; }

    public virtual DbSet<ItemDetail> ItemDetail { get; set; }

    public virtual DbSet<Leaderboard> Leaderboard { get; set; }

    public virtual DbSet<PurchaseHistory> PurchaseHistory { get; set; }

    public virtual DbSet<Roles> Roles { get; set; }

    public virtual DbSet<TaskFocusSetting> TaskFocusSetting { get; set; }

    public virtual DbSet<Tasks> Tasks { get; set; }

    public virtual DbSet<TradeHistory> TradeHistory { get; set; }

    public virtual DbSet<Transactions> Transactions { get; set; }

    public virtual DbSet<TreeLevelConfig> TreeLevelConfig { get; set; }

    public virtual DbSet<TreeType> TreeType { get; set; }

    public virtual DbSet<TreeXpLog> TreeXpLog { get; set; }

    public virtual DbSet<UserExperience> UserExperience { get; set; }

    public virtual DbSet<UserLevelConfig> UserLevelConfig { get; set; }

    public virtual DbSet<Users> Users { get; set; }

    public virtual DbSet<UserTree> UserTree { get; set; }

    public virtual DbSet<UserXpLog> UserXpLog { get; set; }

    public virtual DbSet<Wallet> Wallet { get; set; }

    public virtual DbSet<Workspace> Workspace { get; set; }

    public virtual DbSet<WorkspaceItem> WorkspaceItem { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        if (!optionsBuilder.IsConfigured)
        {
            var connectionString = GetConnectionString();
            //optionsBuilder.UseMySql("server=localhost;database=zengarden;uid=root;pwd=10112003", Microsoft.EntityFrameworkCore.ServerVersion.Parse("9.2.0-mysql"));
            optionsBuilder.UseMySql(connectionString?? "server=localhost;database=zengarden;uid=root;pwd=10112003", Microsoft.EntityFrameworkCore.ServerVersion.Parse("9.2.0-mysql"));
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
            
            entity.HasIndex(e => e.UserId, "idx_bag_user").IsUnique();

            entity.Property(e => e.BagId).HasColumnName("BagID");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("UTC_TIMESTAMP()")
                .HasColumnType("timestamp");
            entity.Property(e => e.UpdatedAt)
                .HasColumnType("timestamp")
                .HasDefaultValueSql("UTC_TIMESTAMP()");
            entity.Property(e => e.UserId).HasColumnName("UserID");
        });

        modelBuilder.Entity<BagItem>(entity =>
        {
            entity.HasKey(e => e.BagItemId).HasName("PRIMARY");
            
            entity.HasIndex(e => e.BagId, "BagID");

            entity.HasIndex(e => e.ItemId, "ItemID");

            entity.Property(e => e.BagItemId).HasColumnName("BagItemID");
            entity.Property(e => e.AddedAt)
                .HasDefaultValueSql("UTC_TIMESTAMP()")
                .HasColumnType("timestamp");
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
            
            entity.Property(e => e.FocusMethodId).HasColumnName("FocusMethodID");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("UTC_TIMESTAMP()")
                .HasColumnType("timestamp");
            entity.Property(e => e.Name).HasMaxLength(100);
        });

        modelBuilder.Entity<Item>(entity =>
        {
            entity.HasKey(e => e.ItemId).HasName("PRIMARY");
            
            entity.HasIndex(e => new { e.Type, e.Rarity }, "idx_item_type_rarity");

            entity.Property(e => e.ItemId).HasColumnName("ItemID");
            entity.Property(e => e.Cost).HasPrecision(10, 2);
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("UTC_TIMESTAMP()")
                .HasColumnType("timestamp");
            entity.Property(e => e.Name).HasMaxLength(100);
            entity.Property(e => e.Rarity).HasMaxLength(50);
            entity.Property(e => e.Type).HasMaxLength(50);
        });

        modelBuilder.Entity<ItemDetail>(entity =>
        {
            entity.HasKey(e => e.ItemDetailId).HasName("PRIMARY");
            
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
                .HasColumnType("timestamp")
                .HasDefaultValueSql("UTC_TIMESTAMP()");

            entity.HasOne(d => d.Item).WithOne(p => p.ItemDetail)
                .HasForeignKey<ItemDetail>(d => d.ItemId)
                .OnDelete(DeleteBehavior.Cascade)
                .HasConstraintName("itemdetail_ibfk_1");
        });

        modelBuilder.Entity<Leaderboard>(entity =>
        {
            entity.HasKey(e => e.LeaderboardId).HasName("PRIMARY");
            
            entity.HasIndex(e => e.UserId, "UserID1");

            entity.Property(e => e.LeaderboardId).HasColumnName("LeaderboardID");
            entity.Property(e => e.LastUpdated)
                .ValueGeneratedOnAddOrUpdate()
                .HasDefaultValueSql("UTC_TIMESTAMP()")
                .HasColumnType("timestamp");
            entity.Property(e => e.UserId).HasColumnName("UserID");

            entity.HasOne(d => d.User).WithMany(p => p.Leaderboard)
                .HasForeignKey(d => d.UserId)
                .HasConstraintName("leaderboard_ibfk_1");
        });

        modelBuilder.Entity<PurchaseHistory>(entity =>
        {
            entity.HasKey(e => e.PurchaseId).HasName("PRIMARY");
            
            entity.HasIndex(e => e.ItemId, "ItemID2");

            entity.HasIndex(e => e.UserId, "UserID2");

            entity.Property(e => e.PurchaseId).HasColumnName("PurchaseID");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("UTC_TIMESTAMP()")
                .HasColumnType("timestamp");
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
            
            entity.HasIndex(e => e.UserId, "idx_task_user");

            entity.Property(e => e.TaskId).HasColumnName("TaskID");
            entity.Property(e => e.AiProcessedDescription)
                .HasColumnType("text")
                .HasColumnName("AIProcessedDescription");
            entity.Property(e => e.CompletedAt)
                .ValueGeneratedOnAddOrUpdate()
                .HasDefaultValueSql("UTC_TIMESTAMP()")
                .HasColumnType("timestamp");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("UTC_TIMESTAMP()")
                .HasColumnType("timestamp");
            entity.Property(e => e.TaskDescription).HasColumnType("text");
            entity.Property(e => e.TaskName).HasMaxLength(255);
            entity.Property(e => e.UserId).HasColumnName("UserID");

            entity.HasOne(d => d.User).WithMany(p => p.Tasks)
                .HasForeignKey(d => d.UserId)
                .HasConstraintName("tasks_ibfk_1");
        });

        modelBuilder.Entity<TradeHistory>(entity =>
        {
            entity.HasKey(e => e.TradeId).HasName("PRIMARY");
            
            entity.HasIndex(e => e.UserBid, "UserBID");

            entity.HasIndex(e => e.UserTreeAid, "UserTreeAID");

            entity.HasIndex(e => e.UserTreeBid, "UserTreeBID");

            entity.HasIndex(e => new { e.UserAid, e.UserBid }, "idx_tradehistory_user");

            entity.Property(e => e.TradeId).HasColumnName("TradeID");
            entity.Property(e => e.CompletedAt).HasColumnType("timestamp");
            entity.Property(e => e.RequestedAt)
                .HasDefaultValueSql("UTC_TIMESTAMP()")
                .HasColumnType("timestamp");
            entity.Property(e => e.TradeFee)
                .HasPrecision(10, 2)
                .HasDefaultValueSql("'0.00'");
            entity.Property(e => e.UserAid).HasColumnName("UserAID");
            entity.Property(e => e.UserBid).HasColumnName("UserBID");
            entity.Property(e => e.UserTreeAid).HasColumnName("UserTreeAID");
            entity.Property(e => e.UserTreeBid).HasColumnName("UserTreeBID");


            entity.HasOne(d => d.UserA).WithMany(p => p.TradeHistoryUserA)
                .HasForeignKey(d => d.UserAid)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(d => d.UserB).WithMany(p => p.TradeHistoryUserB)
                .HasForeignKey(d => d.UserBid)
                .HasConstraintName("tradehistory_ibfk_2");

            entity.HasOne(d => d.UserTreeA).WithMany(p => p.TradeHistoryUserTreeA)
                .HasForeignKey(d => d.UserTreeAid)
                .HasConstraintName("tradehistory_ibfk_3");

            entity.HasOne(d => d.UserTreeB).WithMany(p => p.TradeHistoryUserTreeB)
                .HasForeignKey(d => d.UserTreeBid)
                .HasConstraintName("tradehistory_ibfk_4");
        });


        modelBuilder.Entity<Transactions>(entity =>
        {
            entity.HasKey(e => e.TransactionId).HasName("PRIMARY");
            
            entity.HasIndex(e => e.WalletId, "WalletID");

            entity.HasIndex(e => e.Status, "idx_transaction_status");

            entity.HasIndex(e => e.UserId, "idx_transaction_user");

            entity.Property(e => e.TransactionId).HasColumnName("TransactionID");
            entity.Property(e => e.Amount).HasPrecision(10, 2);
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("UTC_TIMESTAMP()")
                .HasColumnType("timestamp");
            entity.Property(e => e.Status).HasMaxLength(20);
            entity.Property(e => e.UserId).HasColumnName("UserID");
            entity.Property(e => e.WalletId).HasColumnName("WalletID");

            entity.HasOne(d => d.User).WithMany(p => p.Transactions)
                .HasForeignKey(d => d.UserId)
                .HasConstraintName("transactions_ibfk_1");

            entity.HasOne(d => d.Wallet).WithMany(p => p.Transactions)
                .HasForeignKey(d => d.WalletId)
                .HasConstraintName("transactions_ibfk_2");
        });


        modelBuilder.Entity<TreeType>(entity =>
        {
            entity.HasKey(e => e.TreeTypeId).HasName("PRIMARY");
            entity.Property(e => e.TreeTypeId).HasColumnName("TreeTypeID");
            entity.Property(e => e.BasePrice).HasPrecision(10, 2);
            entity.Property(e => e.CreatedAt).HasDefaultValueSql("UTC_TIMESTAMP()").HasColumnType("timestamp");
            entity.Property(e => e.Name).HasMaxLength(100);
            entity.Property(e => e.Rarity).HasMaxLength(50);
        });


        modelBuilder.Entity<UserExperience>(entity =>
        {
            entity.HasKey(e => e.UserId).HasName("PRIMARY");
            
            entity.Property(e => e.TotalXp).HasColumnName("TotalXP");
            entity.Property(e => e.UpdatedAt)
                .HasColumnType("timestamp")
                .HasDefaultValueSql("UTC_TIMESTAMP()");
            entity.HasOne(e => e.User)
                .WithOne(u => u.UserExperience)
                .HasForeignKey<UserExperience>(e => e.UserId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<Users>(entity =>
        {
            entity.HasKey(e => e.UserId).HasName("PRIMARY");
            
            entity.HasIndex(e => e.Email, "Email").IsUnique();

            entity.HasIndex(e => e.RoleId, "RoleID");

            entity.Property(e => e.UserId).HasColumnName("UserID");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("UTC_TIMESTAMP()")
                .HasColumnType("timestamp");
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
            entity.Property(e => e.IsActive)
                    .HasDefaultValue(true);

            entity.Property(e => e.RefreshTokenHash)
                    .HasMaxLength(255)
                    .IsUnicode(false);

            entity.Property(e => e.RefreshTokenExpiry)
                .HasColumnType("timestamp")
                .HasDefaultValueSql("UTC_TIMESTAMP()");

            entity.Property(e => e.OtpCodeHash)
                    .HasMaxLength(255)
                    .IsUnicode(false);

            entity.Property(e => e.OtpExpiry)
                .HasColumnType("timestamp")
                .HasDefaultValueSql("UTC_TIMESTAMP()");

            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("UTC_TIMESTAMP()")
                    .HasColumnType("timestamp");
            entity.Property(e => e.UpdatedAt)
                .HasColumnType("timestamp")
                .HasDefaultValueSql("UTC_TIMESTAMP()");
            
            entity.HasOne(d => d.Role)
                .WithMany(p => p.Users)
                .HasForeignKey(d => d.RoleId)
                .HasConstraintName("users_ibfk_1");

            entity.HasOne(u => u.Wallet)
                .WithOne(w => w.User)
                .HasForeignKey<Wallet>(w => w.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(u => u.Bag)
                .WithOne(b => b.User)
                .HasForeignKey<Bag>(b => b.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(u => u.Workspace)
                .WithOne(w => w.User)
                .HasForeignKey<Workspace>(w => w.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(u => u.UserExperience)
                .WithOne(ue => ue.User)
                .HasForeignKey<UserExperience>(ue => ue.UserId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<UserTree>(entity =>
        {
            entity.HasKey(e => e.UserTreeId).HasName("PRIMARY");
            
            entity.HasIndex(e => e.FinalTreeId, "FinalTreeID");

            entity.HasIndex(e => e.UserId, "UserID4");

            entity.Property(e => e.UserTreeId).HasColumnName("UserTreeID");
            entity.Property(e => e.FinalTreeId).HasColumnName("FinalTreeID");
            entity.Property(e => e.FinalTreeRarity).HasMaxLength(50);
            entity.Property(e => e.LastUpdated)
                .ValueGeneratedOnAddOrUpdate()
                .HasDefaultValueSql("UTC_TIMESTAMP()")
                .HasColumnType("timestamp");
            entity.Property(e => e.PlantedAt).HasColumnType("timestamp");
            entity.Property(e => e.TreeStatus)
                .HasDefaultValueSql("'Growing'")
                .HasColumnType("enum('Growing','Mature','MaxLevel')");
            entity.Property(e => e.UserId).HasColumnName("UserID");

            entity.HasOne(d => d.User).WithMany(p => p.UserTree)
                .HasForeignKey(d => d.UserId)
                .OnDelete(DeleteBehavior.Cascade)
                .HasConstraintName("usertree_ibfk_1");
        });

        modelBuilder.Entity<Wallet>(entity =>
        {
            entity.HasKey(e => e.WalletId).HasName("PRIMARY");
            
            entity.HasIndex(e => e.UserId, "UserID5").IsUnique();

            entity.Property(e => e.WalletId).HasColumnName("WalletID");
            entity.Property(e => e.Balance)
                .HasPrecision(10, 2)
                .HasDefaultValueSql("'0.00'");
            entity.Property(e => e.UpdatedAt)
                .HasColumnType("timestamp")
                .HasDefaultValueSql("UTC_TIMESTAMP()");

            entity.Property(e => e.UserId).HasColumnName("UserID");

            entity.HasOne(d => d.User).WithOne(p => p.Wallet)
                .HasForeignKey<Wallet>(d => d.UserId)
                .OnDelete(DeleteBehavior.Cascade)
                .HasConstraintName("wallet_ibfk_1");
        });

        modelBuilder.Entity<Workspace>(entity =>
        {
            entity.HasKey(e => e.WorkspaceId).HasName("PRIMARY");
            
            entity.HasIndex(e => e.UserId, "idx_workspace_user").IsUnique();

            entity.Property(e => e.WorkspaceId).HasColumnName("WorkspaceID");
            entity.Property(e => e.Configuration).HasColumnType("json");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("UTC_TIMESTAMP()")
                .HasColumnType("timestamp");
            entity.Property(e => e.UpdatedAt)
                .HasColumnType("timestamp")
                .HasDefaultValueSql("UTC_TIMESTAMP()");

            entity.Property(e => e.UserId).HasColumnName("UserID");

            entity.HasOne(d => d.User).WithOne(p => p.Workspace)
                .HasForeignKey<Workspace>(d => d.UserId)
                .OnDelete(DeleteBehavior.Cascade)
                .HasConstraintName("workspace_ibfk_1");

            entity.HasMany(w => w.Tasks)
                .WithOne(t => t.Workspace)
                .HasForeignKey(t => t.WorkspaceId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<WorkspaceItem>(entity =>
        {
            entity.HasKey(e => e.WorkspaceItemId).HasName("PRIMARY");
            
            entity.HasIndex(e => e.ItemId, "ItemID3");

            entity.HasIndex(e => e.WorkspaceId, "WorkspaceID");

            entity.HasIndex(e => e.UserId, "idx_workspace_item_user");

            entity.Property(e => e.WorkspaceItemId).HasColumnName("WorkspaceItemID");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("UTC_TIMESTAMP()")
                .HasColumnType("timestamp");
            entity.Property(e => e.Effect).HasColumnType("json");
            entity.Property(e => e.ItemId).HasColumnName("ItemID");
            entity.Property(e => e.UserId).HasColumnName("UserID");
            entity.Property(e => e.WorkspaceId).HasColumnName("WorkspaceID");

            entity.HasOne(d => d.Item).WithMany(p => p.WorkspaceItem)
                .HasForeignKey(d => d.ItemId)
                .HasConstraintName("workspaceitem_ibfk_2");

            entity.HasOne(d => d.User).WithMany(p => p.WorkspaceItem)
                .HasForeignKey(d => d.UserId)
                .HasConstraintName("workspaceitem_ibfk_3");

            entity.HasOne(d => d.Workspace).WithMany(p => p.WorkspaceItem)
                .HasForeignKey(d => d.WorkspaceId)
                .HasConstraintName("workspaceitem_ibfk_1");
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
