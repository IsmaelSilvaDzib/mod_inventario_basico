using SistemaInventario.Application.DTO;
using SistemaInventario.Domain.Entities;
using SistemaInventario.Domain.Interfaces;
using SistemaInventario.Domain.ValueObjects;

namespace SistemaInventario.Application.Services
{
    public class ProductService
    {
        private readonly IProductRepository _productRepository;
        private readonly ICategoryRepository _categoryRepository;

        public ProductService(IProductRepository productRepository, ICategoryRepository categoryRepository)
        {
            _productRepository = productRepository;
            _categoryRepository = categoryRepository;
        }

        public async Task<IEnumerable<ProductDto>> GetAllProductsAsync()
        {
            var products = await _productRepository.GetAllAsync();
            return products.Select(MapToDto);
        }

        public async Task<ProductDto> GetProductByIdAsync(int id)
        {
            var product = await _productRepository.GetByIdAsync(id);
            return product != null ? MapToDto(product) : null;
        }

        public async Task<ProductDto> CreateProductAsync(CreateProductDto dto)
        {
            // Validaciones de negocio
            if (string.IsNullOrWhiteSpace(dto.Name))
                throw new ArgumentException("El nombre del producto es requerido");

            if (dto.Price <= 0)
                throw new ArgumentException("El precio debe ser mayor a cero");

            if (dto.Stock < 0)
                throw new ArgumentException("El stock no puede ser negativo");

            // Verificar que la categoría existe
            if (!await _categoryRepository.ExistsAsync(dto.CategoryId))
                throw new ArgumentException("La categoría especificada no existe");

            // Crear producto con Value Objects
            var product = new Product(
                dto.Name,
                new Money(dto.Price),
                new StockQuantity(dto.Stock),
                dto.CategoryId
            );

            var savedProduct = await _productRepository.AddAsync(product);
            return MapToDto(savedProduct);
        }

        public async Task<ProductDto> UpdateProductAsync(int id, UpdateProductDto dto)
        {
            var product = await _productRepository.GetByIdAsync(id);
            if (product == null)
                throw new ArgumentException("El producto no existe");

            // Validaciones de negocio
            if (string.IsNullOrWhiteSpace(dto.Name))
                throw new ArgumentException("El nombre del producto es requerido");

            if (dto.Price <= 0)
                throw new ArgumentException("El precio debe ser mayor a cero");

            if (dto.Stock < 0)
                throw new ArgumentException("El stock no puede ser negativo");

            // Verificar que la categoría existe si se está cambiando
            if (dto.CategoryId != product.CategoryId && !await _categoryRepository.ExistsAsync(dto.CategoryId))
                throw new ArgumentException("La categoría especificada no existe");

            product.Name = dto.Name;
            product.Price = new Money(dto.Price);
            product.Stock = new StockQuantity(dto.Stock);
            product.CategoryId = dto.CategoryId;
            product.UpdatedAt = DateTime.UtcNow;

            await _productRepository.UpdateAsync(product);
            return MapToDto(product);
        }

        public async Task<ProductDto> UpdateStockAsync(int id, UpdateStockDto dto)
        {
            var product = await _productRepository.GetByIdAsync(id);
            if (product == null)
                throw new ArgumentException("El producto no existe");

            if (dto.Stock < 0)
                throw new ArgumentException("El stock no puede ser negativo");

            product.Stock = new StockQuantity(dto.Stock);
            product.UpdatedAt = DateTime.UtcNow;

            await _productRepository.UpdateAsync(product);
            return MapToDto(product);
        }

        public async Task<bool> DeleteProductAsync(int id)
        {
            if (!await _productRepository.ExistsAsync(id))
                return false;

            await _productRepository.DeleteAsync(id);
            return true;
        }

        public async Task<IEnumerable<ProductDto>> SearchProductsAsync(string searchTerm)
        {
            var products = await _productRepository.SearchByNameAsync(searchTerm);
            return products.Select(MapToDto);
        }

        public async Task<IEnumerable<ProductDto>> GetProductsByCategoryAsync(int categoryId)
        {
            var products = await _productRepository.GetByCategoryAsync(categoryId);
            return products.Select(MapToDto);
        }

        public async Task<IEnumerable<ProductDto>> GetLowStockProductsAsync()
        {
            var products = await _productRepository.GetLowStockProductsAsync();
            return products.Select(MapToDto);
        }

        public async Task<ProductStatisticsDto> GetStatisticsAsync()
        {
            return new ProductStatisticsDto
            {
                TotalProducts = await _productRepository.GetTotalCountAsync(),
                TotalValue = await _productRepository.GetTotalValueAsync(),
                LowStockCount = await _productRepository.GetLowStockCountAsync(),
                TotalUnits = (await _productRepository.GetAllAsync()).Sum(p => p.Stock.Value),
                OutOfStockCount = (await _productRepository.GetAllAsync()).Count(p => p.Stock.Value == 0)
            };
        }

        // Método para aplicar descuento masivo (lógica de negocio)
        public async Task ApplyDiscountToAllProductsAsync(decimal discountPercentage)
        {
            if (discountPercentage <= 0 || discountPercentage > 100)
                throw new ArgumentException("El descuento debe estar entre 1 y 100");

            var products = await _productRepository.GetAllAsync();

            foreach (var product in products)
            {
                var discountedPrice = product.Price.ApplyDiscount(discountPercentage);
                product.UpdatePrice(discountedPrice);
                await _productRepository.UpdateAsync(product);
            }
        }

        // Mapeo de entidad a DTO
        private ProductDto MapToDto(Product product)
        {
            return new ProductDto
            {
                Id = product.Id,
                Name = product.Name,
                Price = product.Price.Value,
                Stock = product.Stock.Value,
                CategoryId = product.CategoryId,
                CategoryName = product.Category?.Name ?? "Sin categoría",
                CreatedAt = product.CreatedAt,
                UpdatedAt = product.UpdatedAt
            };
        }
    }
}