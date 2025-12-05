    using SistemaInventario.Domain.Entities;

    namespace SistemaInventario.Domain.Interfaces
    {
        public interface IProductRepository
        {
            // Operaciones básicas CRUD
            Task<Product> GetByIdAsync(int id);
            Task<IEnumerable<Product>> GetAllAsync();
            Task<Product> AddAsync(Product product);
            Task UpdateAsync(Product product);
            Task DeleteAsync(int id);

            // Consultas específicas del dominio
            Task<IEnumerable<Product>> GetByCategoryAsync(int categoryId);
            Task<IEnumerable<Product>> GetLowStockProductsAsync();
            Task<IEnumerable<Product>> SearchByNameAsync(string name);
            Task<bool> ExistsAsync(int id);

            // Estadísticas
            Task<int> GetTotalCountAsync();
            Task<decimal> GetTotalValueAsync();
            Task<int> GetLowStockCountAsync();
        }
    }

