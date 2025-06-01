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
        public DbSet<License> Licenses { get; set; }
        public DbSet<TaxMode> TaxesModes { get; set; }
        public DbSet<User> Users { get; set; }
        public DbSet<Role> Roles { get; set; }
        public DbSet<UserRole> UserRoles { get; set; }
        public DbSet<RefreshToken> RefreshTokens { get; set; }
        public DbSet<OrganizationEconomicActivity> OrganizationsEconomicActivities { get; set; }
        public DbSet<UserFavoriteOrganization> UsersFavoritesOrganizations { get; set; }
        public DbSet<UserFavoriteCertificate> UsersFavoritesCertificates { get; set; }
        public DbSet<AuditLog> AuditLog { get; set; }
        public DbSet<Stamp> Stamps { get; set; }
        public DbSet<Tender> Tenders { get; set; }
        public DbSet<ArbitrationCase> ArbitrationsCases { get; set; }
        public DbSet<Notification> Notifications { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.UseNpgsql("Host=79.174.83.231;Port=5432;Database=stk3;Username=postgres;Password=secret123");
        }
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<OrganizationEconomicActivity>()
                .HasKey(oe => new { oe.OrganizationId, oe.EconomicActivityId });

            modelBuilder.Entity<OrganizationEconomicActivity>()
                .HasOne(oe => oe.Organizations)
                .WithMany(o => o.OrganizationsEconomicActivities)
                .HasForeignKey(oe => oe.OrganizationId);

            modelBuilder.Entity<OrganizationEconomicActivity>()
                .HasOne(oe => oe.EconomicActivities)
                .WithMany(ea => ea.OrganizationsEconomicActivities)
                .HasForeignKey(oe => oe.EconomicActivityId);

            modelBuilder.Entity<Certificate>()
                .HasOne(c => c.Organization)
                .WithMany(o => o.Certificates)
                .HasForeignKey(c => c.OrganizationId);

            modelBuilder.Entity<Stamp>()
                .HasOne(s => s.Organization)
                .WithMany(o => o.Stamps)
                .HasForeignKey(s => s.OrganizationId);

            modelBuilder.Entity<ArbitrationCase>()
                .HasOne(ac => ac.Organization)
                .WithMany(o => o.ArbitrationsCases)
                .HasForeignKey(ac => ac.OrganizationId);

            modelBuilder.Entity<TaxMode>()
                .HasMany(tm => tm.Organization)
                .WithMany(o => o.TaxesModes)
                .UsingEntity(j => j.ToTable("OrganizationTaxMode"));

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

            modelBuilder.Entity<License>()
                .HasOne (l => l.Organization)
                .WithMany(o => o.Licenses)
                .HasForeignKey(l => l.OrganizationId);

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

            modelBuilder.Entity<UserFavoriteOrganization>()
                .HasKey(ufo => new { ufo.UserId, ufo.OrganizationId });

            modelBuilder.Entity<UserFavoriteOrganization>()
                .HasOne(ufo => ufo.User)
                .WithMany(u => u.FavoritesOrganizations)
                .HasForeignKey(ufo => ufo.UserId);

            modelBuilder.Entity<UserFavoriteOrganization>()
                .HasOne(ufo => ufo.Organization)
                .WithMany(o => o.FavoritedByUsers)
                .HasForeignKey(ufo => ufo.OrganizationId);

            modelBuilder.Entity<UserFavoriteCertificate>()
                .HasKey(ufo => new { ufo.UserId, ufo.CertificateId });

            modelBuilder.Entity<UserFavoriteCertificate>()
                .HasOne(ufo => ufo.User)
                .WithMany(u => u.FavoritesCertificates)
                .HasForeignKey(ufo => ufo.UserId);

            modelBuilder.Entity<UserFavoriteCertificate>()
                .HasOne(ufo => ufo.Certificate)
                .WithMany(o => o.FavoritedByUsers)
                .HasForeignKey(ufo => ufo.CertificateId);

            modelBuilder.Entity<AuditLog>()
                .ToTable("AuditLog")
                .HasKey(x => x.Id);

            modelBuilder.Entity<AuditLog>()
                .Property(x => x.OldData)
                .HasColumnType("jsonb");

            modelBuilder.Entity<AuditLog>()
                .Property(x => x.NewData)
                .HasColumnType("jsonb");

            modelBuilder.Entity<Tender>()
                .HasKey(x => x.Id);

            modelBuilder.Entity<Notification>()
                .HasKey(x => x.Id);
        }
    }
}
