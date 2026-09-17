using Microsoft.EntityFrameworkCore;
using SmartStorage_API.Model.Context.Seed;
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

    public virtual DbSet<Invoice> Invoices { get; set; }

    public virtual DbSet<InvoiceItem> InvoiceItems { get; set; }

    public virtual DbSet<Product> Products { get; set; }

    public virtual DbSet<ProductStockMovement> ProductStockMovements { get; set; }

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

            entity.Property(p => p.ProVolume).HasPrecision(18, 3);

            entity.Property(p => p.ProPrecoInicial).HasPrecision(18, 2);

            entity.Property(p => p.ProCusto).HasPrecision(18, 4);

            entity.HasIndex(e => e.ProCodigo, "UQ_Product_codigo").IsUnique().HasFilter("[ProCodigo] IS NOT NULL");

            entity.HasOne(d => d.Employee).WithMany(p => p.Products).HasForeignKey(d => d.ProEmpId);
        });

        modelBuilder.Entity<Invoice>(entity =>
        {
            entity.HasIndex(e => e.InvChave, "UQ_Invoice_chave").IsUnique();

            entity.HasIndex(e => e.InvUseId, "IX_Invoice_userId");

            entity.HasOne(d => d.User).WithMany().HasForeignKey(d => d.InvUseId).OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<InvoiceItem>(entity =>
        {
            entity.HasIndex(e => e.IniInvId, "IX_InvoiceItem_invoiceId");

            entity.HasIndex(e => e.IniProId, "IX_InvoiceItem_productId");

            entity.Property(p => p.IniQntdNota).HasPrecision(18, 4);

            entity.Property(p => p.IniValorTotal).HasPrecision(18, 2);

            entity.Property(p => p.IniCustoUnitario).HasPrecision(18, 4);

            entity.HasOne(d => d.Invoice).WithMany(p => p.Items).HasForeignKey(d => d.IniInvId).OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(d => d.Product).WithMany().HasForeignKey(d => d.IniProId).OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<Shelf>(entity =>
        {
            entity.Property(p => p.SheVolume).HasPrecision(18, 3);
        });

        modelBuilder.Entity<Sale>(entity =>
        {
            entity.HasIndex(e => e.SalEntId, "IX_Sale_enterId");

            entity.Property(p => p.SalPrice).HasPrecision(18, 2);

            entity.HasOne(d => d.Enter).WithMany(p => p.Sales).HasForeignKey(d => d.SalEntId);
        });

        modelBuilder.Entity<ProductStockMovement>(entity =>
        {
            entity.HasIndex(e => e.PsmProId, "IX_ProductStockMovement_productId");

            entity.HasIndex(e => e.PsmSheId, "IX_ProductStockMovement_shelfId");

            entity.HasIndex(e => e.PsmUseId, "IX_ProductStockMovement_userId");

            entity.Property(x => x.PsmType).HasConversion<byte>().IsRequired();

            entity.ToTable(t => t.HasCheckConstraint("CK_ProductStockMovement_Tipo", "[PsmType] IN (0, 1, 2, 3, 4, 5, 6)"));

            entity.HasOne(d => d.Product).WithMany().HasForeignKey(d => d.PsmProId).OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(d => d.Shelf).WithMany().HasForeignKey(d => d.PsmSheId).OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(d => d.User).WithMany().HasForeignKey(d => d.PsmUseId).OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<User>(entity =>
        {
            entity.HasIndex(e => e.Username).IsUnique().HasDatabaseName("UQ_users_user_name");

            entity.Property(e => e.RefreshTokenExpiryTime).HasColumnName("UseRefreshTokenExpiryTime").HasColumnType("datetime2(6)").IsRequired(false);

            entity.Property(x => x.UseType).HasConversion<byte>().IsRequired();
            entity.ToTable(t => t.HasCheckConstraint("CK_User_Tipo", "[UseType] IN (0, 1)"));
        });

        modelBuilder.Seed();

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
