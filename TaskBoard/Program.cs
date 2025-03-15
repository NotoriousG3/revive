using System.Net.Sockets;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using Polly;
using TaskBoard;
using TaskBoard.Data;
using TaskBoard.WorkTask;

Policy
    .Handle<SocketException>()
    .Retry();

Policy
    .Handle<Grpc.Core.RpcException>()
    .Retry();


var builder = WebApplication.CreateBuilder(args);


var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlite(connectionString));

builder.Services.AddDatabaseDeveloperPageExceptionFilter();
    
builder.Services.AddDefaultIdentity<IdentityUser>(options =>
{
    options.SignIn.RequireConfirmedAccount = true;
    options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(3);
    options.Lockout.MaxFailedAccessAttempts = 4;
}).AddEntityFrameworkStores<ApplicationDbContext>();

builder.Services.AddControllersWithViews().AddNewtonsoftJson(options =>
{
    options.SerializerSettings.DateTimeZoneHandling = DateTimeZoneHandling.Utc;
    options.SerializerSettings.PreserveReferencesHandling = PreserveReferencesHandling.Objects;
});

builder.Services.AddAntiforgery(options => options.HeaderName = "X-XSRF-TOKEN");

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Add other services for DI

// Also use proxy for email validator client
builder.Services.AddHttpClient(nameof(EmailValidator)).ConfigurePrimaryHttpMessageHandler(HttpHandlerGenerator.WithProxy);

// Background services
builder.Services.AddSingleton<WorkRequestTracker>();
builder.Services.AddHostedService<SnapchatActionsWorker>();
builder.Services.AddHostedService<UploadCleanupService>();
builder.Services.AddHostedService<RemoteSettings>();
builder.Services.AddHostedService<ProxyScrapeService>();
builder.Services.AddHostedService<LogClearService>();
builder.Services.AddScoped<AppSettingsLoader>();
builder.Services.AddSingleton<SnapchatAccountManager>();
builder.Services.AddScoped<KeywordManager>();
builder.Services.AddScoped<TargetManager>();
builder.Services.AddScoped<PhoneNumberManager>();
builder.Services.AddScoped<EmailAddressManager>();
builder.Services.AddScoped<WorkScheduler>();
builder.Services.AddSingleton<FakePersonGenerator>();
builder.Services.AddSingleton<IProxyManager, ProxyManager>();
builder.Services.AddSingleton<SnapchatClientFactory>();
builder.Services.AddSingleton<Utilities>();
builder.Services.AddSingleton<SnapchatActionRunner>();
builder.Services.AddSingleton<UploadManager>();
builder.Services.AddSingleton<EmailManager>();
builder.Services.AddSingleton<MacroManager>();
builder.Services.AddSingleton<UserNameManager>();
builder.Services.AddSingleton<NameManager>();
builder.Services.AddSingleton<AccountTracker>();
builder.Services.AddScoped<WorkLogger>();
builder.Services.AddScoped<ModuleEnabler>();
builder.Services.AddScoped<FiveSimVerificator>();
builder.Services.AddScoped<SmsActivateActivator>();
builder.Services.AddScoped<SmsPoolActivator>();
builder.Services.AddScoped<TextVerifiedActivator>();
builder.Services.AddScoped<OutlookValidator>();
builder.Services.AddScoped<YahooValidator>();
builder.Services.AddScoped<GmailValidator>();
builder.Services.AddScoped<GmxValidator>();
builder.Services.AddScoped<YandexValidator>();
builder.Services.AddScoped<SnapWebManagerClient>();
builder.Services.AddScoped<CreateAccountTask>();
builder.Services.AddScoped<PostDirectTask>();
builder.Services.AddScoped<SubscribeTask>();
builder.Services.AddScoped<SendMentionTask>();
builder.Services.AddScoped<SendMessageTask>();
builder.Services.AddScoped<RelogAccountTask>();
builder.Services.AddScoped<TestTask>();
builder.Services.AddScoped<ReportUserRandomTask>();
builder.Services.AddScoped<ChangeUsernameTask>();
builder.Services.AddScoped<PostStoryTask>();
builder.Services.AddScoped<AddFriendTask>();
builder.Services.AddScoped<AcceptFriendTask>();
builder.Services.AddScoped<ExportFriendTask>();
builder.Services.AddScoped<ViewBusinessPublicStoryTask>();
builder.Services.AddScoped<EmailScraperTask>();
builder.Services.AddScoped<FindUsersViaSearchTask>();
builder.Services.AddScoped<PhoneScrapeTask>();
builder.Services.AddScoped<RefreshFriendTask>();
builder.Services.AddScoped<QuickAddTask>();
builder.Services.AddScoped<FriendCleanerTask>();
builder.Services.AddScoped<ViewPublicStoryTask>();
builder.Services.AddScoped<ReportUserStoryRandomTask>();
builder.Services.AddTransient<IEmailSender, EmailSender>();

// Require everyone to be authenticated
builder.Services.AddAuthorization(opt => opt.FallbackPolicy = new AuthorizationPolicyBuilder().RequireAuthenticatedUser().Build());

var app = builder.Build().MigrateDatabase<ApplicationDbContext>();

// Make sure we have some initial settings in the db, and that it happens before other stuff
await InitialSettingsSeed.CreateSettings(app.Services);

// Clean up log entries with no matching work id
await LogEntryCleaner.RemoveEntriesWithNoMatchingWorkId(app.Services);

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseMigrationsEndPoint();
    app.UseSwagger();
    app.UseSwaggerUI();
}
else
{
    app.UseForwardedHeaders(new ForwardedHeadersOptions
    {
        ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto
    });

    app.UseHttpsRedirection();
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseStaticFiles();

app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllerRoute(
    "default",
    "{controller=Home}/{action=Index}/{id?}");

app.MapRazorPages();

app.Run();