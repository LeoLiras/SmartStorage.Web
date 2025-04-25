using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;
using SmartStorage_API.Model;

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

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
#warning To protect potentially sensitive information in your connection string, you should move it out of source code. You can avoid scaffolding the connection string by using the Name= syntax to read it from configuration - see https://go.microsoft.com/fwlink/?linkid=2131148. For more guidance on storing connection strings, see https://go.microsoft.com/fwlink/?LinkId=723263.
        => optionsBuilder.UseNpgsql("Host=localhost;Port=5432;Database=smart-storage;Username=postgres;Password=root");

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Employee>(entity =>
        {
            entity.Property(e => e.Cpf).HasColumnName("cpf");
            entity.Property(e => e.DateRegister).HasColumnName("date_register");
            entity.Property(e => e.Name).HasColumnName("name");
            entity.Property(e => e.Rg).HasColumnName("rg");
        });

        modelBuilder.Entity<Enter>(entity =>
        {
            entity.ToTable("Enter");

            entity.HasIndex(e => e.ProductId, "IX_Enter_productId");

            entity.HasIndex(e => e.ShelfId, "IX_Enter_shelfId");

            entity.Property(e => e.DateEnter).HasColumnName("date_enter");
            entity.Property(e => e.IdProduct).HasColumnName("id_product");
            entity.Property(e => e.IdShelf).HasColumnName("id_shelf");
            entity.Property(e => e.Price)
                .HasPrecision(18, 2)
                .HasDefaultValueSql("10.00")
                .HasColumnName("price");
            entity.Property(e => e.ProductId).HasColumnName("productId");
            entity.Property(e => e.Qntd)
                .HasDefaultValue(0)
                .HasColumnName("qntd");
            entity.Property(e => e.ShelfId).HasColumnName("shelfId");

            entity.HasOne(d => d.Product).WithMany(p => p.Enters).HasForeignKey(d => d.ProductId);

            entity.HasOne(d => d.Shelf).WithMany(p => p.Enters).HasForeignKey(d => d.ShelfId);
        });

        modelBuilder.Entity<Product>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("Product-PK");

            entity.ToTable("Product");

            entity.HasIndex(e => e.EmployeeId, "IX_Product_employeeId");

            entity.Property(e => e.DateRegister).HasColumnName("date_register");
            entity.Property(e => e.Descricao).HasColumnName("descricao");
            entity.Property(e => e.EmployeeId).HasColumnName("employeeId");
            entity.Property(e => e.EmployeeId1)
                .HasDefaultValue(0)
                .HasColumnName("employee_id");
            entity.Property(e => e.Name).HasColumnName("name");
            entity.Property(e => e.Qntd).HasColumnName("qntd");

            entity.HasOne(d => d.Employee).WithMany(p => p.Products).HasForeignKey(d => d.EmployeeId);
        });

        modelBuilder.Entity<Sale>(entity =>
        {
            entity.ToTable("Sale");

            entity.HasIndex(e => e.EnterId, "IX_Sale_enterId");

            entity.Property(e => e.DateSale)
                .HasDefaultValueSql("'2024-07-08 00:00:00-03'::timestamp with time zone")
                .HasColumnName("date_sale");
            entity.Property(e => e.EnterId).HasColumnName("enterId");
            entity.Property(e => e.IdEnter).HasColumnName("id_enter");
            entity.Property(e => e.Qntd)
                .HasDefaultValue(0)
                .HasColumnName("qntd");

            entity.HasOne(d => d.Enter).WithMany(p => p.Sales).HasForeignKey(d => d.EnterId);
        });

        modelBuilder.Entity<Shelf>(entity =>
        {
            entity.ToTable("Shelf");

            entity.Property(e => e.DataRegister).HasColumnName("data_register");
            entity.Property(e => e.Name).HasColumnName("name");
        });

        modelBuilder.Entity<User>(entity =>
        {
            entity.ToTable("User");

            entity.Property(e => e.Address).HasColumnName("address");
            entity.Property(e => e.Cpf).HasColumnName("cpf");
            entity.Property(e => e.Email).HasColumnName("email");
            entity.Property(e => e.Name).HasColumnName("name");
            entity.Property(e => e.Password).HasColumnName("password");
            entity.Property(e => e.Phone).HasColumnName("phone");
            entity.Property(e => e.Rg).HasColumnName("rg");
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
