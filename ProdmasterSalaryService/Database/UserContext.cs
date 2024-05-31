using Microsoft.EntityFrameworkCore;
using ProdmasterSalaryService.Models.Classes;
using ProdmasterSalaryService.Models.Interfaces;

namespace ProdmasterSalaryService.Database
{
    public class UserContext : DbContext
    {
        public DbSet<User> User { get; set; }
        public DbSet<Custom> Custom { get; set; }
        public DbSet<Operation> Operations { get; set; }
        public DbSet<Shift> Shift { get; set; }
        public UserContext(DbContextOptions<UserContext> options) : base(options)
        {
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<User>()
                .HasOne(u => u.Custom)
                .WithOne(c => c.User);
            modelBuilder.Entity<Operation>()
                .HasOne(o => o.Custom)
                .WithMany(c => c.Operations)
                .HasForeignKey(o => o.Object)
                .HasPrincipalKey(c => c.DisanId);
            modelBuilder.Entity<Shift>()
                .HasOne(s => s.Custom)
                .WithMany(c => c.Shifts)
                .HasForeignKey(s => s.Object)
                .HasPrincipalKey(c => c.DisanId);
        }
        public override Task<int> SaveChangesAsync(bool acceptAllChangesOnSuccess,
            CancellationToken cancellationToken = new CancellationToken())
        {
            FillUpdatedDate();
            return base.SaveChangesAsync(acceptAllChangesOnSuccess, cancellationToken);
        }
        public void FillUpdatedDate()
        {
            var newEntities = ChangeTracker.Entries()
                .Where(
                    x => x.State == EntityState.Added &&
                         x.Entity != null &&
                         x.Entity is IDisanModel
                )
                .Select(x => x.Entity as IDisanModel);

            var modifiedEntities = ChangeTracker.Entries()
                .Where(
                    x => x.State == EntityState.Modified &&
                         x.Entity != null &&
                         x.Entity is IDisanModel
                )
                .Select(x => x.Entity as IDisanModel);

            foreach (var newEntity in newEntities)
            {
                if (newEntity != null)
                {
                    if (newEntity.Created == default)
                        newEntity.Created = DateTime.UtcNow;
                    if (newEntity.Modified == default)
                        newEntity.Modified = DateTime.UtcNow;
                }
            }

            foreach (var modifiedEntity in modifiedEntities)
            {
                if (modifiedEntity != null) modifiedEntity.Modified = DateTime.UtcNow;
            }
        }
    }
}
