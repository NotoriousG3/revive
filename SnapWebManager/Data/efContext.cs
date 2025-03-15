using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using SnapWebModels;

namespace SnapWebManager.Data
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

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            
            OnModelCreatingPartial(modelBuilder);
        }

        partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
        public virtual DbSet<SnapWebClientModel>? Clients { get; set; }
        public virtual DbSet<SnapWebModule>? Modules { get; set; }
        public virtual DbSet<AllowedModules>? AllowedModules { get; set; }
        public virtual DbSet<InvoiceModel>? Invoices { get; set; }
    }
}
