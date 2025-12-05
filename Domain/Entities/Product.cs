using SistemaInventario.Domain.ValueObjects;


namespace SistemaInventario.Domain.Entities
{
    public class Product
    {
        public int Id { get;  set; }
        public string Name { get;  set; }
        public Money Price { get;  set; }
        public StockQuantity Stock { get;  set; }
        public int CategoryId { get;  set; }
        public Category Category { get;  set; }
        public DateTime CreatedAt { get;  set; }
        public DateTime? UpdatedAt { get;  set; }

        // Constructor privado para EF Core
        private Product() { }

        // Constructor para crear nuevos productos
        public Product(string name, Money price, StockQuantity stock, int categoryId)
        {
            if (string.IsNullOrWhiteSpace(name))
                throw new ArgumentException("El nombre del producto no puede estar vacío", nameof(name));

            Name = name;
            Price = price;
            Stock = stock;
            CategoryId = categoryId;
            CreatedAt = DateTime.UtcNow;
        }

        // Métodos de negocio
        public void UpdateStock(StockQuantity newStock)
        {
            Stock = newStock;
            UpdatedAt = DateTime.UtcNow;
        }

        public void UpdatePrice(Money newPrice)
        {
            Price = newPrice;
            UpdatedAt = DateTime.UtcNow;
        }

        public bool IsLowStock() => Stock.Value < 10;
        public bool IsOutOfStock() => Stock.Value == 0;
    }
}