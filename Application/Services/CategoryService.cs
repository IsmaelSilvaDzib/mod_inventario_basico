using SistemaInventario.Application.DTO;
using SistemaInventario.Domain.Entities;
using SistemaInventario.Domain.Interfaces;

namespace SistemaInventario.Application.Services
{
    public class CategoryService
    {
        private readonly ICategoryRepository _categoryRepository;

        public CategoryService(ICategoryRepository categoryRepository)
        {
            _categoryRepository = categoryRepository;
        }

        public async Task<IEnumerable<CategoryDto>> GetAllCategoriesAsync()
        {
            var categories = await _categoryRepository.GetAllAsync();
            return categories.Select(MapToDto);
        }

        public async Task<IEnumerable<CategoryDto>> GetActiveCategoriesAsync()
        {
            var categories = await _categoryRepository.GetActiveAsync();
            return categories.Select(MapToDto);
        }

        public async Task<CategoryDto> GetCategoryByIdAsync(int id)
        {
            var category = await _categoryRepository.GetByIdAsync(id);
            return category != null ? MapToDto(category) : null;
        }

        public async Task<CategoryDto> CreateCategoryAsync(CreateCategoryDto dto)
        {
            // Validaciones de negocio
            if (string.IsNullOrWhiteSpace(dto.Name))
                throw new ArgumentException("El nombre de la categoría es requerido");

            // Verificar que no exista una categoría con el mismo nombre
            if (await _categoryRepository.ExistsByNameAsync(dto.Name))
                throw new ArgumentException("Ya existe una categoría con ese nombre");

            var category = new Category(dto.Name, dto.Description);
            var savedCategory = await _categoryRepository.AddAsync(category);

            return MapToDto(savedCategory);
        }

        public async Task<CategoryDto> UpdateCategoryAsync(int id, UpdateCategoryDto dto)
        {
            var category = await _categoryRepository.GetByIdAsync(id);
            if (category == null)
                throw new ArgumentException("La categoría no existe");

            // Validaciones de negocio
            if (string.IsNullOrWhiteSpace(dto.Name))
                throw new ArgumentException("El nombre de la categoría es requerido");

            // Verificar que no exista otra categoría con el mismo nombre
            var existingCategory = await _categoryRepository.GetByNameAsync(dto.Name);
            if (existingCategory != null && existingCategory.Id != id)
                throw new ArgumentException("Ya existe otra categoría con ese nombre");

            // Actualizar usando métodos del dominio
            category.UpdateInfo(dto.Name, dto.Description);

            if (!dto.IsActive && category.IsActive)
                category.Deactivate();
            else if (dto.IsActive && !category.IsActive)
                category.Activate();

            await _categoryRepository.UpdateAsync(category);
            return MapToDto(category);
        }

        public async Task<bool> DeleteCategoryAsync(int id)
        {
            var category = await _categoryRepository.GetByIdAsync(id);
            if (category == null)
                return false;

            // Validación de negocio: no permitir eliminar categorías con productos
            if (category.Products.Any())
                throw new InvalidOperationException("No se puede eliminar una categoría que tiene productos asociados");

            await _categoryRepository.DeleteAsync(id);
            return true;
        }

        public async Task<bool> DeactivateCategoryAsync(int id)
        {
            var category = await _categoryRepository.GetByIdAsync(id);
            if (category == null)
                return false;

            category.Deactivate();
            await _categoryRepository.UpdateAsync(category);
            return true;
        }

        // Mapeo de entidad a DTO
        private CategoryDto MapToDto(Category category)
        {
            return new CategoryDto
            {
                Id = category.Id,
                Name = category.Name,
                Description = category.Description,
                IsActive = category.IsActive,
                CreatedAt = category.CreatedAt,
                UpdatedAt = category.UpdatedAt,
                ProductCount = category.Products?.Count ?? 0
            };
        }
    }
}