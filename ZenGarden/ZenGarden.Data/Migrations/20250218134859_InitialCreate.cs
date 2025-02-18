using System;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ZenGarden.Data.Migrations
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
                name: "taskstatus",
                columns: table => new
                {
                    StatusID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    StatusName = table.Column<string>(type: "varchar(50)", maxLength: 50, nullable: true, collation: "utf8mb4_0900_ai_ci")
                        .Annotation("MySql:CharSet", "utf8mb4")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PRIMARY", x => x.StatusID);
                })
                .Annotation("MySql:CharSet", "utf8mb4")
                .Annotation("Relational:Collation", "utf8mb4_0900_ai_ci");

            migrationBuilder.CreateTable(
                name: "tradestatus",
                columns: table => new
                {
                    StatusID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    StatusName = table.Column<string>(type: "varchar(50)", maxLength: 50, nullable: true, collation: "utf8mb4_0900_ai_ci")
                        .Annotation("MySql:CharSet", "utf8mb4")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PRIMARY", x => x.StatusID);
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
                name: "dailyreward",
                columns: table => new
                {
                    DailyID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    Day = table.Column<int>(type: "int", nullable: true),
                    Reward = table.Column<string>(type: "varchar(100)", maxLength: 100, nullable: true, collation: "utf8mb4_0900_ai_ci")
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    RewardType = table.Column<string>(type: "varchar(50)", maxLength: 50, nullable: true, collation: "utf8mb4_0900_ai_ci")
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    ItemID = table.Column<int>(type: "int", nullable: true),
                    Quantity = table.Column<int>(type: "int", nullable: true),
                    ConditionType = table.Column<string>(type: "varchar(50)", maxLength: 50, nullable: true, collation: "utf8mb4_0900_ai_ci")
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    ConditionValue = table.Column<string>(type: "varchar(100)", maxLength: 100, nullable: true, collation: "utf8mb4_0900_ai_ci")
                        .Annotation("MySql:CharSet", "utf8mb4")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PRIMARY", x => x.DailyID);
                    table.ForeignKey(
                        name: "dailyreward_ibfk_1",
                        column: x => x.ItemID,
                        principalTable: "item",
                        principalColumn: "ItemID");
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
                    FullName = table.Column<string>(type: "varchar(100)", maxLength: 100, nullable: true, collation: "utf8mb4_0900_ai_ci")
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Email = table.Column<string>(type: "varchar(100)", maxLength: 100, nullable: true, collation: "utf8mb4_0900_ai_ci")
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Password = table.Column<string>(type: "varchar(255)", maxLength: 255, nullable: true, collation: "utf8mb4_0900_ai_ci")
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Phone = table.Column<string>(type: "varchar(20)", maxLength: 20, nullable: true, collation: "utf8mb4_0900_ai_ci")
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Status = table.Column<string>(type: "varchar(20)", maxLength: 20, nullable: true, collation: "utf8mb4_0900_ai_ci")
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    CreatedAt = table.Column<DateTime>(type: "timestamp", nullable: true, defaultValueSql: "CURRENT_TIMESTAMP"),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp", nullable: true, defaultValueSql: "CURRENT_TIMESTAMP")
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
                    UserID = table.Column<int>(type: "int", nullable: true),
                    Capacity = table.Column<int>(type: "int", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp", nullable: true, defaultValueSql: "CURRENT_TIMESTAMP"),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp", nullable: true, defaultValueSql: "CURRENT_TIMESTAMP")
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.ComputedColumn)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PRIMARY", x => x.BagID);
                    table.ForeignKey(
                        name: "bag_ibfk_1",
                        column: x => x.UserID,
                        principalTable: "users",
                        principalColumn: "UserID");
                })
                .Annotation("MySql:CharSet", "utf8mb4")
                .Annotation("Relational:Collation", "utf8mb4_0900_ai_ci");

            migrationBuilder.CreateTable(
                name: "deposittransaction",
                columns: table => new
                {
                    DepositID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    UserID = table.Column<int>(type: "int", nullable: true),
                    Amount = table.Column<decimal>(type: "decimal(10,2)", precision: 10, scale: 2, nullable: true),
                    PaymentMethod = table.Column<string>(type: "varchar(50)", maxLength: 50, nullable: true, collation: "utf8mb4_0900_ai_ci")
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    PaymentStatus = table.Column<string>(type: "varchar(20)", maxLength: 20, nullable: true, collation: "utf8mb4_0900_ai_ci")
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    TransactionReference = table.Column<string>(type: "varchar(100)", maxLength: 100, nullable: true, collation: "utf8mb4_0900_ai_ci")
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    CreatedAt = table.Column<DateTime>(type: "timestamp", nullable: true, defaultValueSql: "CURRENT_TIMESTAMP"),
                    CompletedAt = table.Column<DateTime>(type: "timestamp", nullable: true, defaultValueSql: "CURRENT_TIMESTAMP")
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.ComputedColumn)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PRIMARY", x => x.DepositID);
                    table.ForeignKey(
                        name: "deposittransaction_ibfk_1",
                        column: x => x.UserID,
                        principalTable: "users",
                        principalColumn: "UserID");
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
                name: "task",
                columns: table => new
                {
                    TaskID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    UserID = table.Column<int>(type: "int", nullable: true),
                    TaskName = table.Column<string>(type: "varchar(255)", maxLength: 255, nullable: true, collation: "utf8mb4_0900_ai_ci")
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    TaskDescription = table.Column<string>(type: "text", nullable: true, collation: "utf8mb4_0900_ai_ci")
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Duration = table.Column<int>(type: "int", nullable: true),
                    AIProcessedDescription = table.Column<string>(type: "text", nullable: true, collation: "utf8mb4_0900_ai_ci")
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    TimeOverdue = table.Column<int>(type: "int", nullable: true),
                    StatusID = table.Column<int>(type: "int", nullable: true),
                    XPReward = table.Column<int>(type: "int", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp", nullable: true, defaultValueSql: "CURRENT_TIMESTAMP"),
                    CompletedAt = table.Column<DateTime>(type: "timestamp", nullable: true, defaultValueSql: "CURRENT_TIMESTAMP")
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.ComputedColumn)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PRIMARY", x => x.TaskID);
                    table.ForeignKey(
                        name: "task_ibfk_1",
                        column: x => x.UserID,
                        principalTable: "users",
                        principalColumn: "UserID");
                    table.ForeignKey(
                        name: "task_ibfk_2",
                        column: x => x.StatusID,
                        principalTable: "taskstatus",
                        principalColumn: "StatusID");
                })
                .Annotation("MySql:CharSet", "utf8mb4")
                .Annotation("Relational:Collation", "utf8mb4_0900_ai_ci");

            migrationBuilder.CreateTable(
                name: "userexperience",
                columns: table => new
                {
                    UserExperienceID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    UserID = table.Column<int>(type: "int", nullable: true),
                    TotalXP = table.Column<long>(type: "bigint", nullable: true),
                    PreviousLevel = table.Column<int>(type: "int", nullable: true),
                    XPToNextLevel = table.Column<int>(type: "int", nullable: true),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp", nullable: true, defaultValueSql: "CURRENT_TIMESTAMP")
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.ComputedColumn)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PRIMARY", x => x.UserExperienceID);
                    table.ForeignKey(
                        name: "userexperience_ibfk_1",
                        column: x => x.UserID,
                        principalTable: "users",
                        principalColumn: "UserID");
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
                    TreeLevel = table.Column<int>(type: "int", nullable: true),
                    XP = table.Column<int>(type: "int", nullable: true),
                    TreeStatus = table.Column<string>(type: "enum('Growing','Mature','MaxLevel')", nullable: true, defaultValueSql: "'Growing'", collation: "utf8mb4_0900_ai_ci")
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    FinalTreeRarity = table.Column<string>(type: "varchar(50)", maxLength: 50, nullable: true, collation: "utf8mb4_0900_ai_ci")
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    LastUpdated = table.Column<DateTime>(type: "timestamp", nullable: true, defaultValueSql: "CURRENT_TIMESTAMP")
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.ComputedColumn)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PRIMARY", x => x.UserTreeID);
                    table.ForeignKey(
                        name: "usertree_ibfk_1",
                        column: x => x.UserID,
                        principalTable: "users",
                        principalColumn: "UserID",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "usertree_ibfk_2",
                        column: x => x.FinalTreeID,
                        principalTable: "treetype",
                        principalColumn: "TreeTypeID");
                })
                .Annotation("MySql:CharSet", "utf8mb4")
                .Annotation("Relational:Collation", "utf8mb4_0900_ai_ci");

            migrationBuilder.CreateTable(
                name: "wallet",
                columns: table => new
                {
                    WalletID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    UserID = table.Column<int>(type: "int", nullable: true),
                    Balance = table.Column<decimal>(type: "decimal(10,2)", precision: 10, scale: 2, nullable: true, defaultValueSql: "'0.00'"),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp", nullable: true, defaultValueSql: "CURRENT_TIMESTAMP")
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.ComputedColumn)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PRIMARY", x => x.WalletID);
                    table.ForeignKey(
                        name: "wallet_ibfk_1",
                        column: x => x.UserID,
                        principalTable: "users",
                        principalColumn: "UserID");
                })
                .Annotation("MySql:CharSet", "utf8mb4")
                .Annotation("Relational:Collation", "utf8mb4_0900_ai_ci");

            migrationBuilder.CreateTable(
                name: "workspace",
                columns: table => new
                {
                    WorkspaceID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    UserID = table.Column<int>(type: "int", nullable: true),
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
                        name: "workspace_ibfk_1",
                        column: x => x.UserID,
                        principalTable: "users",
                        principalColumn: "UserID");
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
                name: "dailyrewardclaim",
                columns: table => new
                {
                    ClaimID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    UserID = table.Column<int>(type: "int", nullable: true),
                    DailyID = table.Column<int>(type: "int", nullable: true),
                    TaskID = table.Column<int>(type: "int", nullable: true),
                    ClaimedAt = table.Column<DateTime>(type: "timestamp", nullable: true, defaultValueSql: "CURRENT_TIMESTAMP")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PRIMARY", x => x.ClaimID);
                    table.ForeignKey(
                        name: "dailyrewardclaim_ibfk_1",
                        column: x => x.UserID,
                        principalTable: "users",
                        principalColumn: "UserID");
                    table.ForeignKey(
                        name: "dailyrewardclaim_ibfk_2",
                        column: x => x.TaskID,
                        principalTable: "task",
                        principalColumn: "TaskID");
                })
                .Annotation("MySql:CharSet", "utf8mb4")
                .Annotation("Relational:Collation", "utf8mb4_0900_ai_ci");

            migrationBuilder.CreateTable(
                name: "useractivity",
                columns: table => new
                {
                    ActivityID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    UserID = table.Column<int>(type: "int", nullable: true),
                    FocusMethodID = table.Column<int>(type: "int", nullable: true),
                    TaskID = table.Column<int>(type: "int", nullable: true),
                    StartTime = table.Column<DateTime>(type: "timestamp", nullable: true),
                    EndTime = table.Column<DateTime>(type: "timestamp", nullable: true),
                    SuggestedDuration = table.Column<int>(type: "int", nullable: true),
                    UserAdjustedDuration = table.Column<int>(type: "int", nullable: true),
                    SuggestedBreak = table.Column<int>(type: "int", nullable: true),
                    UserAdjustedBreak = table.Column<int>(type: "int", nullable: true),
                    Interruptions = table.Column<int>(type: "int", nullable: true),
                    MouseClicks = table.Column<int>(type: "int", nullable: true, defaultValueSql: "'0'"),
                    MouseScrolls = table.Column<int>(type: "int", nullable: true, defaultValueSql: "'0'"),
                    Keystrokes = table.Column<int>(type: "int", nullable: true, defaultValueSql: "'0'"),
                    ActiveTime = table.Column<int>(type: "int", nullable: true),
                    InactiveTime = table.Column<int>(type: "int", nullable: true),
                    FocusScore = table.Column<int>(type: "int", nullable: true),
                    WarningSpent = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PRIMARY", x => x.ActivityID);
                    table.ForeignKey(
                        name: "useractivity_ibfk_1",
                        column: x => x.UserID,
                        principalTable: "users",
                        principalColumn: "UserID");
                    table.ForeignKey(
                        name: "useractivity_ibfk_2",
                        column: x => x.FocusMethodID,
                        principalTable: "focusmethod",
                        principalColumn: "FocusMethodID");
                    table.ForeignKey(
                        name: "useractivity_ibfk_3",
                        column: x => x.TaskID,
                        principalTable: "task",
                        principalColumn: "TaskID");
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
                    StatusID = table.Column<int>(type: "int", nullable: true),
                    RequestedAt = table.Column<DateTime>(type: "timestamp", nullable: true, defaultValueSql: "CURRENT_TIMESTAMP"),
                    CompletedAt = table.Column<DateTime>(type: "timestamp", nullable: true)
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
                    table.ForeignKey(
                        name: "tradehistory_ibfk_5",
                        column: x => x.StatusID,
                        principalTable: "tradestatus",
                        principalColumn: "StatusID");
                })
                .Annotation("MySql:CharSet", "utf8mb4")
                .Annotation("Relational:Collation", "utf8mb4_0900_ai_ci");

            migrationBuilder.CreateTable(
                name: "treeprogress",
                columns: table => new
                {
                    ProgressID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    UserTreeID = table.Column<int>(type: "int", nullable: true),
                    XPRequired = table.Column<int>(type: "int", nullable: true),
                    MaxTreesPerPeriod = table.Column<int>(type: "int", nullable: true),
                    PeriodType = table.Column<string>(type: "enum('Daily','Weekly','Monthly')", nullable: true, collation: "utf8mb4_0900_ai_ci")
                        .Annotation("MySql:CharSet", "utf8mb4")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PRIMARY", x => x.ProgressID);
                    table.ForeignKey(
                        name: "treeprogress_ibfk_1",
                        column: x => x.UserTreeID,
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
                    TransactionType = table.Column<string>(type: "varchar(50)", maxLength: 50, nullable: true, collation: "utf8mb4_0900_ai_ci")
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Status = table.Column<string>(type: "varchar(20)", maxLength: 20, nullable: true, collation: "utf8mb4_0900_ai_ci")
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    CommissionFee = table.Column<decimal>(type: "decimal(10,2)", precision: 10, scale: 2, nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp", nullable: true, defaultValueSql: "CURRENT_TIMESTAMP")
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

            migrationBuilder.CreateIndex(
                name: "idx_bag_user",
                table: "bag",
                column: "UserID");

            migrationBuilder.CreateIndex(
                name: "BagID",
                table: "bagitem",
                column: "BagID");

            migrationBuilder.CreateIndex(
                name: "ItemID",
                table: "bagitem",
                column: "ItemID");

            migrationBuilder.CreateIndex(
                name: "ItemID1",
                table: "dailyreward",
                column: "ItemID");

            migrationBuilder.CreateIndex(
                name: "idx_daily_reward_claim_user",
                table: "dailyrewardclaim",
                column: "UserID");

            migrationBuilder.CreateIndex(
                name: "TaskID",
                table: "dailyrewardclaim",
                column: "TaskID");

            migrationBuilder.CreateIndex(
                name: "UserID",
                table: "deposittransaction",
                column: "UserID");

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
                name: "idx_task_user",
                table: "task",
                column: "UserID");

            migrationBuilder.CreateIndex(
                name: "StatusID",
                table: "task",
                column: "StatusID");

            migrationBuilder.CreateIndex(
                name: "StatusName",
                table: "taskstatus",
                column: "StatusName",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "idx_tradehistory_statusid",
                table: "tradehistory",
                column: "StatusID");

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
                name: "UserTreeID",
                table: "treeprogress",
                column: "UserTreeID");

            migrationBuilder.CreateIndex(
                name: "FocusMethodID",
                table: "useractivity",
                column: "FocusMethodID");

            migrationBuilder.CreateIndex(
                name: "idx_user_activity_user",
                table: "useractivity",
                column: "UserID");

            migrationBuilder.CreateIndex(
                name: "TaskID1",
                table: "useractivity",
                column: "TaskID");

            migrationBuilder.CreateIndex(
                name: "UserID3",
                table: "userexperience",
                column: "UserID");

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
                name: "UserID5",
                table: "wallet",
                column: "UserID");

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
                name: "dailyreward");

            migrationBuilder.DropTable(
                name: "dailyrewardclaim");

            migrationBuilder.DropTable(
                name: "deposittransaction");

            migrationBuilder.DropTable(
                name: "itemdetail");

            migrationBuilder.DropTable(
                name: "leaderboard");

            migrationBuilder.DropTable(
                name: "purchasehistory");

            migrationBuilder.DropTable(
                name: "tradehistory");

            migrationBuilder.DropTable(
                name: "transactions");

            migrationBuilder.DropTable(
                name: "treeprogress");

            migrationBuilder.DropTable(
                name: "useractivity");

            migrationBuilder.DropTable(
                name: "userexperience");

            migrationBuilder.DropTable(
                name: "workspaceitem");

            migrationBuilder.DropTable(
                name: "bag");

            migrationBuilder.DropTable(
                name: "tradestatus");

            migrationBuilder.DropTable(
                name: "wallet");

            migrationBuilder.DropTable(
                name: "usertree");

            migrationBuilder.DropTable(
                name: "focusmethod");

            migrationBuilder.DropTable(
                name: "task");

            migrationBuilder.DropTable(
                name: "workspace");

            migrationBuilder.DropTable(
                name: "item");

            migrationBuilder.DropTable(
                name: "treetype");

            migrationBuilder.DropTable(
                name: "taskstatus");

            migrationBuilder.DropTable(
                name: "users");

            migrationBuilder.DropTable(
                name: "roles");
        }
    }
}
