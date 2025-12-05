using Microsoft.EntityFrameworkCore;
using SistemaInventario.Domain.Entities;
using SistemaInventario.Domain.ValueObjects;

namespace SistemaInventario.Infrastructure.Data
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options) { }

        // DbSets para nuestras entidades
        public DbSet<Product> Products { get; set; }
        public DbSet<Category> Categories { get; set; }
        public DbSet<User> Users { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Configuración de Product
            modelBuilder.Entity<Product>(entity =>
            {
                entity.HasKey(e => e.Id);

                entity.Property(e => e.Name)
                    .IsRequired()
                    .HasMaxLength(200);

                // Configurar Value Object Money como Owned Type
                entity.OwnsOne(e => e.Price, price =>
                {
                    price.Property(p => p.Value)
                        .HasColumnName("Price")
                        .HasColumnType("decimal(18,2)")
                        .IsRequired();
                });

                // Configurar Value Object StockQuantity como Owned Type
                entity.OwnsOne(e => e.Stock, stock =>
                {
                    stock.Property(s => s.Value)
                        .HasColumnName("Stock")
                        .IsRequired();
                });

                entity.Property(e => e.CreatedAt)
                    .IsRequired();

                entity.Property(e => e.UpdatedAt);

                // Relación con Category
                entity.HasOne(e => e.Category)
                    .WithMany(c => c.Products)
                    .HasForeignKey(e => e.CategoryId)
                    .OnDelete(DeleteBehavior.Restrict);
            });

            // Configuración de Category
            modelBuilder.Entity<Category>(entity =>
            {
                entity.HasKey(e => e.Id);

                entity.Property(e => e.Name)
                    .IsRequired()
                    .HasMaxLength(100);

                entity.Property(e => e.Description)
                    .HasMaxLength(500);

                entity.Property(e => e.IsActive)
                    .IsRequired();

                entity.Property(e => e.CreatedAt)
                    .IsRequired();

                entity.Property(e => e.UpdatedAt);

                // Índice único en Name
                entity.HasIndex(e => e.Name)
                    .IsUnique();
            });

            // Configuración de User
            modelBuilder.Entity<User>(entity =>
            {
                entity.HasKey(e => e.Id);

                entity.Property(e => e.Username)
                    .IsRequired()
                    .HasMaxLength(50);

                entity.Property(e => e.Email)
                    .IsRequired()
                    .HasMaxLength(100);

                entity.Property(e => e.PasswordHash)
                    .IsRequired();

                entity.Property(e => e.Role)
                    .IsRequired()
                    .HasMaxLength(20);

                entity.Property(e => e.CreatedAt)
                    .IsRequired();

                entity.Property(e => e.LastLoginAt);

                // Índices únicos
                entity.HasIndex(e => e.Username)
                    .IsUnique();

                entity.HasIndex(e => e.Email)
                    .IsUnique();
            });

            // Seed data inicial
            SeedData(modelBuilder);
        }

        private void SeedData(ModelBuilder modelBuilder)
        {
            // Categorías iniciales - usando objetos anónimos
            modelBuilder.Entity<Category>().HasData(
                new
                {
                    Id = 1,
                    Name = "Electrónicos",
                    Description = "Dispositivos electrónicos y gadgets",
                    IsActive = true,
                    CreatedAt = new DateTime(2024, 1, 1, 0, 0, 0, DateTimeKind.Utc)
                },
                new
                {
                    Id = 2,
                    Name = "Computación",
                    Description = "Equipos de cómputo y accesorios",
                    IsActive = true,
                    CreatedAt = new DateTime(2024, 1, 1, 0, 0, 0, DateTimeKind.Utc)
                },
                new
                {
                    Id = 3,
                    Name = "Móviles",
                    Description = "Teléfonos celulares y accesorios",
                    IsActive = true,
                    CreatedAt = new DateTime(2024, 1, 1, 0, 0, 0, DateTimeKind.Utc)
                }
            );

            // Usuario administrador inicial
            var hashedPassword = BCrypt.Net.BCrypt.HashPassword("Admin123!");
            modelBuilder.Entity<User>().HasData(
                new
                {
                    Id = 1,
                    Username = "admin",
                    Email = "admin@inventorysystem.com",
                    PasswordHash = hashedPassword,
                    Role = "Admin",
                    CreatedAt = new DateTime(2024, 1, 1, 0, 0, 0, DateTimeKind.Utc)
                }
            );

            // Productos de ejemplo con Value Objects
            modelBuilder.Entity<Product>().HasData(
                new
                {
                    Id = 1,
                    Name = "Laptop Dell Inspiron",
                    CategoryId = 2,
                    CreatedAt = new DateTime(2024, 1, 1, 0, 0, 0, DateTimeKind.Utc)
                },
                new
                {
                    Id = 2,
                    Name = "Mouse Logitech",
                    CategoryId = 2,
                    CreatedAt = new DateTime(2024, 1, 1, 0, 0, 0, DateTimeKind.Utc)
                },
                new
                {
                    Id = 3,
                    Name = "iPhone 15",
                    CategoryId = 3,
                    CreatedAt = new DateTime(2024, 1, 1, 0, 0, 0, DateTimeKind.Utc)
                },
                new
                {
                    Id = 4,
                    Name = "Audífonos Sony",
                    CategoryId = 1,
                    CreatedAt = new DateTime(2024, 1, 1, 0, 0, 0, DateTimeKind.Utc)
                }
            );

            // Seed data para Value Objects - se hace por separado
            // Price (usando el nombre de columna Price)
            modelBuilder.Entity<Product>().OwnsOne(p => p.Price).HasData(
                new { ProductId = 1, Value = 15000.00m },
                new { ProductId = 2, Value = 500.00m },
                new { ProductId = 3, Value = 25000.00m },
                new { ProductId = 4, Value = 2500.00m }
            );

            // Stock (usando el nombre de columna Stock)
            modelBuilder.Entity<Product>().OwnsOne(p => p.Stock).HasData(
                new { ProductId = 1, Value = 5 },
                new { ProductId = 2, Value = 25 },
                new { ProductId = 3, Value = 8 },
                new { ProductId = 4, Value = 15 }
            );
        }
    }
}