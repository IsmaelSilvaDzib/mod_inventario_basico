namespace SistemaInventario.Domain.Entities
{
        public class Category
        {
            public int Id { get;  set; }
            public string Name { get;  set; }
            public string Description { get;  set; }
            public bool IsActive { get;  set; }
            public DateTime CreatedAt { get;  set; }
            public DateTime? UpdatedAt { get;  set; }

            // Relación con productos
            public virtual ICollection<Product> Products { get; private set; } = new List<Product>();

            // Constructor privado para EF Core
            private Category() { }

            // Constructor para crear nuevas categorías
            public Category(string name, string description)
            {
                if (string.IsNullOrWhiteSpace(name))
                    throw new ArgumentException("El nombre de la categoría no puede estar vacío", nameof(name));

                Name = name;
                Description = description ?? string.Empty;
                IsActive = true;
                CreatedAt = DateTime.UtcNow;
            }

            // Métodos de negocio
            public void UpdateInfo(string name, string description)
            {
                if (!string.IsNullOrWhiteSpace(name))
                    Name = name;

                Description = description ?? Description;
                UpdatedAt = DateTime.UtcNow;
            }

            public void Deactivate()
            {
                IsActive = false;
                UpdatedAt = DateTime.UtcNow;
            }

            public void Activate()
            {
                IsActive = true;
                UpdatedAt = DateTime.UtcNow;
            }
        }
}
