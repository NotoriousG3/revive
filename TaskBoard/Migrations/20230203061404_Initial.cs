using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TaskBoard.Migrations
{
    /// <inheritdoc />
    public partial class Initial : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "AccountGroups",
                columns: table => new
                {
                    Id = table.Column<long>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Name = table.Column<string>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AccountGroups", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "AppSettings",
                columns: table => new
                {
                    Id = table.Column<long>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    ApiKey = table.Column<string>(type: "TEXT", nullable: true),
                    FiveSimApiKey = table.Column<string>(type: "TEXT", nullable: true),
                    TwilioApiKey = table.Column<string>(type: "TEXT", nullable: true),
                    TextVerifiedApiKey = table.Column<string>(type: "TEXT", nullable: true),
                    SmsActivateApiKey = table.Column<string>(type: "TEXT", nullable: true),
                    SmsPoolApiKey = table.Column<string>(type: "TEXT", nullable: true),
                    NamsorApiKey = table.Column<string>(type: "TEXT", nullable: true),
                    KopeechkaApiKey = table.Column<string>(type: "TEXT", nullable: true),
                    ProxyScraping = table.Column<bool>(type: "INTEGER", nullable: false),
                    ProxyChecking = table.Column<bool>(type: "INTEGER", nullable: false),
                    MaxRegisterAttempts = table.Column<int>(type: "INTEGER", nullable: false),
                    Threads = table.Column<int>(type: "INTEGER", nullable: false),
                    MaxTasks = table.Column<int>(type: "INTEGER", nullable: false),
                    Timeout = table.Column<int>(type: "INTEGER", nullable: false),
                    EnableDebug = table.Column<bool>(type: "INTEGER", nullable: false),
                    EnableBandwidthSaver = table.Column<bool>(type: "INTEGER", nullable: false),
                    EnableChromeBrowser = table.Column<bool>(type: "INTEGER", nullable: false),
                    EnableStealth = table.Column<bool>(type: "INTEGER", nullable: false),
                    MaxManagedAccounts = table.Column<int>(type: "INTEGER", nullable: false),
                    MaxAddFriendsUsers = table.Column<int>(type: "INTEGER", nullable: false),
                    MaxQuotaMb = table.Column<long>(type: "INTEGER", nullable: false),
                    AccountCooldown = table.Column<int>(type: "INTEGER", nullable: false),
                    AccessDeadline = table.Column<DateTime>(type: "TEXT", nullable: true),
                    DefaultOs = table.Column<int>(type: "INTEGER", nullable: true),
                    MaxRetries = table.Column<int>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AppSettings", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "AspNetRoles",
                columns: table => new
                {
                    Id = table.Column<string>(type: "TEXT", nullable: false),
                    Name = table.Column<string>(type: "TEXT", maxLength: 256, nullable: true),
                    NormalizedName = table.Column<string>(type: "TEXT", maxLength: 256, nullable: true),
                    ConcurrencyStamp = table.Column<string>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AspNetRoles", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "AspNetUsers",
                columns: table => new
                {
                    Id = table.Column<string>(type: "TEXT", nullable: false),
                    UserName = table.Column<string>(type: "TEXT", maxLength: 256, nullable: true),
                    NormalizedUserName = table.Column<string>(type: "TEXT", maxLength: 256, nullable: true),
                    Email = table.Column<string>(type: "TEXT", maxLength: 256, nullable: true),
                    NormalizedEmail = table.Column<string>(type: "TEXT", maxLength: 256, nullable: true),
                    EmailConfirmed = table.Column<bool>(type: "INTEGER", nullable: false),
                    PasswordHash = table.Column<string>(type: "TEXT", nullable: true),
                    SecurityStamp = table.Column<string>(type: "TEXT", nullable: true),
                    ConcurrencyStamp = table.Column<string>(type: "TEXT", nullable: true),
                    PhoneNumber = table.Column<string>(type: "TEXT", nullable: true),
                    PhoneNumberConfirmed = table.Column<bool>(type: "INTEGER", nullable: false),
                    TwoFactorEnabled = table.Column<bool>(type: "INTEGER", nullable: false),
                    LockoutEnd = table.Column<DateTimeOffset>(type: "TEXT", nullable: true),
                    LockoutEnabled = table.Column<bool>(type: "INTEGER", nullable: false),
                    AccessFailedCount = table.Column<int>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AspNetUsers", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "BannedAccountDeletionLog",
                columns: table => new
                {
                    Id = table.Column<long>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    DeletionTime = table.Column<DateTime>(type: "TEXT", nullable: false),
                    Username = table.Column<string>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BannedAccountDeletionLog", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Bitmojis",
                columns: table => new
                {
                    Id = table.Column<long>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Name = table.Column<string>(type: "TEXT", nullable: false),
                    Gender = table.Column<int>(type: "INTEGER", nullable: false),
                    Style = table.Column<int>(type: "INTEGER", nullable: false),
                    Rotation = table.Column<int>(type: "INTEGER", nullable: false),
                    Body = table.Column<int>(type: "INTEGER", nullable: false),
                    Bottom = table.Column<int>(type: "INTEGER", nullable: false),
                    BottomTone1 = table.Column<int>(type: "INTEGER", nullable: false),
                    BottomTone2 = table.Column<int>(type: "INTEGER", nullable: false),
                    BottomTone3 = table.Column<int>(type: "INTEGER", nullable: false),
                    BottomTone4 = table.Column<int>(type: "INTEGER", nullable: false),
                    BottomTone5 = table.Column<int>(type: "INTEGER", nullable: false),
                    BottomTone6 = table.Column<int>(type: "INTEGER", nullable: false),
                    BottomTone7 = table.Column<int>(type: "INTEGER", nullable: false),
                    BottomTone8 = table.Column<int>(type: "INTEGER", nullable: false),
                    BottomTone9 = table.Column<int>(type: "INTEGER", nullable: false),
                    BottomTone10 = table.Column<int>(type: "INTEGER", nullable: false),
                    Brow = table.Column<int>(type: "INTEGER", nullable: false),
                    ClothingType = table.Column<int>(type: "INTEGER", nullable: false),
                    Ear = table.Column<int>(type: "INTEGER", nullable: false),
                    Eye = table.Column<int>(type: "INTEGER", nullable: false),
                    Eyelash = table.Column<int>(type: "INTEGER", nullable: false),
                    FaceProportion = table.Column<int>(type: "INTEGER", nullable: false),
                    Footwear = table.Column<int>(type: "INTEGER", nullable: false),
                    FootwearTone1 = table.Column<int>(type: "INTEGER", nullable: false),
                    FootwearTone2 = table.Column<int>(type: "INTEGER", nullable: false),
                    FootwearTone3 = table.Column<int>(type: "INTEGER", nullable: false),
                    FootwearTone4 = table.Column<int>(type: "INTEGER", nullable: false),
                    FootwearTone5 = table.Column<int>(type: "INTEGER", nullable: false),
                    FootwearTone6 = table.Column<int>(type: "INTEGER", nullable: false),
                    FootwearTone7 = table.Column<int>(type: "INTEGER", nullable: false),
                    FootwearTone8 = table.Column<int>(type: "INTEGER", nullable: false),
                    FootwearTone9 = table.Column<int>(type: "INTEGER", nullable: false),
                    FootwearTone10 = table.Column<int>(type: "INTEGER", nullable: false),
                    Hair = table.Column<int>(type: "INTEGER", nullable: false),
                    HairTone = table.Column<int>(type: "INTEGER", nullable: false),
                    IsTucked = table.Column<int>(type: "INTEGER", nullable: false),
                    Jaw = table.Column<int>(type: "INTEGER", nullable: false),
                    Mouth = table.Column<int>(type: "INTEGER", nullable: false),
                    Nose = table.Column<int>(type: "INTEGER", nullable: false),
                    Pupil = table.Column<int>(type: "INTEGER", nullable: false),
                    PupilTone = table.Column<int>(type: "INTEGER", nullable: false),
                    SkinTone = table.Column<int>(type: "INTEGER", nullable: false),
                    Sock = table.Column<int>(type: "INTEGER", nullable: false),
                    SockTone1 = table.Column<int>(type: "INTEGER", nullable: false),
                    SockTone2 = table.Column<int>(type: "INTEGER", nullable: false),
                    SockTone3 = table.Column<int>(type: "INTEGER", nullable: false),
                    SockTone4 = table.Column<int>(type: "INTEGER", nullable: false),
                    Top = table.Column<int>(type: "INTEGER", nullable: false),
                    TopTone1 = table.Column<int>(type: "INTEGER", nullable: false),
                    TopTone2 = table.Column<int>(type: "INTEGER", nullable: false),
                    TopTone3 = table.Column<int>(type: "INTEGER", nullable: false),
                    TopTone4 = table.Column<int>(type: "INTEGER", nullable: false),
                    TopTone5 = table.Column<int>(type: "INTEGER", nullable: false),
                    TopTone6 = table.Column<int>(type: "INTEGER", nullable: false),
                    TopTone7 = table.Column<int>(type: "INTEGER", nullable: false),
                    TopTone8 = table.Column<int>(type: "INTEGER", nullable: false),
                    TopTone9 = table.Column<int>(type: "INTEGER", nullable: false),
                    TopTone10 = table.Column<int>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Bitmojis", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "EmailList",
                columns: table => new
                {
                    Id = table.Column<long>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Address = table.Column<string>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_EmailList", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Keywords",
                columns: table => new
                {
                    Id = table.Column<long>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Name = table.Column<string>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Keywords", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Macros",
                columns: table => new
                {
                    Id = table.Column<long>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Text = table.Column<string>(type: "TEXT", nullable: false),
                    Replacement = table.Column<string>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Macros", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "MediaFiles",
                columns: table => new
                {
                    Id = table.Column<long>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Name = table.Column<string>(type: "TEXT", nullable: false),
                    ServerPath = table.Column<string>(type: "TEXT", nullable: false),
                    SizeBytes = table.Column<long>(type: "INTEGER", nullable: false),
                    LastAccess = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MediaFiles", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Names",
                columns: table => new
                {
                    Id = table.Column<long>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    FirstName = table.Column<string>(type: "TEXT", nullable: false),
                    LastName = table.Column<string>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Names", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "PhoneList",
                columns: table => new
                {
                    Id = table.Column<long>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Number = table.Column<string>(type: "TEXT", nullable: false),
                    CountryCode = table.Column<string>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PhoneList", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Proxies",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Address = table.Column<string>(type: "TEXT", nullable: false),
                    User = table.Column<string>(type: "TEXT", nullable: true),
                    Password = table.Column<string>(type: "TEXT", nullable: true),
                    LastUsed = table.Column<DateTime>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Proxies", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "ProxyGroups",
                columns: table => new
                {
                    Id = table.Column<long>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Name = table.Column<string>(type: "TEXT", nullable: false),
                    ProxyType = table.Column<int>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ProxyGroups", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "TargetUsers",
                columns: table => new
                {
                    Id = table.Column<long>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Username = table.Column<string>(type: "TEXT", nullable: false),
                    CountryCode = table.Column<string>(type: "TEXT", nullable: true),
                    Gender = table.Column<string>(type: "TEXT", nullable: true),
                    Race = table.Column<string>(type: "TEXT", nullable: true),
                    Added = table.Column<bool>(type: "INTEGER", nullable: false),
                    Used = table.Column<bool>(type: "INTEGER", nullable: false),
                    Searched = table.Column<bool>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TargetUsers", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "UserNames",
                columns: table => new
                {
                    Id = table.Column<long>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    UserName = table.Column<string>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserNames", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "EnabledModules",
                columns: table => new
                {
                    Id = table.Column<long>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    ModuleId = table.Column<int>(type: "INTEGER", nullable: false),
                    AppSettingsId = table.Column<long>(type: "INTEGER", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_EnabledModules", x => x.Id);
                    table.ForeignKey(
                        name: "FK_EnabledModules_AppSettings_AppSettingsId",
                        column: x => x.AppSettingsId,
                        principalTable: "AppSettings",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "AspNetRoleClaims",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    RoleId = table.Column<string>(type: "TEXT", nullable: false),
                    ClaimType = table.Column<string>(type: "TEXT", nullable: true),
                    ClaimValue = table.Column<string>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AspNetRoleClaims", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AspNetRoleClaims_AspNetRoles_RoleId",
                        column: x => x.RoleId,
                        principalTable: "AspNetRoles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "AspNetUserClaims",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    UserId = table.Column<string>(type: "TEXT", nullable: false),
                    ClaimType = table.Column<string>(type: "TEXT", nullable: true),
                    ClaimValue = table.Column<string>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AspNetUserClaims", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AspNetUserClaims_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "AspNetUserLogins",
                columns: table => new
                {
                    LoginProvider = table.Column<string>(type: "TEXT", maxLength: 128, nullable: false),
                    ProviderKey = table.Column<string>(type: "TEXT", maxLength: 128, nullable: false),
                    ProviderDisplayName = table.Column<string>(type: "TEXT", nullable: true),
                    UserId = table.Column<string>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AspNetUserLogins", x => new { x.LoginProvider, x.ProviderKey });
                    table.ForeignKey(
                        name: "FK_AspNetUserLogins_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "AspNetUserRoles",
                columns: table => new
                {
                    UserId = table.Column<string>(type: "TEXT", nullable: false),
                    RoleId = table.Column<string>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AspNetUserRoles", x => new { x.UserId, x.RoleId });
                    table.ForeignKey(
                        name: "FK_AspNetUserRoles_AspNetRoles_RoleId",
                        column: x => x.RoleId,
                        principalTable: "AspNetRoles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_AspNetUserRoles_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "AspNetUserTokens",
                columns: table => new
                {
                    UserId = table.Column<string>(type: "TEXT", nullable: false),
                    LoginProvider = table.Column<string>(type: "TEXT", maxLength: 128, nullable: false),
                    Name = table.Column<string>(type: "TEXT", maxLength: 128, nullable: false),
                    Value = table.Column<string>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AspNetUserTokens", x => new { x.UserId, x.LoginProvider, x.Name });
                    table.ForeignKey(
                        name: "FK_AspNetUserTokens_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "WorkRequests",
                columns: table => new
                {
                    Id = table.Column<long>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Action = table.Column<int>(type: "INTEGER", nullable: false),
                    AccountsToUse = table.Column<int>(type: "INTEGER", nullable: false),
                    Arguments = table.Column<string>(type: "TEXT", nullable: false),
                    RequestTime = table.Column<DateTime>(type: "TEXT", nullable: false),
                    ScheduledTime = table.Column<DateTime>(type: "TEXT", nullable: true),
                    StartTime = table.Column<DateTime>(type: "TEXT", nullable: true),
                    FinishTime = table.Column<DateTime>(type: "TEXT", nullable: true),
                    AccountsFail = table.Column<int>(type: "INTEGER", nullable: false),
                    AccountsPass = table.Column<int>(type: "INTEGER", nullable: false),
                    ActionsPerAccount = table.Column<int>(type: "INTEGER", nullable: false),
                    FailedAccounts = table.Column<string>(type: "TEXT", nullable: true),
                    ExportedFriends = table.Column<string>(type: "TEXT", nullable: true),
                    Status = table.Column<int>(type: "INTEGER", nullable: false),
                    MediaFileId = table.Column<long>(type: "INTEGER", nullable: true),
                    PreviousWorkRequestId = table.Column<long>(type: "INTEGER", nullable: true),
                    ChainDelayMs = table.Column<long>(type: "INTEGER", nullable: true),
                    MinFriends = table.Column<int>(type: "INTEGER", nullable: false),
                    MaxFriends = table.Column<int>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_WorkRequests", x => x.Id);
                    table.ForeignKey(
                        name: "FK_WorkRequests_MediaFiles_MediaFileId",
                        column: x => x.MediaFileId,
                        principalTable: "MediaFiles",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_WorkRequests_WorkRequests_PreviousWorkRequestId",
                        column: x => x.PreviousWorkRequestId,
                        principalTable: "WorkRequests",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "Accounts",
                columns: table => new
                {
                    Id = table.Column<long>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Username = table.Column<string>(type: "TEXT", nullable: false),
                    Password = table.Column<string>(type: "TEXT", nullable: false),
                    Device = table.Column<string>(type: "TEXT", nullable: true),
                    Install = table.Column<string>(type: "TEXT", nullable: true),
                    UserId = table.Column<string>(type: "TEXT", nullable: true),
                    AuthToken = table.Column<string>(type: "TEXT", nullable: true),
                    DToken1I = table.Column<string>(type: "TEXT", nullable: true),
                    DToken1V = table.Column<string>(type: "TEXT", nullable: true),
                    InstallTime = table.Column<long>(type: "INTEGER", nullable: false),
                    OS = table.Column<int>(type: "INTEGER", nullable: false),
                    SnapchatVersion = table.Column<int>(type: "INTEGER", nullable: false),
                    CreationDate = table.Column<DateTime>(type: "TEXT", nullable: true),
                    ProxyId = table.Column<int>(type: "INTEGER", nullable: true),
                    AccountStatus = table.Column<int>(type: "INTEGER", nullable: false),
                    FriendCount = table.Column<int>(type: "INTEGER", nullable: false),
                    IncomingFriendCount = table.Column<int>(type: "INTEGER", nullable: false),
                    OutgoingFriendCount = table.Column<int>(type: "INTEGER", nullable: false),
                    DeviceProfile = table.Column<string>(type: "TEXT", nullable: true),
                    AccessToken = table.Column<string>(type: "TEXT", nullable: true),
                    BusinessAccessToken = table.Column<string>(type: "TEXT", nullable: true),
                    AccountCountryCode = table.Column<string>(type: "TEXT", nullable: true),
                    Horoscope = table.Column<int>(type: "INTEGER", nullable: false),
                    TimeZone = table.Column<string>(type: "TEXT", nullable: true),
                    ClientID = table.Column<string>(type: "TEXT", nullable: true),
                    PhoneValidated = table.Column<int>(type: "INTEGER", nullable: false),
                    EmailValidated = table.Column<int>(type: "INTEGER", nullable: false),
                    hasAdded = table.Column<bool>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Accounts", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Accounts_Proxies_ProxyId",
                        column: x => x.ProxyId,
                        principalTable: "Proxies",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "ProxyProxyGroup",
                columns: table => new
                {
                    GroupsId = table.Column<long>(type: "INTEGER", nullable: false),
                    ProxiesId = table.Column<int>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ProxyProxyGroup", x => new { x.GroupsId, x.ProxiesId });
                    table.ForeignKey(
                        name: "FK_ProxyProxyGroup_Proxies_ProxiesId",
                        column: x => x.ProxiesId,
                        principalTable: "Proxies",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ProxyProxyGroup_ProxyGroups_GroupsId",
                        column: x => x.GroupsId,
                        principalTable: "ProxyGroups",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ChosenTargets",
                columns: table => new
                {
                    Id = table.Column<long>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    TargetUserId = table.Column<long>(type: "INTEGER", nullable: false),
                    WorkId = table.Column<long>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ChosenTargets", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ChosenTargets_TargetUsers_TargetUserId",
                        column: x => x.TargetUserId,
                        principalTable: "TargetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ChosenTargets_WorkRequests_WorkId",
                        column: x => x.WorkId,
                        principalTable: "WorkRequests",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "LogEntries",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    LogLevel = table.Column<int>(type: "INTEGER", nullable: false),
                    Message = table.Column<string>(type: "TEXT", nullable: false),
                    WorkId = table.Column<long>(type: "INTEGER", nullable: false),
                    Time = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_LogEntries", x => x.Id);
                    table.ForeignKey(
                        name: "FK_LogEntries_WorkRequests_WorkId",
                        column: x => x.WorkId,
                        principalTable: "WorkRequests",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "AccountGroupSnapchatAccountModel",
                columns: table => new
                {
                    AccountsId = table.Column<long>(type: "INTEGER", nullable: false),
                    GroupsId = table.Column<long>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AccountGroupSnapchatAccountModel", x => new { x.AccountsId, x.GroupsId });
                    table.ForeignKey(
                        name: "FK_AccountGroupSnapchatAccountModel_AccountGroups_GroupsId",
                        column: x => x.GroupsId,
                        principalTable: "AccountGroups",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_AccountGroupSnapchatAccountModel_Accounts_AccountsId",
                        column: x => x.AccountsId,
                        principalTable: "Accounts",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ChosenAccounts",
                columns: table => new
                {
                    Id = table.Column<long>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    AccountId = table.Column<long>(type: "INTEGER", nullable: false),
                    WorkId = table.Column<long>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ChosenAccounts", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ChosenAccounts_Accounts_AccountId",
                        column: x => x.AccountId,
                        principalTable: "Accounts",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ChosenAccounts_WorkRequests_WorkId",
                        column: x => x.WorkId,
                        principalTable: "WorkRequests",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Emails",
                columns: table => new
                {
                    Address = table.Column<string>(type: "TEXT", nullable: false),
                    Password = table.Column<string>(type: "TEXT", nullable: true),
                    AccountId = table.Column<long>(type: "INTEGER", nullable: true),
                    IsFake = table.Column<bool>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Emails", x => x.Address);
                    table.ForeignKey(
                        name: "FK_Emails_Accounts_AccountId",
                        column: x => x.AccountId,
                        principalTable: "Accounts",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateIndex(
                name: "IX_AccountGroupSnapchatAccountModel_GroupsId",
                table: "AccountGroupSnapchatAccountModel",
                column: "GroupsId");

            migrationBuilder.CreateIndex(
                name: "IX_Accounts_ProxyId",
                table: "Accounts",
                column: "ProxyId");

            migrationBuilder.CreateIndex(
                name: "IX_AspNetRoleClaims_RoleId",
                table: "AspNetRoleClaims",
                column: "RoleId");

            migrationBuilder.CreateIndex(
                name: "RoleNameIndex",
                table: "AspNetRoles",
                column: "NormalizedName",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_AspNetUserClaims_UserId",
                table: "AspNetUserClaims",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_AspNetUserLogins_UserId",
                table: "AspNetUserLogins",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_AspNetUserRoles_RoleId",
                table: "AspNetUserRoles",
                column: "RoleId");

            migrationBuilder.CreateIndex(
                name: "EmailIndex",
                table: "AspNetUsers",
                column: "NormalizedEmail");

            migrationBuilder.CreateIndex(
                name: "UserNameIndex",
                table: "AspNetUsers",
                column: "NormalizedUserName",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ChosenAccounts_AccountId",
                table: "ChosenAccounts",
                column: "AccountId");

            migrationBuilder.CreateIndex(
                name: "IX_ChosenAccounts_WorkId",
                table: "ChosenAccounts",
                column: "WorkId");

            migrationBuilder.CreateIndex(
                name: "IX_ChosenTargets_TargetUserId",
                table: "ChosenTargets",
                column: "TargetUserId");

            migrationBuilder.CreateIndex(
                name: "IX_ChosenTargets_WorkId",
                table: "ChosenTargets",
                column: "WorkId");

            migrationBuilder.CreateIndex(
                name: "IX_Emails_AccountId",
                table: "Emails",
                column: "AccountId");

            migrationBuilder.CreateIndex(
                name: "IX_EnabledModules_AppSettingsId",
                table: "EnabledModules",
                column: "AppSettingsId");

            migrationBuilder.CreateIndex(
                name: "IX_LogEntries_WorkId",
                table: "LogEntries",
                column: "WorkId");

            migrationBuilder.CreateIndex(
                name: "IX_ProxyProxyGroup_ProxiesId",
                table: "ProxyProxyGroup",
                column: "ProxiesId");

            migrationBuilder.CreateIndex(
                name: "IX_WorkRequests_MediaFileId",
                table: "WorkRequests",
                column: "MediaFileId");

            migrationBuilder.CreateIndex(
                name: "IX_WorkRequests_PreviousWorkRequestId",
                table: "WorkRequests",
                column: "PreviousWorkRequestId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "AccountGroupSnapchatAccountModel");

            migrationBuilder.DropTable(
                name: "AspNetRoleClaims");

            migrationBuilder.DropTable(
                name: "AspNetUserClaims");

            migrationBuilder.DropTable(
                name: "AspNetUserLogins");

            migrationBuilder.DropTable(
                name: "AspNetUserRoles");

            migrationBuilder.DropTable(
                name: "AspNetUserTokens");

            migrationBuilder.DropTable(
                name: "BannedAccountDeletionLog");

            migrationBuilder.DropTable(
                name: "Bitmojis");

            migrationBuilder.DropTable(
                name: "ChosenAccounts");

            migrationBuilder.DropTable(
                name: "ChosenTargets");

            migrationBuilder.DropTable(
                name: "EmailList");

            migrationBuilder.DropTable(
                name: "Emails");

            migrationBuilder.DropTable(
                name: "EnabledModules");

            migrationBuilder.DropTable(
                name: "Keywords");

            migrationBuilder.DropTable(
                name: "LogEntries");

            migrationBuilder.DropTable(
                name: "Macros");

            migrationBuilder.DropTable(
                name: "Names");

            migrationBuilder.DropTable(
                name: "PhoneList");

            migrationBuilder.DropTable(
                name: "ProxyProxyGroup");

            migrationBuilder.DropTable(
                name: "UserNames");

            migrationBuilder.DropTable(
                name: "AccountGroups");

            migrationBuilder.DropTable(
                name: "AspNetRoles");

            migrationBuilder.DropTable(
                name: "AspNetUsers");

            migrationBuilder.DropTable(
                name: "TargetUsers");

            migrationBuilder.DropTable(
                name: "Accounts");

            migrationBuilder.DropTable(
                name: "AppSettings");

            migrationBuilder.DropTable(
                name: "WorkRequests");

            migrationBuilder.DropTable(
                name: "ProxyGroups");

            migrationBuilder.DropTable(
                name: "Proxies");

            migrationBuilder.DropTable(
                name: "MediaFiles");
        }
    }
}
