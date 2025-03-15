using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using TaskBoard.Models;

namespace TaskBoard
{
    public partial class ApplicationDbContext : IdentityDbContext
    {
        public ApplicationDbContext()
        {
        }

        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        public override int SaveChanges()
        {
            try
            {
                this.BulkSaveChanges();
                
                return 1;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"{ex.Message}{ex.StackTrace}");
            }

            return 0;
        }

        public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
        {
            try
            {
                await this.BulkSaveChangesAsync(cancellationToken);

                return 1;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"{ex.Message}{ex.StackTrace}");
            }
            
            return 0;
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            
            OnModelCreatingPartial(modelBuilder);
        }

        partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
        public DbSet<AppSettings>? AppSettings { get; set; }
        public DbSet<SnapchatAccountModel>? Accounts { get; set; }
        public DbSet<NameModel>? Names { get; set; }
        public DbSet<UserNameModel>? UserNames { get; set; }
        public DbSet<MacroModel>? Macros { get; set; }
        public DbSet<BannedAccountDeletionLog>? BannedAccountDeletionLog { get; set; }
        public DbSet<WorkRequest>? WorkRequests { get; set; }
        public DbSet<LogEntry>? LogEntries { get; set; }
        public DbSet<Proxy>? Proxies { get; set; }
        public DbSet<ChosenAccount>? ChosenAccounts { get; set; }
        public DbSet<EnabledModule>? EnabledModules { get; set; }
        public DbSet<EmailModel>? Emails { get; set; }
        public virtual DbSet<TargetUser>? TargetUsers { get; set; }
        public DbSet<Keyword>? Keywords { get; set; }
        public DbSet<PhoneListModel>? PhoneList { get; set; }
        public DbSet<EmailListModel>? EmailList { get; set; }
        public DbSet<MediaFile>? MediaFiles { get; set; }
        public DbSet<ChosenTarget>? ChosenTargets { get; set; }
        public DbSet<AccountGroup>? AccountGroups { get; set; }
        public DbSet<ProxyGroup>? ProxyGroups { get; set; }
        public DbSet<BitmojiModel>? Bitmojis { get; set; }
    }
}