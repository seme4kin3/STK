using Microsoft.EntityFrameworkCore;
using STK.Domain.Entities;


namespace STK.Persistance
{
    public class DataContext: DbContext 
    {
        public DataContext() { }
        public DataContext(DbContextOptions<DataContext> options) : base(options) 
        {
            
        }

        public DbSet<Certificate> Certificates { get; set; }
        public DbSet<EconomicActivity> EconomicActivities { get; set; }
        public DbSet<Management> Managements { get; set; }
        public DbSet<Organization> Organizations { get; set; }
        public DbSet<Requisite> Requisites { get; set; }

        //protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        //{
        //    optionsBuilder.UseNpgsql("Host=localhost;Port=5430;Database=stk;Username=stk;Password=stk");
        //}
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            
            modelBuilder.Entity<Certificate>()
                .HasOne(c => c.Organization)
                .WithMany(o => o.Certificates)
                .HasForeignKey(c => c.OrganizationId);

            modelBuilder.Entity<EconomicActivity>()
                .HasOne(e => e.Organization)
                .WithMany(o => o.EconomicActivities)
                .HasForeignKey(e => e.OrganizationId);

            modelBuilder.Entity<Management>()
                .HasOne(m => m.Organization)
                .WithMany(o => o.Managements)
                .HasForeignKey(m => m.OrganizationId);

            modelBuilder.Entity<Requisite>()
                .HasOne(r => r.Organization)
                .WithOne(o => o.Requisites)
                .HasForeignKey<Requisite>(r => r.OrganizationId);

        }
    }
}
