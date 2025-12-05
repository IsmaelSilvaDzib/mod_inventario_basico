using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SistemaInventario.Application.DTO;
using SistemaInventario.Application.Services;
using SistemaInventario.Domain.Entities;
using System.Security.Claims;


namespace SistemaInventario.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Produces("application/json")]
    public class AuthController : ControllerBase
    {
        private readonly AuthService _authService;
        private readonly ILogger<AuthController> _logger;

        public AuthController(AuthService authService, ILogger<AuthController> logger)
        {
            _authService = authService;
            _logger = logger;
        }

        /// <summary>
        /// Autentica un usuario y devuelve un token JWT
        /// </summary>
        /// <param name="loginDto">Credenciales de login</param>
        /// <returns>Token JWT y datos del usuario</returns>
        [HttpPost("login")]
        [ProducesResponseType(typeof(LoginResponseDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<LoginResponseDto>> Login([FromBody] LoginDto loginDto)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }

                _logger.LogInformation("Intento de login para usuario: {Username}", loginDto.Username);
                var result = await _authService.LoginAsync(loginDto);

                _logger.LogInformation("Login exitoso para usuario: {Username}", loginDto.Username);
                return Ok(result);
            }
            catch (UnauthorizedAccessException ex)
            {
                _logger.LogWarning(ex, "Login fallido para usuario: {Username}", loginDto.Username);
                return Unauthorized(new { message = "Credenciales inválidas" });
            }
            catch (ArgumentException ex)
            {
                _logger.LogWarning(ex, "Datos inválidos en login para usuario: {Username}", loginDto.Username);
                return BadRequest(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error en login para usuario: {Username}", loginDto.Username);
                return StatusCode(500, new { message = "Error interno del servidor" });
            }
        }

        /// <summary>
        /// Registra un nuevo usuario en el sistema
        /// </summary>
        /// <param name="createUserDto">Datos del nuevo usuario</param>
        /// <returns>Datos del usuario creado</returns>
        [HttpPost("register")]
        [ProducesResponseType(typeof(UserDto), StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<UserDto>> Register([FromBody] CreateUserDto createUserDto)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }

                _logger.LogInformation("Registrando nuevo usuario: {Username}", createUserDto.Username);
                var user = await _authService.RegisterAsync(createUserDto);

                _logger.LogInformation("Usuario registrado exitosamente: {Username}", createUserDto.Username);
                return CreatedAtAction(nameof(GetProfile), new { }, user);
            }
            catch (ArgumentException ex)
            {
                _logger.LogWarning(ex, "Error de validación al registrar usuario: {Username}", createUserDto.Username);
                return BadRequest(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al registrar usuario: {Username}", createUserDto.Username);
                return StatusCode(500, new { message = "Error interno del servidor" });
            }
        }

        /// <summary>
        /// Obtiene el perfil del usuario autenticado
        /// </summary>
        /// <returns>Datos del usuario actual</returns>
        [HttpGet("profile")]
        [Authorize] // Requiere autenticación
        [ProducesResponseType(typeof(UserDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<UserDto>> GetProfile()
        {
            try
            {
                // Obtener ID del usuario del token JWT
                var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (string.IsNullOrEmpty(userIdClaim) || !int.TryParse(userIdClaim, out int userId))
                {
                    return Unauthorized(new { message = "Token inválido" });
                }

                _logger.LogInformation("Obteniendo perfil para usuario ID: {UserId}", userId);

                // Aquí necesitaríamos un UserService.GetByIdAsync, por simplicidad retornamos la info del token
                var username = User.FindFirst(ClaimTypes.Name)?.Value;
                var email = User.FindFirst(ClaimTypes.Email)?.Value;
                var role = User.FindFirst(ClaimTypes.Role)?.Value;

                return Ok(new UserDto
                {
                    Id = userId,
                    Username = username,
                    Email = email,
                    Role = role
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener perfil de usuario");
                return StatusCode(500, new { message = "Error interno del servidor" });
            }
        }

        /// <summary>
        /// Cambia la contraseña del usuario autenticado
        /// </summary>
        /// <param name="changePasswordDto">Contraseña actual y nueva contraseña</param>
        /// <returns>Confirmación de cambio de contraseña</returns>
        [HttpPost("change-password")]
        [Authorize] // Requiere autenticación
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> ChangePassword([FromBody] ChangePasswordDto changePasswordDto)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }

                // Obtener ID del usuario del token JWT
                var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (string.IsNullOrEmpty(userIdClaim) || !int.TryParse(userIdClaim, out int userId))
                {
                    return Unauthorized(new { message = "Token inválido" });
                }

                _logger.LogInformation("Cambiando contraseña para usuario ID: {UserId}", userId);
                var success = await _authService.ChangePasswordAsync(userId, changePasswordDto);

                if (success)
                {
                    return Ok(new { message = "Contraseña cambiada exitosamente" });
                }

                return BadRequest(new { message = "No se pudo cambiar la contraseña" });
            }
            catch (UnauthorizedAccessException ex)
            {
                _logger.LogWarning(ex, "Contraseña actual incorrecta");
                return BadRequest(new { message = ex.Message });
            }
            catch (ArgumentException ex)
            {
                _logger.LogWarning(ex, "Error de validación al cambiar contraseña");
                return BadRequest(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al cambiar contraseña");
                return StatusCode(500, new { message = "Error interno del servidor" });
            }
        }

        /// <summary>
        /// Verifica si un token JWT es válido
        /// </summary>
        /// <returns>Estado de validez del token</returns>
        [HttpGet("validate-token")]
        [Authorize] // Requiere autenticación
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public IActionResult ValidateToken()
        {
            try
            {
                var username = User.FindFirst(ClaimTypes.Name)?.Value;
                var role = User.FindFirst(ClaimTypes.Role)?.Value;

                return Ok(new
                {
                    valid = true,
                    username = username,
                    role = role,
                    message = "Token válido"
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al validar token");
                return StatusCode(500, new { message = "Error interno del servidor" });
            }
        }
    }
}