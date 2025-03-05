using System;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ZenGarden.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterDatabase()
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "focusmethod",
                columns: table => new
                {
                    FocusMethodID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    Name = table.Column<string>(type: "varchar(100)", maxLength: 100, nullable: true, collation: "utf8mb4_0900_ai_ci")
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    DefaultDuration = table.Column<int>(type: "int", nullable: true),
                    DefaultBreak = table.Column<int>(type: "int", nullable: true),
                    MinDuration = table.Column<int>(type: "int", nullable: true),
                    MaxDuration = table.Column<int>(type: "int", nullable: true),
                    MinBreak = table.Column<int>(type: "int", nullable: true),
                    MaxBreak = table.Column<int>(type: "int", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp", nullable: true, defaultValueSql: "CURRENT_TIMESTAMP")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PRIMARY", x => x.FocusMethodID);
                })
                .Annotation("MySql:CharSet", "utf8mb4")
                .Annotation("Relational:Collation", "utf8mb4_0900_ai_ci");

            migrationBuilder.CreateTable(
                name: "item",
                columns: table => new
                {
                    ItemID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    Name = table.Column<string>(type: "varchar(100)", maxLength: 100, nullable: true, collation: "utf8mb4_0900_ai_ci")
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Type = table.Column<string>(type: "varchar(50)", maxLength: 50, nullable: true, collation: "utf8mb4_0900_ai_ci")
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Rarity = table.Column<string>(type: "varchar(50)", maxLength: 50, nullable: true, collation: "utf8mb4_0900_ai_ci")
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Cost = table.Column<decimal>(type: "decimal(10,2)", precision: 10, scale: 2, nullable: true),
                    Limited = table.Column<bool>(type: "tinyint(1)", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp", nullable: true, defaultValueSql: "CURRENT_TIMESTAMP")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PRIMARY", x => x.ItemID);
                })
                .Annotation("MySql:CharSet", "utf8mb4")
                .Annotation("Relational:Collation", "utf8mb4_0900_ai_ci");

            migrationBuilder.CreateTable(
                name: "roles",
                columns: table => new
                {
                    RoleID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    RoleName = table.Column<string>(type: "varchar(50)", maxLength: 50, nullable: true, collation: "utf8mb4_0900_ai_ci")
                        .Annotation("MySql:CharSet", "utf8mb4")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PRIMARY", x => x.RoleID);
                })
                .Annotation("MySql:CharSet", "utf8mb4")
                .Annotation("Relational:Collation", "utf8mb4_0900_ai_ci");

            migrationBuilder.CreateTable(
                name: "treetype",
                columns: table => new
                {
                    TreeTypeID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    Name = table.Column<string>(type: "varchar(100)", maxLength: 100, nullable: true, collation: "utf8mb4_0900_ai_ci")
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Rarity = table.Column<string>(type: "varchar(50)", maxLength: 50, nullable: true, collation: "utf8mb4_0900_ai_ci")
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    CreatedAt = table.Column<DateTime>(type: "timestamp", nullable: true),
                    BasePrice = table.Column<decimal>(type: "decimal(10,2)", precision: 10, scale: 2, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PRIMARY", x => x.TreeTypeID);
                })
                .Annotation("MySql:CharSet", "utf8mb4")
                .Annotation("Relational:Collation", "utf8mb4_0900_ai_ci");

            migrationBuilder.CreateTable(
                name: "UserLevelConfig",
                columns: table => new
                {
                    Level = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    XpRequired = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserLevelConfig", x => x.Level);
                })
                .Annotation("MySql:CharSet", "utf8mb4")
                .Annotation("Relational:Collation", "utf8mb4_0900_ai_ci");

            migrationBuilder.CreateTable(
                name: "itemdetail",
                columns: table => new
                {
                    ItemDetailID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    ItemID = table.Column<int>(type: "int", nullable: true),
                    Description = table.Column<string>(type: "text", nullable: true, collation: "utf8mb4_0900_ai_ci")
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    ImageUrl = table.Column<string>(type: "varchar(255)", maxLength: 255, nullable: true, collation: "utf8mb4_0900_ai_ci")
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Stats = table.Column<string>(type: "json", nullable: true, collation: "utf8mb4_0900_ai_ci")
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Requirements = table.Column<string>(type: "json", nullable: true, collation: "utf8mb4_0900_ai_ci")
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    SpecialEffects = table.Column<string>(type: "text", nullable: true, collation: "utf8mb4_0900_ai_ci")
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    DurationType = table.Column<string>(type: "varchar(50)", maxLength: 50, nullable: true, collation: "utf8mb4_0900_ai_ci")
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Duration = table.Column<int>(type: "int", nullable: true),
                    Cooldown = table.Column<int>(type: "int", nullable: true),
                    MaxStack = table.Column<int>(type: "int", nullable: true),
                    Tags = table.Column<string>(type: "json", nullable: true, collation: "utf8mb4_0900_ai_ci")
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp", nullable: true, defaultValueSql: "CURRENT_TIMESTAMP")
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.ComputedColumn)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PRIMARY", x => x.ItemDetailID);
                    table.ForeignKey(
                        name: "itemdetail_ibfk_1",
                        column: x => x.ItemID,
                        principalTable: "item",
                        principalColumn: "ItemID",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySql:CharSet", "utf8mb4")
                .Annotation("Relational:Collation", "utf8mb4_0900_ai_ci");

            migrationBuilder.CreateTable(
                name: "users",
                columns: table => new
                {
                    UserID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    RoleID = table.Column<int>(type: "int", nullable: true),
                    UserName = table.Column<string>(type: "varchar(100)", maxLength: 100, nullable: false, collation: "utf8mb4_0900_ai_ci")
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Email = table.Column<string>(type: "varchar(100)", maxLength: 100, nullable: false, collation: "utf8mb4_0900_ai_ci")
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Password = table.Column<string>(type: "varchar(255)", maxLength: 255, nullable: false, collation: "utf8mb4_0900_ai_ci")
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Phone = table.Column<string>(type: "varchar(20)", maxLength: 20, nullable: false, collation: "utf8mb4_0900_ai_ci")
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Status = table.Column<int>(type: "int", nullable: false),
                    IsActive = table.Column<bool>(type: "tinyint(1)", nullable: false, defaultValue: true),
                    RefreshTokenHash = table.Column<string>(type: "varchar(255)", unicode: false, maxLength: 255, nullable: true, collation: "utf8mb4_0900_ai_ci")
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    RefreshTokenExpiry = table.Column<DateTime>(type: "timestamp", nullable: true, defaultValueSql: "CURRENT_TIMESTAMP"),
                    OtpCodeHash = table.Column<string>(type: "varchar(255)", unicode: false, maxLength: 255, nullable: true, collation: "utf8mb4_0900_ai_ci")
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    OtpExpiry = table.Column<DateTime>(type: "timestamp", nullable: true, defaultValueSql: "CURRENT_TIMESTAMP"),
                    CreatedAt = table.Column<DateTime>(type: "timestamp", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP"),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP")
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.ComputedColumn)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PRIMARY", x => x.UserID);
                    table.ForeignKey(
                        name: "users_ibfk_1",
                        column: x => x.RoleID,
                        principalTable: "roles",
                        principalColumn: "RoleID");
                })
                .Annotation("MySql:CharSet", "utf8mb4")
                .Annotation("Relational:Collation", "utf8mb4_0900_ai_ci");

            migrationBuilder.CreateTable(
                name: "bag",
                columns: table => new
                {
                    BagID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    UserID = table.Column<int>(type: "int", nullable: false),
                    Capacity = table.Column<int>(type: "int", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp", nullable: true, defaultValueSql: "CURRENT_TIMESTAMP"),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp", nullable: true, defaultValueSql: "CURRENT_TIMESTAMP")
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.ComputedColumn)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PRIMARY", x => x.BagID);
                    table.ForeignKey(
                        name: "FK_bag_users_UserID",
                        column: x => x.UserID,
                        principalTable: "users",
                        principalColumn: "UserID",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySql:CharSet", "utf8mb4")
                .Annotation("Relational:Collation", "utf8mb4_0900_ai_ci");

            migrationBuilder.CreateTable(
                name: "leaderboard",
                columns: table => new
                {
                    LeaderboardID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    UserID = table.Column<int>(type: "int", nullable: true),
                    TotalTrees = table.Column<int>(type: "int", nullable: true),
                    BestTrees = table.Column<int>(type: "int", nullable: true),
                    ProductivityScore = table.Column<int>(type: "int", nullable: true),
                    LastUpdated = table.Column<DateTime>(type: "timestamp", nullable: true, defaultValueSql: "CURRENT_TIMESTAMP")
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.ComputedColumn)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PRIMARY", x => x.LeaderboardID);
                    table.ForeignKey(
                        name: "leaderboard_ibfk_1",
                        column: x => x.UserID,
                        principalTable: "users",
                        principalColumn: "UserID");
                })
                .Annotation("MySql:CharSet", "utf8mb4")
                .Annotation("Relational:Collation", "utf8mb4_0900_ai_ci");

            migrationBuilder.CreateTable(
                name: "purchasehistory",
                columns: table => new
                {
                    PurchaseID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    UserID = table.Column<int>(type: "int", nullable: true),
                    ItemID = table.Column<int>(type: "int", nullable: true),
                    TotalPrice = table.Column<decimal>(type: "decimal(10,2)", precision: 10, scale: 2, nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp", nullable: true, defaultValueSql: "CURRENT_TIMESTAMP"),
                    Status = table.Column<string>(type: "varchar(20)", maxLength: 20, nullable: true, collation: "utf8mb4_0900_ai_ci")
                        .Annotation("MySql:CharSet", "utf8mb4")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PRIMARY", x => x.PurchaseID);
                    table.ForeignKey(
                        name: "purchasehistory_ibfk_1",
                        column: x => x.UserID,
                        principalTable: "users",
                        principalColumn: "UserID");
                    table.ForeignKey(
                        name: "purchasehistory_ibfk_2",
                        column: x => x.ItemID,
                        principalTable: "item",
                        principalColumn: "ItemID");
                })
                .Annotation("MySql:CharSet", "utf8mb4")
                .Annotation("Relational:Collation", "utf8mb4_0900_ai_ci");

            migrationBuilder.CreateTable(
                name: "userexperience",
                columns: table => new
                {
                    UserId = table.Column<int>(type: "int", nullable: false),
                    TotalXP = table.Column<long>(type: "bigint", nullable: false),
                    CurrentLevel = table.Column<int>(type: "int", nullable: false),
                    XpToNextLevel = table.Column<int>(type: "int", nullable: false),
                    LevelId = table.Column<int>(type: "int", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp", nullable: true, defaultValueSql: "CURRENT_TIMESTAMP")
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.ComputedColumn)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PRIMARY", x => x.UserId);
                    table.ForeignKey(
                        name: "FK_userexperience_UserLevelConfig_LevelId",
                        column: x => x.LevelId,
                        principalTable: "UserLevelConfig",
                        principalColumn: "Level",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_userexperience_users_UserId",
                        column: x => x.UserId,
                        principalTable: "users",
                        principalColumn: "UserID",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySql:CharSet", "utf8mb4")
                .Annotation("Relational:Collation", "utf8mb4_0900_ai_ci");

            migrationBuilder.CreateTable(
                name: "usertree",
                columns: table => new
                {
                    UserTreeID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    UserID = table.Column<int>(type: "int", nullable: true),
                    FinalTreeID = table.Column<int>(type: "int", nullable: true),
                    PlantedAt = table.Column<DateTime>(type: "timestamp", nullable: true),
                    TreeLevel = table.Column<int>(type: "int", nullable: false),
                    TotalXp = table.Column<int>(type: "int", nullable: false),
                    TreeStatus = table.Column<string>(type: "enum('Growing','Mature','MaxLevel')", nullable: true, defaultValueSql: "'Growing'", collation: "utf8mb4_0900_ai_ci")
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    FinalTreeRarity = table.Column<int>(type: "int", maxLength: 50, nullable: true),
                    LastUpdated = table.Column<DateTime>(type: "timestamp", nullable: true, defaultValueSql: "CURRENT_TIMESTAMP")
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.ComputedColumn),
                    LevelId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PRIMARY", x => x.UserTreeID);
                    table.ForeignKey(
                        name: "FK_usertree_treetype_FinalTreeID",
                        column: x => x.FinalTreeID,
                        principalTable: "treetype",
                        principalColumn: "TreeTypeID");
                    table.ForeignKey(
                        name: "usertree_ibfk_1",
                        column: x => x.UserID,
                        principalTable: "users",
                        principalColumn: "UserID",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySql:CharSet", "utf8mb4")
                .Annotation("Relational:Collation", "utf8mb4_0900_ai_ci");

            migrationBuilder.CreateTable(
                name: "wallet",
                columns: table => new
                {
                    WalletID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    UserID = table.Column<int>(type: "int", nullable: false),
                    Balance = table.Column<decimal>(type: "decimal(10,2)", precision: 10, scale: 2, nullable: true, defaultValueSql: "'0.00'"),
                    CreatedAt = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp", nullable: true, defaultValueSql: "CURRENT_TIMESTAMP")
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.ComputedColumn),
                    Currency = table.Column<string>(type: "longtext", nullable: true, collation: "utf8mb4_0900_ai_ci")
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    IsLocked = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    LastTransactionAt = table.Column<DateTime>(type: "datetime(6)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PRIMARY", x => x.WalletID);
                    table.ForeignKey(
                        name: "FK_wallet_users_UserID",
                        column: x => x.UserID,
                        principalTable: "users",
                        principalColumn: "UserID",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySql:CharSet", "utf8mb4")
                .Annotation("Relational:Collation", "utf8mb4_0900_ai_ci");

            migrationBuilder.CreateTable(
                name: "workspace",
                columns: table => new
                {
                    WorkspaceID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    UserID = table.Column<int>(type: "int", nullable: false),
                    Configuration = table.Column<string>(type: "json", nullable: true, collation: "utf8mb4_0900_ai_ci")
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    CreatedAt = table.Column<DateTime>(type: "timestamp", nullable: true, defaultValueSql: "CURRENT_TIMESTAMP"),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp", nullable: true, defaultValueSql: "CURRENT_TIMESTAMP")
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.ComputedColumn)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PRIMARY", x => x.WorkspaceID);
                    table.ForeignKey(
                        name: "FK_workspace_users_UserID",
                        column: x => x.UserID,
                        principalTable: "users",
                        principalColumn: "UserID",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySql:CharSet", "utf8mb4")
                .Annotation("Relational:Collation", "utf8mb4_0900_ai_ci");

            migrationBuilder.CreateTable(
                name: "bagitem",
                columns: table => new
                {
                    BagItemID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    BagID = table.Column<int>(type: "int", nullable: true),
                    ItemID = table.Column<int>(type: "int", nullable: true),
                    Quantity = table.Column<int>(type: "int", nullable: true),
                    AddedAt = table.Column<DateTime>(type: "timestamp", nullable: true, defaultValueSql: "CURRENT_TIMESTAMP")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PRIMARY", x => x.BagItemID);
                    table.ForeignKey(
                        name: "bagitem_ibfk_1",
                        column: x => x.BagID,
                        principalTable: "bag",
                        principalColumn: "BagID");
                    table.ForeignKey(
                        name: "bagitem_ibfk_2",
                        column: x => x.ItemID,
                        principalTable: "item",
                        principalColumn: "ItemID");
                })
                .Annotation("MySql:CharSet", "utf8mb4")
                .Annotation("Relational:Collation", "utf8mb4_0900_ai_ci");

            migrationBuilder.CreateTable(
                name: "tradehistory",
                columns: table => new
                {
                    TradeID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    UserAID = table.Column<int>(type: "int", nullable: true),
                    UserBID = table.Column<int>(type: "int", nullable: true),
                    UserTreeAID = table.Column<int>(type: "int", nullable: true),
                    UserTreeBID = table.Column<int>(type: "int", nullable: true),
                    TradeFee = table.Column<decimal>(type: "decimal(10,2)", precision: 10, scale: 2, nullable: true, defaultValueSql: "'0.00'"),
                    RequestedAt = table.Column<DateTime>(type: "timestamp", nullable: true, defaultValueSql: "CURRENT_TIMESTAMP"),
                    CompletedAt = table.Column<DateTime>(type: "timestamp", nullable: true),
                    Status = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PRIMARY", x => x.TradeID);
                    table.ForeignKey(
                        name: "tradehistory_ibfk_1",
                        column: x => x.UserAID,
                        principalTable: "users",
                        principalColumn: "UserID");
                    table.ForeignKey(
                        name: "tradehistory_ibfk_2",
                        column: x => x.UserBID,
                        principalTable: "users",
                        principalColumn: "UserID");
                    table.ForeignKey(
                        name: "tradehistory_ibfk_3",
                        column: x => x.UserTreeAID,
                        principalTable: "usertree",
                        principalColumn: "UserTreeID");
                    table.ForeignKey(
                        name: "tradehistory_ibfk_4",
                        column: x => x.UserTreeBID,
                        principalTable: "usertree",
                        principalColumn: "UserTreeID");
                })
                .Annotation("MySql:CharSet", "utf8mb4")
                .Annotation("Relational:Collation", "utf8mb4_0900_ai_ci");

            migrationBuilder.CreateTable(
                name: "transactions",
                columns: table => new
                {
                    TransactionID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    UserID = table.Column<int>(type: "int", nullable: true),
                    WalletID = table.Column<int>(type: "int", nullable: true),
                    Amount = table.Column<decimal>(type: "decimal(10,2)", precision: 10, scale: 2, nullable: true),
                    Type = table.Column<int>(type: "int", nullable: false),
                    Status = table.Column<int>(type: "int", maxLength: 20, nullable: false),
                    PaymentMethod = table.Column<string>(type: "longtext", nullable: true, collation: "utf8mb4_0900_ai_ci")
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    TransactionRef = table.Column<string>(type: "longtext", nullable: true, collation: "utf8mb4_0900_ai_ci")
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    CreatedAt = table.Column<DateTime>(type: "timestamp", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP"),
                    CompletedAt = table.Column<DateTime>(type: "datetime(6)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PRIMARY", x => x.TransactionID);
                    table.ForeignKey(
                        name: "transactions_ibfk_1",
                        column: x => x.UserID,
                        principalTable: "users",
                        principalColumn: "UserID");
                    table.ForeignKey(
                        name: "transactions_ibfk_2",
                        column: x => x.WalletID,
                        principalTable: "wallet",
                        principalColumn: "WalletID");
                })
                .Annotation("MySql:CharSet", "utf8mb4")
                .Annotation("Relational:Collation", "utf8mb4_0900_ai_ci");

            migrationBuilder.CreateTable(
                name: "tasks",
                columns: table => new
                {
                    TaskID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    UserID = table.Column<int>(type: "int", nullable: true),
                    WorkspaceId = table.Column<int>(type: "int", nullable: true),
                    UserTreeID = table.Column<int>(type: "int", nullable: true),
                    TaskName = table.Column<string>(type: "varchar(255)", maxLength: 255, nullable: true, collation: "utf8mb4_0900_ai_ci")
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    TaskDescription = table.Column<string>(type: "text", nullable: true, collation: "utf8mb4_0900_ai_ci")
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Duration = table.Column<int>(type: "int", nullable: true),
                    AIProcessedDescription = table.Column<string>(type: "text", nullable: true, collation: "utf8mb4_0900_ai_ci")
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    TimeOverdue = table.Column<int>(type: "int", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp", nullable: true, defaultValueSql: "CURRENT_TIMESTAMP"),
                    CompletedAt = table.Column<DateTime>(type: "timestamp", nullable: true, defaultValueSql: "CURRENT_TIMESTAMP")
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.ComputedColumn),
                    Status = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PRIMARY", x => x.TaskID);
                    table.ForeignKey(
                        name: "FK_tasks_usertree_UserTreeID",
                        column: x => x.UserTreeID,
                        principalTable: "usertree",
                        principalColumn: "UserTreeID");
                    table.ForeignKey(
                        name: "FK_tasks_workspace_WorkspaceId",
                        column: x => x.WorkspaceId,
                        principalTable: "workspace",
                        principalColumn: "WorkspaceID",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "tasks_ibfk_1",
                        column: x => x.UserID,
                        principalTable: "users",
                        principalColumn: "UserID");
                })
                .Annotation("MySql:CharSet", "utf8mb4")
                .Annotation("Relational:Collation", "utf8mb4_0900_ai_ci");

            migrationBuilder.CreateTable(
                name: "workspaceitem",
                columns: table => new
                {
                    WorkspaceItemID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    WorkspaceID = table.Column<int>(type: "int", nullable: true),
                    ItemID = table.Column<int>(type: "int", nullable: true),
                    UserID = table.Column<int>(type: "int", nullable: true),
                    Quantity = table.Column<int>(type: "int", nullable: true),
                    Effect = table.Column<string>(type: "json", nullable: true, collation: "utf8mb4_0900_ai_ci")
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    CreatedAt = table.Column<DateTime>(type: "timestamp", nullable: true, defaultValueSql: "CURRENT_TIMESTAMP")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PRIMARY", x => x.WorkspaceItemID);
                    table.ForeignKey(
                        name: "workspaceitem_ibfk_1",
                        column: x => x.WorkspaceID,
                        principalTable: "workspace",
                        principalColumn: "WorkspaceID");
                    table.ForeignKey(
                        name: "workspaceitem_ibfk_2",
                        column: x => x.ItemID,
                        principalTable: "item",
                        principalColumn: "ItemID");
                    table.ForeignKey(
                        name: "workspaceitem_ibfk_3",
                        column: x => x.UserID,
                        principalTable: "users",
                        principalColumn: "UserID");
                })
                .Annotation("MySql:CharSet", "utf8mb4")
                .Annotation("Relational:Collation", "utf8mb4_0900_ai_ci");

            migrationBuilder.CreateTable(
                name: "TaskFocusSetting",
                columns: table => new
                {
                    TaskFocusSettingId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    TaskId = table.Column<int>(type: "int", nullable: false),
                    FocusMethodId = table.Column<int>(type: "int", nullable: false),
                    SuggestedDuration = table.Column<int>(type: "int", nullable: false),
                    CustomDuration = table.Column<int>(type: "int", nullable: true),
                    SuggestedBreak = table.Column<int>(type: "int", nullable: false),
                    CustomBreak = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TaskFocusSetting", x => x.TaskFocusSettingId);
                    table.ForeignKey(
                        name: "FK_TaskFocusSetting_focusmethod_FocusMethodId",
                        column: x => x.FocusMethodId,
                        principalTable: "focusmethod",
                        principalColumn: "FocusMethodID",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_TaskFocusSetting_tasks_TaskId",
                        column: x => x.TaskId,
                        principalTable: "tasks",
                        principalColumn: "TaskID",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySql:CharSet", "utf8mb4")
                .Annotation("Relational:Collation", "utf8mb4_0900_ai_ci");

            migrationBuilder.CreateIndex(
                name: "idx_bag_user",
                table: "bag",
                column: "UserID");

            migrationBuilder.CreateIndex(
                name: "IX_bag_UserID",
                table: "bag",
                column: "UserID",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "BagID",
                table: "bagitem",
                column: "BagID");

            migrationBuilder.CreateIndex(
                name: "ItemID",
                table: "bagitem",
                column: "ItemID");

            migrationBuilder.CreateIndex(
                name: "idx_item_type_rarity",
                table: "item",
                columns: new[] { "Type", "Rarity" });

            migrationBuilder.CreateIndex(
                name: "idx_item_detail_itemid",
                table: "itemdetail",
                column: "ItemID",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "UserID1",
                table: "leaderboard",
                column: "UserID");

            migrationBuilder.CreateIndex(
                name: "ItemID2",
                table: "purchasehistory",
                column: "ItemID");

            migrationBuilder.CreateIndex(
                name: "UserID2",
                table: "purchasehistory",
                column: "UserID");

            migrationBuilder.CreateIndex(
                name: "IX_TaskFocusSetting_FocusMethodId",
                table: "TaskFocusSetting",
                column: "FocusMethodId");

            migrationBuilder.CreateIndex(
                name: "IX_TaskFocusSetting_TaskId",
                table: "TaskFocusSetting",
                column: "TaskId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "idx_task_user",
                table: "tasks",
                column: "UserID");

            migrationBuilder.CreateIndex(
                name: "IX_tasks_UserTreeID",
                table: "tasks",
                column: "UserTreeID");

            migrationBuilder.CreateIndex(
                name: "IX_tasks_WorkspaceId",
                table: "tasks",
                column: "WorkspaceId");

            migrationBuilder.CreateIndex(
                name: "idx_tradehistory_user",
                table: "tradehistory",
                columns: new[] { "UserAID", "UserBID" });

            migrationBuilder.CreateIndex(
                name: "UserBID",
                table: "tradehistory",
                column: "UserBID");

            migrationBuilder.CreateIndex(
                name: "UserTreeAID",
                table: "tradehistory",
                column: "UserTreeAID");

            migrationBuilder.CreateIndex(
                name: "UserTreeBID",
                table: "tradehistory",
                column: "UserTreeBID");

            migrationBuilder.CreateIndex(
                name: "idx_transaction_status",
                table: "transactions",
                column: "Status");

            migrationBuilder.CreateIndex(
                name: "idx_transaction_user",
                table: "transactions",
                column: "UserID");

            migrationBuilder.CreateIndex(
                name: "WalletID",
                table: "transactions",
                column: "WalletID");

            migrationBuilder.CreateIndex(
                name: "IX_userexperience_LevelId",
                table: "userexperience",
                column: "LevelId");

            migrationBuilder.CreateIndex(
                name: "Email",
                table: "users",
                column: "Email",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "RoleID",
                table: "users",
                column: "RoleID");

            migrationBuilder.CreateIndex(
                name: "FinalTreeID",
                table: "usertree",
                column: "FinalTreeID");

            migrationBuilder.CreateIndex(
                name: "UserID4",
                table: "usertree",
                column: "UserID");

            migrationBuilder.CreateIndex(
                name: "IX_wallet_UserID",
                table: "wallet",
                column: "UserID",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "UserID5",
                table: "wallet",
                column: "UserID");

            migrationBuilder.CreateIndex(
                name: "IX_workspace_UserID",
                table: "workspace",
                column: "UserID",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "UserID6",
                table: "workspace",
                column: "UserID");

            migrationBuilder.CreateIndex(
                name: "idx_workspace_item_user",
                table: "workspaceitem",
                column: "UserID");

            migrationBuilder.CreateIndex(
                name: "ItemID3",
                table: "workspaceitem",
                column: "ItemID");

            migrationBuilder.CreateIndex(
                name: "WorkspaceID",
                table: "workspaceitem",
                column: "WorkspaceID");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "bagitem");

            migrationBuilder.DropTable(
                name: "itemdetail");

            migrationBuilder.DropTable(
                name: "leaderboard");

            migrationBuilder.DropTable(
                name: "purchasehistory");

            migrationBuilder.DropTable(
                name: "TaskFocusSetting");

            migrationBuilder.DropTable(
                name: "tradehistory");

            migrationBuilder.DropTable(
                name: "transactions");

            migrationBuilder.DropTable(
                name: "userexperience");

            migrationBuilder.DropTable(
                name: "workspaceitem");

            migrationBuilder.DropTable(
                name: "bag");

            migrationBuilder.DropTable(
                name: "focusmethod");

            migrationBuilder.DropTable(
                name: "tasks");

            migrationBuilder.DropTable(
                name: "wallet");

            migrationBuilder.DropTable(
                name: "UserLevelConfig");

            migrationBuilder.DropTable(
                name: "item");

            migrationBuilder.DropTable(
                name: "usertree");

            migrationBuilder.DropTable(
                name: "workspace");

            migrationBuilder.DropTable(
                name: "treetype");

            migrationBuilder.DropTable(
                name: "users");

            migrationBuilder.DropTable(
                name: "roles");
        }
    }
}
