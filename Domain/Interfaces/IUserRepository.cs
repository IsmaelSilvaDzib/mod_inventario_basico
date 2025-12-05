using SistemaInventario.Domain.Entities;


namespace SistemaInventario.Domain.Interfaces
{
    public interface IUserRepository
    {
        // Operaciones básicas CRUD
        Task<User> GetByIdAsync(int id);
        Task<IEnumerable<User>> GetAllAsync();
        Task<User> AddAsync(User user);
        Task UpdateAsync(User user);
        Task DeleteAsync(int id);

        // Consultas específicas del dominio
        Task<User> GetByUsernameAsync(string username);
        Task<User> GetByEmailAsync(string email);
        Task<bool> ExistsByUsernameAsync(string username);
        Task<bool> ExistsByEmailAsync(string email);
    }
}