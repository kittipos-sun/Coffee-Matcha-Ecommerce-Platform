using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;
using Pomelo.EntityFrameworkCore.MySql.Scaffolding.Internal;

namespace CSI402_Final.Models.DB;

public partial class Csi402FinalContext : DbContext
{
    public Csi402FinalContext()
    {
    }

    public Csi402FinalContext(DbContextOptions<Csi402FinalContext> options)
        : base(options)
    {
    }

    public virtual DbSet<Address> Addresses { get; set; }

    public virtual DbSet<Cart> Carts { get; set; }

    public virtual DbSet<CartItem> CartItems { get; set; }

    public virtual DbSet<DiscountCoupon> DiscountCoupons { get; set; }

    public virtual DbSet<Order> Orders { get; set; }

    public virtual DbSet<OrdersItem> OrdersItems { get; set; }

    public virtual DbSet<Product> Products { get; set; }

    public virtual DbSet<ProductCategory> ProductCategories { get; set; }

    public virtual DbSet<ProductCoffeeDetail> ProductCoffeeDetails { get; set; }

    public virtual DbSet<ProductEquipmentDetail> ProductEquipmentDetails { get; set; }

    public virtual DbSet<ProductMatchaDetail> ProductMatchaDetails { get; set; }

    public virtual DbSet<ProductOption> ProductOptions { get; set; }

