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
        public DbSet<BalanceSheet> BalanceSheets { get; set; }
        public DbSet<FinancialResult> FinancialResults { get; set; }
        public DbSet<User> Users { get; set; }
        public DbSet<Role> Roles { get; set; }
        public DbSet<UserRole> UserRoles { get; set; }
        public DbSet<RefreshToken> RefreshTokens { get; set; }

        //protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        //{
        //    optionsBuilder.UseNpgsql("Host=79.174.83.231;Port=5432;Database=stk;Username=postgres;Password=secret123");
        //}
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {

            modelBuilder.Entity<Certificate>()
                .HasOne(c => c.Organization)
                .WithMany(o => o.Certificates)
                .HasForeignKey(c => c.OrganizationId);

            modelBuilder.Entity<EconomicActivity>()
                .HasMany(ea => ea.Organization)
                .WithMany(o => o.EconomicActivities)
                .UsingEntity(j => j.ToTable("OrganizationEconomicActivity"));

            modelBuilder.Entity<Management>()
                .HasOne(m => m.Organization)
                .WithMany(o => o.Managements)
                .HasForeignKey(m => m.OrganizationId);

            modelBuilder.Entity<Requisite>()
                .HasOne(r => r.Organization)
                .WithOne(o => o.Requisites)
                .HasForeignKey<Requisite>(r => r.OrganizationId);

            modelBuilder.Entity<BalanceSheet>()
                .HasOne(bs => bs.Organization)
                .WithMany(o => o.BalanceSheets)
                .HasForeignKey(bs => bs.OrganizationId);

            modelBuilder.Entity<FinancialResult>()
                .HasOne(fr => fr.Organization)
                .WithMany(o => o.FinancialResults)
                .HasForeignKey(fr => fr.OrganizationId);

            modelBuilder.Entity<UserRole>()
                .HasKey(ur => new { ur.UserId, ur.RoleId });

            modelBuilder.Entity<UserRole>()
                .HasOne(ur => ur.User)
                .WithMany(u => u.UserRoles)
                .HasForeignKey(ur => ur.UserId);

            modelBuilder.Entity<UserRole>()
                .HasOne(ur => ur.Role)
                .WithMany(r => r.UserRoles)
                .HasForeignKey(ur => ur.RoleId);

            modelBuilder.Entity<User>()
                .HasMany(u => u.RefreshTokens)
                .WithOne(rt => rt.User)
                .HasForeignKey(rt => rt.UserId);
        }
    }
}
