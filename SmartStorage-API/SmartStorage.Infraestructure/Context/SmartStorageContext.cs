using Microsoft.EntityFrameworkCore;
using SmartStorage_Shared.Model;

namespace SmartStorage_API.Model.Context;

public partial class SmartStorageContext : DbContext
{
    public SmartStorageContext()
    {
    }

    public SmartStorageContext(DbContextOptions<SmartStorageContext> options)
        : base(options)
    {
    }

    public virtual DbSet<Employee> Employees { get; set; }

    public virtual DbSet<Enter> Enters { get; set; }

    public virtual DbSet<Product> Products { get; set; }

    public virtual DbSet<Sale> Sales { get; set; }

    public virtual DbSet<Shelf> Shelves { get; set; }

    public virtual DbSet<User> Users { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Enter>(entity =>
        {
            entity.HasIndex(e => e.EntProId, "IX_Enter_productId");

            entity.HasIndex(e => e.EntSheId, "IX_Enter_shelfId");

            entity.Property(p => p.EntPrice).HasPrecision(18, 2);

            entity.HasOne(d => d.Product).WithMany(p => p.Enters).HasForeignKey(d => d.EntProId);

            entity.HasOne(d => d.Shelf).WithMany(p => p.Enters).HasForeignKey(d => d.EntSheId);
        });

        modelBuilder.Entity<Product>(entity =>
        {
            entity.HasIndex(e => e.ProEmpId, "IX_Product_employeeId");

            entity.HasOne(d => d.Employee).WithMany(p => p.Products).HasForeignKey(d => d.ProEmpId);
        });

        modelBuilder.Entity<Sale>(entity =>
        {
            entity.HasIndex(e => e.SalEntId, "IX_Sale_enterId");

            entity.HasOne(d => d.Enter).WithMany(p => p.Sales).HasForeignKey(d => d.SalEntId);
        });

        modelBuilder.Entity<User>(entity =>
        {
            entity.HasIndex(e => e.Username).IsUnique().HasDatabaseName("UQ_users_user_name");

            entity.Property(e => e.RefreshTokenExpiryTime).HasColumnName("refresh_token_expiry_time").HasColumnType("datetime2(6)").IsRequired(false);

            entity.Property(x => x.UseType).HasConversion<byte>().IsRequired();
            entity.ToTable(t => t.HasCheckConstraint("CK_User_Tipo", "[UseType] IN (0, 1)"));
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
