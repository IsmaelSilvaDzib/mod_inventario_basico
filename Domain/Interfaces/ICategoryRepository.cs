using SistemaInventario.Domain.Entities;

namespace SistemaInventario.Domain.Interfaces
{
    public interface ICategoryRepository
    {
        // Operaciones básicas CRUD
        Task<Category> GetByIdAsync(int id);
        Task<IEnumerable<Category>> GetAllAsync();
        Task<Category> AddAsync(Category category);
        Task UpdateAsync(Category category);
        Task DeleteAsync(int id);

        // Consultas específicas del dominio
        Task<IEnumerable<Category>> GetActiveAsync();
        Task<Category> GetByNameAsync(string name);
        Task<bool> ExistsAsync(int id);
        Task<bool> ExistsByNameAsync(string name);
    }
}