    public virtual DbSet<User> Users { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        if (!optionsBuilder.IsConfigured)
        {
            var connectionString = Environment.GetEnvironmentVariable("CSI402_CONNECTION_STRING")
                ?? "server=localhost;port=3306;database=csi402_final;user=root;password=change_me";
            optionsBuilder.UseMySql(connectionString, ServerVersion.AutoDetect(connectionString));
        }
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder
            // utf8mb4_0900_ai_ci is MySQL-8-only and prevents the local
            // MariaDB/XAMPP instance from creating the schema. This collation
            // is supported by both MySQL and MariaDB.
            .UseCollation("utf8mb4_unicode_ci")
            .HasCharSet("utf8mb4");

        modelBuilder.Entity<Address>(entity =>
        {
            entity.HasKey(e => e.AddressId).HasName("PRIMARY");

            entity.ToTable("address");

            entity.HasIndex(e => e.UserId, "userID");

            entity.Property(e => e.AddressId)
                .HasMaxLength(36)
                .HasColumnName("addressID");
            entity.Property(e => e.AddressLine)
                .HasColumnType("text")
                .HasColumnName("addressLine");
            entity.Property(e => e.District)
                .HasMaxLength(100)
                .HasColumnName("district");
            entity.Property(e => e.FullName)
                .HasMaxLength(100)
                .HasColumnName("fullName");
            entity.Property(e => e.Phone)
                .HasMaxLength(20)
                .HasColumnName("phone");
            entity.Property(e => e.PostalCode)
                .HasMaxLength(10)
                .HasColumnName("postalCode");
            entity.Property(e => e.Province)
                .HasMaxLength(100)
                .HasColumnName("province");
            entity.Property(e => e.UserId)
                .HasMaxLength(36)
                .HasColumnName("userID");

            entity.HasOne(d => d.User).WithMany(p => p.Addresses)
                .HasForeignKey(d => d.UserId)
                .HasConstraintName("address_ibfk_1");
        });

        modelBuilder.Entity<Cart>(entity =>
        {
            entity.HasKey(e => e.CartId).HasName("PRIMARY");

            entity.ToTable("cart");

            entity.HasIndex(e => e.UserId, "userId");

            entity.Property(e => e.CartId)
                .HasMaxLength(36)
                .HasColumnName("cartID");
            entity.Property(e => e.CartStatus)
                .HasMaxLength(20)
                .HasColumnName("cartStatus");
            entity.Property(e => e.CreatedAt)
                .HasColumnType("datetime")
                .HasColumnName("createdAt");
            entity.Property(e => e.UserId)
                .HasMaxLength(36)
                .HasColumnName("userId");

            entity.HasOne(d => d.User).WithMany(p => p.Carts)
                .HasForeignKey(d => d.UserId)
                .HasConstraintName("cart_ibfk_1");
        });

        modelBuilder.Entity<CartItem>(entity =>
        {
            entity.HasKey(e => e.CartItemId).HasName("PRIMARY");

            entity.ToTable("cart_item");

            entity.HasIndex(e => e.CartId, "cartID");

            entity.HasIndex(e => e.ProductOptionId, "productOptionID");

            entity.Property(e => e.CartItemId)
                .HasMaxLength(36)
                .HasColumnName("cartItemID");
            entity.Property(e => e.CartId)
                .HasMaxLength(36)
                .HasColumnName("cartID");
            entity.Property(e => e.CartItemAmount).HasColumnName("cartItemAmount");
            entity.Property(e => e.ProductOptionId)
                .HasMaxLength(36)
                .HasColumnName("productOptionID");

            entity.HasOne(d => d.Cart).WithMany(p => p.CartItems)
                .HasForeignKey(d => d.CartId)
                .HasConstraintName("cart_item_ibfk_1");

            entity.HasOne(d => d.ProductOption).WithMany(p => p.CartItems)
                .HasForeignKey(d => d.ProductOptionId)
                .HasConstraintName("cart_item_ibfk_2");
        });

        modelBuilder.Entity<DiscountCoupon>(entity =>
        {
            entity.HasKey(e => e.DiscountId).HasName("PRIMARY");

            entity.ToTable("discount_coupon");

            entity.HasIndex(e => e.DiscountCode, "discountCode").IsUnique();

            entity.Property(e => e.DiscountId)
                .HasMaxLength(36)
                .HasColumnName("discountID");
            entity.Property(e => e.DiscountAmount)
                .HasPrecision(10, 2)
                .HasColumnName("discountAmount");
            entity.Property(e => e.DiscountCode)
                .HasMaxLength(36)
                .HasColumnName("discountCode");
            entity.Property(e => e.DiscountLeft).HasColumnName("discountLeft");
            entity.Property(e => e.DiscountName)
                .HasMaxLength(100)
                .HasColumnName("discountName");
            entity.Property(e => e.DiscountPrice)
                .HasPrecision(10, 2)
                .HasColumnName("discountPrice");
            entity.Property(e => e.DiscountType)
                .HasMaxLength(10)
                .HasColumnName("discountType");
            entity.Property(e => e.IsActive)
                .HasDefaultValueSql("'1'")
                .HasColumnName("isActive");

            entity.HasMany(d => d.Users).WithMany(p => p.Discounts)
                .UsingEntity<Dictionary<string, object>>(
                    "DiscountUsed",
                    r => r.HasOne<User>().WithMany()
                        .HasForeignKey("UserId")
                        .OnDelete(DeleteBehavior.ClientSetNull)
                        .HasConstraintName("discount_used_ibfk_2"),
                    l => l.HasOne<DiscountCoupon>().WithMany()
                        .HasForeignKey("DiscountId")
                        .OnDelete(DeleteBehavior.ClientSetNull)
                        .HasConstraintName("discount_used_ibfk_1"),
                    j =>
                    {
                        j.HasKey("DiscountId", "UserId")
                            .HasName("PRIMARY")
                            .HasAnnotation("MySql:IndexPrefixLength", new[] { 0, 0 });
                        j.ToTable("discount_used");
                        j.HasIndex(new[] { "UserId" }, "userID");
                        j.IndexerProperty<string>("DiscountId")
                            .HasMaxLength(36)
                            .HasColumnName("discountID");
                        j.IndexerProperty<string>("UserId")
                            .HasMaxLength(36)
                            .HasColumnName("userID");
                    });
        });

        modelBuilder.Entity<Order>(entity =>
        {
            entity.HasKey(e => e.OrderId).HasName("PRIMARY");

            entity.ToTable("orders");

            entity.HasIndex(e => e.AddressId, "addressID");

            entity.HasIndex(e => e.UserId, "userID");

            entity.Property(e => e.OrderId)
                .HasMaxLength(36)
                .HasColumnName("orderID");
            entity.Property(e => e.AddressId)
                .HasMaxLength(36)
                .HasColumnName("addressID");
            entity.Property(e => e.CreatedAt)
                .HasColumnType("datetime")
                .HasColumnName("createdAt");
            entity.Property(e => e.ShippingAddress)
                .HasColumnType("text")
                .HasColumnName("shippingAddress");
            entity.Property(e => e.ShippingName)
                .HasMaxLength(100)
                .HasColumnName("shippingName");
            entity.Property(e => e.ShippingPhone)
                .HasMaxLength(20)
                .HasColumnName("shippingPhone");
            entity.Property(e => e.Status)
                .HasMaxLength(20)
                .HasColumnName("status");
            entity.Property(e => e.TotalPrice)
                .HasPrecision(10, 2)
                .HasColumnName("totalPrice");
            entity.Property(e => e.UserId)
                .HasMaxLength(36)
                .HasColumnName("userID");

            entity.HasOne(d => d.Address).WithMany(p => p.Orders)
                .HasForeignKey(d => d.AddressId)
                .HasConstraintName("orders_ibfk_2");

            entity.HasOne(d => d.User).WithMany(p => p.Orders)
                .HasForeignKey(d => d.UserId)
                .HasConstraintName("orders_ibfk_1");
        });

        modelBuilder.Entity<OrdersItem>(entity =>
        {
            entity.HasKey(e => e.OrderItemId).HasName("PRIMARY");

            entity.ToTable("orders_item");

            entity.HasIndex(e => e.OrderId, "orderID");

            entity.HasIndex(e => e.ProductOptionId, "productOptionID");

            entity.Property(e => e.OrderItemId)
                .HasMaxLength(36)
                .HasColumnName("orderItemID");
            entity.Property(e => e.Amount).HasColumnName("amount");
            entity.Property(e => e.OrderId)
                .HasMaxLength(36)
                .HasColumnName("orderID");
            entity.Property(e => e.Price)
                .HasPrecision(10, 2)
                .HasColumnName("price");
            entity.Property(e => e.ProductOptionId)
                .HasMaxLength(36)
                .HasColumnName("productOptionID");

            entity.HasOne(d => d.Order).WithMany(p => p.OrdersItems)
                .HasForeignKey(d => d.OrderId)
                .HasConstraintName("orders_item_ibfk_1");

            entity.HasOne(d => d.ProductOption).WithMany(p => p.OrdersItems)
                .HasForeignKey(d => d.ProductOptionId)
                .HasConstraintName("orders_item_ibfk_2");
        });

        modelBuilder.Entity<Product>(entity =>
        {
            entity.HasKey(e => e.ProductId).HasName("PRIMARY");

            entity.ToTable("product");

            entity.HasIndex(e => e.CategoryId, "categoryID");

            entity.Property(e => e.ProductId)
                .HasMaxLength(36)
                .HasColumnName("productID");
            entity.Property(e => e.CategoryId)
                .HasMaxLength(36)
                .HasColumnName("categoryID");
            entity.Property(e => e.ProductDescription)
                .HasColumnType("text")
                .HasColumnName("productDescription");
            entity.Property(e => e.ProductImage)
                .HasMaxLength(500)
                .HasColumnName("productImage");
            entity.Property(e => e.ProductName)
                .HasMaxLength(150)
                .HasColumnName("productName");
            entity.Property(e => e.ProductType)
                .HasMaxLength(20)
                .HasColumnName("productType");

            entity.HasOne(d => d.Category).WithMany(p => p.Products)
                .HasForeignKey(d => d.CategoryId)
                .HasConstraintName("product_ibfk_1");
        });

        modelBuilder.Entity<ProductCategory>(entity =>
        {
            entity.HasKey(e => e.CategoryId).HasName("PRIMARY");

            entity.ToTable("product_category");

            entity.HasIndex(e => e.CategoryName, "categoryName").IsUnique();

            entity.Property(e => e.CategoryId)
                .HasMaxLength(36)
                .HasColumnName("categoryID");
            entity.Property(e => e.CategoryName)
                .HasMaxLength(50)
                .HasColumnName("categoryName");
        });

        modelBuilder.Entity<ProductCoffeeDetail>(entity =>
        {
            entity.HasKey(e => e.ProductId).HasName("PRIMARY");

            entity.ToTable("product_coffee_detail");

            entity.Property(e => e.ProductId)
                .HasMaxLength(36)
                .HasColumnName("productID");
            entity.Property(e => e.FlavorNote)
                .HasMaxLength(100)
                .HasColumnName("flavorNote");
            entity.Property(e => e.Origin)
                .HasMaxLength(100)
                .HasColumnName("origin");
            entity.Property(e => e.Process)
                .HasMaxLength(50)
                .HasColumnName("process");
            entity.Property(e => e.RoastLevel)
                .HasMaxLength(50)
                .HasColumnName("roastLevel");

            entity.HasOne(d => d.Product).WithOne(p => p.ProductCoffeeDetail)
                .HasForeignKey<ProductCoffeeDetail>(d => d.ProductId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("product_coffee_detail_ibfk_1");
        });

        modelBuilder.Entity<ProductEquipmentDetail>(entity =>
        {
            entity.HasKey(e => e.ProductId).HasName("PRIMARY");

            entity.ToTable("product_equipment_detail");

            entity.Property(e => e.ProductId)
                .HasMaxLength(36)
                .HasColumnName("productID");
            entity.Property(e => e.Brand)
                .HasMaxLength(100)
                .HasColumnName("brand");
            entity.Property(e => e.EquipmentType)
                .HasMaxLength(100)
                .HasColumnName("equipmentType");
            entity.Property(e => e.Material)
                .HasMaxLength(100)
                .HasColumnName("material");

            entity.HasOne(d => d.Product).WithOne(p => p.ProductEquipmentDetail)
                .HasForeignKey<ProductEquipmentDetail>(d => d.ProductId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("product_equipment_detail_ibfk_1");
        });

        modelBuilder.Entity<ProductMatchaDetail>(entity =>
        {
            entity.HasKey(e => e.ProductId).HasName("PRIMARY");

            entity.ToTable("product_matcha_detail");

            entity.Property(e => e.ProductId)
                .HasMaxLength(36)
                .HasColumnName("productID");
            entity.Property(e => e.ColorLevel)
                .HasMaxLength(50)
                .HasColumnName("colorLevel");
            entity.Property(e => e.Grade)
                .HasMaxLength(50)
                .HasColumnName("grade");
            entity.Property(e => e.Origin)
                .HasMaxLength(100)
                .HasColumnName("origin");

            entity.HasOne(d => d.Product).WithOne(p => p.ProductMatchaDetail)
                .HasForeignKey<ProductMatchaDetail>(d => d.ProductId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("product_matcha_detail_ibfk_1");
        });

        modelBuilder.Entity<ProductOption>(entity =>
        {
            entity.HasKey(e => e.ProductOptionId).HasName("PRIMARY");

            entity.ToTable("product_option");

            entity.HasIndex(e => e.ProductId, "productID");

            entity.Property(e => e.ProductOptionId)
                .HasMaxLength(36)
                .HasColumnName("productOptionID");
            entity.Property(e => e.ProductAmount).HasColumnName("productAmount");
            entity.Property(e => e.ProductId)
                .HasMaxLength(36)
                .HasColumnName("productID");
            entity.Property(e => e.ProductPrice)
                .HasPrecision(10, 2)
                .HasColumnName("productPrice");
            entity.Property(e => e.ProductStock).HasColumnName("productStock");
            entity.Property(e => e.ProductUnit)
                .HasMaxLength(50)
                .HasColumnName("productUnit");

            entity.HasOne(d => d.Product).WithMany(p => p.ProductOptions)
                .HasForeignKey(d => d.ProductId)
                .HasConstraintName("product_option_ibfk_1");
        });

        modelBuilder.Entity<User>(entity =>
        {
            entity.HasKey(e => e.UserId).HasName("PRIMARY");

            entity.ToTable("user");

            entity.HasIndex(e => e.Email, "email").IsUnique();

            entity.Property(e => e.UserId)
                .HasMaxLength(36)
                .HasColumnName("userID");
            entity.Property(e => e.CreatedAt)
                .HasColumnType("datetime")
                .HasColumnName("createdAt");
            entity.Property(e => e.Email)
                .HasMaxLength(254)
                .HasColumnName("email");
            entity.Property(e => e.Password)
                .HasMaxLength(255)
                .HasColumnName("password");
            entity.Property(e => e.Phone)
                .HasMaxLength(20)
                .HasColumnName("phone");
            entity.Property(e => e.Role)
                .HasMaxLength(20)
                .HasColumnName("role");
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
