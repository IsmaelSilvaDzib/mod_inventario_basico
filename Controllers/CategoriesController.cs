using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SistemaInventario.Application.DTO;
using SistemaInventario.Application.Services;
using System.Security.Claims;


namespace SistemaInventario.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Produces("application/json")]
    public class CategoriesController : ControllerBase
    {
        private readonly CategoryService _categoryService;
        private readonly ProductService _productService;
        private readonly ILogger<CategoriesController> _logger;

        public CategoriesController(
            CategoryService categoryService,
            ProductService productService,
            ILogger<CategoriesController> logger)
        {
            _categoryService = categoryService;
            _productService = productService;
            _logger = logger;
        }

        /// <summary>
        /// Obtiene todas las categorías
        /// </summary>
        /// <returns>Lista de todas las categorías</returns>
        [HttpGet]
        [ProducesResponseType(typeof(IEnumerable<CategoryDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<IEnumerable<CategoryDto>>> GetAllCategories()
        {
            try
            {
                _logger.LogInformation("Obteniendo todas las categorías");
                var categories = await _categoryService.GetAllCategoriesAsync();
                return Ok(categories);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener categorías");
                return StatusCode(500, new { message = "Error interno del servidor" });
            }
        }

        /// <summary>
        /// Obtiene solo las categorías activas
        /// </summary>
        /// <returns>Lista de categorías activas</returns>
        [HttpGet("active")]
        [ProducesResponseType(typeof(IEnumerable<CategoryDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<IEnumerable<CategoryDto>>> GetActiveCategories()
        {
            try
            {
                _logger.LogInformation("Obteniendo categorías activas");
                var categories = await _categoryService.GetActiveCategoriesAsync();
                return Ok(categories);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener categorías activas");
                return StatusCode(500, new { message = "Error interno del servidor" });
            }
        }

        /// <summary>
        /// Obtiene una categoría específica por su ID
        /// </summary>
        /// <param name="id">ID de la categoría</param>
        /// <returns>Categoría encontrada</returns>
        [HttpGet("{id}")]
        [ProducesResponseType(typeof(CategoryDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<CategoryDto>> GetCategory(int id)
        {
            try
            {
                _logger.LogInformation("Obteniendo categoría con ID: {CategoryId}", id);
                var category = await _categoryService.GetCategoryByIdAsync(id);

                if (category == null)
                {
                    _logger.LogWarning("Categoría con ID {CategoryId} no encontrada", id);
                    return NotFound(new { message = $"Categoría con ID {id} no encontrada" });
                }

                return Ok(category);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener categoría con ID: {CategoryId}", id);
                return StatusCode(500, new { message = "Error interno del servidor" });
            }
        }

        /// <summary>
        /// Obtiene todos los productos de una categoría específica
        /// </summary>
        /// <param name="id">ID de la categoría</param>
        /// <returns>Lista de productos de la categoría</returns>
        [HttpGet("{id}/products")]
        [ProducesResponseType(typeof(IEnumerable<ProductDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<IEnumerable<ProductDto>>> GetProductsByCategory(int id)
        {
            try
            {
                _logger.LogInformation("Obteniendo productos de la categoría: {CategoryId}", id);

                // Verificar que la categoría existe
                var category = await _categoryService.GetCategoryByIdAsync(id);
                if (category == null)
                {
                    return NotFound(new { message = $"Categoría con ID {id} no encontrada" });
                }

                var products = await _productService.GetProductsByCategoryAsync(id);
                return Ok(products);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener productos de la categoría: {CategoryId}", id);
                return StatusCode(500, new { message = "Error interno del servidor" });
            }
        }

        /// <summary>
        /// Crea una nueva categoría
        /// </summary>
        /// <param name="createCategoryDto">Datos de la categoría a crear</param>
        /// <returns>Categoría creada</returns>
        [HttpPost]
        [Authorize] // Requiere autenticación
        [ProducesResponseType(typeof(CategoryDto), StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<CategoryDto>> CreateCategory([FromBody] CreateCategoryDto createCategoryDto)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }

                _logger.LogInformation("Creando nueva categoría: {CategoryName}", createCategoryDto.Name);
                var category = await _categoryService.CreateCategoryAsync(createCategoryDto);

                return CreatedAtAction(
                    nameof(GetCategory),
                    new { id = category.Id },
                    category);
            }
            catch (ArgumentException ex)
            {
                _logger.LogWarning(ex, "Error de validación al crear categoría");
                return BadRequest(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al crear categoría");
                return StatusCode(500, new { message = "Error interno del servidor" });
            }
        }

        /// <summary>
        /// Actualiza una categoría existente
        /// </summary>
        /// <param name="id">ID de la categoría a actualizar</param>
        /// <param name="updateCategoryDto">Datos actualizados de la categoría</param>
        /// <returns>Categoría actualizada</returns>
        [HttpPut("{id}")]
        [Authorize] // Requiere autenticación
        [ProducesResponseType(typeof(CategoryDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<CategoryDto>> UpdateCategory(int id, [FromBody] UpdateCategoryDto updateCategoryDto)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }

                _logger.LogInformation("Actualizando categoría con ID: {CategoryId}", id);
                var category = await _categoryService.UpdateCategoryAsync(id, updateCategoryDto);

                return Ok(category);
            }
            catch (ArgumentException ex)
            {
                _logger.LogWarning(ex, "Error de validación al actualizar categoría con ID: {CategoryId}", id);
                return BadRequest(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al actualizar categoría con ID: {CategoryId}", id);
                return StatusCode(500, new { message = "Error interno del servidor" });
            }
        }

        /// <summary>
        /// Desactiva una categoría
        /// </summary>
        /// <param name="id">ID de la categoría a desactivar</param>
        /// <returns>Confirmación de desactivación</returns>
        [HttpPatch("{id}/deactivate")]
        [Authorize(Roles = "Admin")] // Solo administradores
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> DeactivateCategory(int id)
        {
            try
            {
                _logger.LogInformation("Desactivando categoría con ID: {CategoryId}", id);
                var success = await _categoryService.DeactivateCategoryAsync(id);

                if (!success)
                {
                    return NotFound(new { message = $"Categoría con ID {id} no encontrada" });
                }

                return Ok(new { message = "Categoría desactivada correctamente" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al desactivar categoría con ID: {CategoryId}", id);
                return StatusCode(500, new { message = "Error interno del servidor" });
            }
        }

        /// <summary>
        /// Elimina una categoría del sistema
        /// </summary>
        /// <param name="id">ID de la categoría a eliminar</param>
        /// <returns>Confirmación de eliminación</returns>
        [HttpDelete("{id}")]
        [Authorize(Roles = "Admin")] // Solo administradores
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> DeleteCategory(int id)
        {
            try
            {
                _logger.LogInformation("Eliminando categoría con ID: {CategoryId}", id);
                var deleted = await _categoryService.DeleteCategoryAsync(id);

                if (!deleted)
                {
                    return NotFound(new { message = $"Categoría con ID {id} no encontrada" });
                }

                return NoContent();
            }
            catch (InvalidOperationException ex)
            {
                _logger.LogWarning(ex, "No se puede eliminar categoría con productos asociados: {CategoryId}", id);
                return BadRequest(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al eliminar categoría con ID: {CategoryId}", id);
                return StatusCode(500, new { message = "Error interno del servidor" });
            }
        }
    }
}