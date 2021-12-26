using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;

#nullable disable

namespace Ecommerse_Api.Models
{
    public partial class EcomContext : DbContext
    {
        public EcomContext()
        {
        }

        public EcomContext(DbContextOptions<EcomContext> options)
            : base(options)
        {
        }

        public virtual DbSet<Product> Products { get; set; }
        public virtual DbSet<ProductImage> ProductImages { get; set; }
        public virtual DbSet<User> Users { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            if (!optionsBuilder.IsConfigured)
            {
                optionsBuilder.UseSqlServer("Data Source=(localdb)\\MSSQLLocalDB;Initial Catalog=Ecom;Integrated Security=True");
            }
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.HasAnnotation("Relational:Collation", "SQL_Latin1_General_CP1_CI_AS");


            modelBuilder.Entity<Product>(entity =>
            {
                entity.Property(e => e.Createdby)
                    .HasMaxLength(100)
                    .IsUnicode(false)
                    .HasColumnName("createdby");

                entity.Property(e => e.Productcategory)
                    .HasMaxLength(100)
                    .IsUnicode(false)
                    .HasColumnName("productcategory");

                entity.Property(e => e.Productdiscription)
                    .HasMaxLength(200)
                    .IsUnicode(false)
                    .HasColumnName("productdiscription");

                entity.Property(e => e.Productimage)
                    .HasMaxLength(100)
                    .IsUnicode(false)
                    .HasColumnName("productimage");

                entity.Property(e => e.Productname)
                    .HasMaxLength(100)
                    .IsUnicode(false)
                    .HasColumnName("productname");

                entity.Property(e => e.Productprice).HasColumnName("productprice");

                entity.Property(e => e.Productrating).HasColumnName("productrating");

                entity.Property(e => e.Productsize)
                    .HasMaxLength(50)
                    .IsUnicode(false)
                    .HasColumnName("productsize");
            });

            modelBuilder.Entity<ProductImage>(entity =>
            {
                entity.Property(e => e.Id).HasColumnName("id");

                entity.Property(e => e.Productid).HasColumnName("productid");

                entity.Property(e => e.Url)
                    .HasMaxLength(100)
                    .IsUnicode(false)
                    .HasColumnName("url");

                entity.HasOne(d => d.Product)
                    .WithMany(p => p.ProductImages)
                    .HasForeignKey(d => d.Productid)
                    .HasConstraintName("FK_ProductImages_ToTable");
            });

            modelBuilder.Entity<User>(entity =>
            {
                entity.Property(e => e.Email)
                    .HasMaxLength(100)
                    .IsUnicode(false);

                entity.Property(e => e.Name)
                    .HasMaxLength(100)
                    .IsUnicode(false);

                entity.Property(e => e.Password)
                    .HasMaxLength(100)
                    .IsUnicode(false);

                entity.Property(e => e.Roles)
                    .HasMaxLength(20)
                    .IsUnicode(false)
                    .HasDefaultValueSql("((1))");
            });

            OnModelCreatingPartial(modelBuilder);
        }

        partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
    }
}
