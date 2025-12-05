using Microsoft.IdentityModel.Tokens;
using SistemaInventario.Application.DTO;
using SistemaInventario.Domain.Entities;
using SistemaInventario.Domain.Interfaces;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;


namespace SistemaInventario.Application.Services
{
    public class AuthService
    {
        private readonly IUserRepository _userRepository;
        private readonly IConfiguration _configuration;

        public AuthService(IUserRepository userRepository, IConfiguration configuration)
        {
            _userRepository = userRepository;
            _configuration = configuration;
        }

        public async Task<LoginResponseDto> LoginAsync(LoginDto loginDto)
        {
            // Validaciones básicas
            if (string.IsNullOrWhiteSpace(loginDto.Username))
                throw new ArgumentException("El username es requerido");

            if (string.IsNullOrWhiteSpace(loginDto.Password))
                throw new ArgumentException("La contraseña es requerida");

            // Buscar usuario
            var user = await _userRepository.GetByUsernameAsync(loginDto.Username);
            if (user == null)
                throw new UnauthorizedAccessException("Credenciales inválidas");

            // Verificar contraseña
            if (!BCrypt.Net.BCrypt.Verify(loginDto.Password, user.PasswordHash))
                throw new UnauthorizedAccessException("Credenciales inválidas");

            // Actualizar último login
            user.UpdateLastLogin();
            await _userRepository.UpdateAsync(user);

            // Generar token JWT
            var token = GenerateJwtToken(user);
            var expiresAt = DateTime.UtcNow.AddMinutes(GetJwtExpirationMinutes());

            return new LoginResponseDto
            {
                Token = token,
                User = MapUserToDto(user),
                ExpiresAt = expiresAt
            };
        }

        public async Task<UserDto> RegisterAsync(CreateUserDto createUserDto)
        {
            // Validaciones
            if (string.IsNullOrWhiteSpace(createUserDto.Username))
                throw new ArgumentException("El username es requerido");

            if (string.IsNullOrWhiteSpace(createUserDto.Email))
                throw new ArgumentException("El email es requerido");

            if (string.IsNullOrWhiteSpace(createUserDto.Password))
                throw new ArgumentException("La contraseña es requerida");

            if (createUserDto.Password.Length < 6)
                throw new ArgumentException("La contraseña debe tener al menos 6 caracteres");

            // Verificar que no exista el usuario
            if (await _userRepository.ExistsByUsernameAsync(createUserDto.Username))
                throw new ArgumentException("El username ya está en uso");

            if (await _userRepository.ExistsByEmailAsync(createUserDto.Email))
                throw new ArgumentException("El email ya está en uso");

            // Hash de la contraseña
            var passwordHash = BCrypt.Net.BCrypt.HashPassword(createUserDto.Password);

            // Crear usuario
            var user = new User(
                createUserDto.Username,
                createUserDto.Email,
                passwordHash,
                createUserDto.Role
            );

            var savedUser = await _userRepository.AddAsync(user);
            return MapUserToDto(savedUser);
        }

        public async Task<bool> ChangePasswordAsync(int userId, ChangePasswordDto changePasswordDto)
        {
            var user = await _userRepository.GetByIdAsync(userId);
            if (user == null)
                throw new ArgumentException("Usuario no encontrado");

            // Verificar contraseña actual
            if (!BCrypt.Net.BCrypt.Verify(changePasswordDto.CurrentPassword, user.PasswordHash))
                throw new UnauthorizedAccessException("La contraseña actual es incorrecta");

            // Validar nueva contraseña
            if (string.IsNullOrWhiteSpace(changePasswordDto.NewPassword))
                throw new ArgumentException("La nueva contraseña es requerida");

            if (changePasswordDto.NewPassword.Length < 6)
                throw new ArgumentException("La nueva contraseña debe tener al menos 6 caracteres");

            // Actualizar contraseña
            var newPasswordHash = BCrypt.Net.BCrypt.HashPassword(changePasswordDto.NewPassword);

            // Crear un nuevo usuario con la contraseña actualizada (esto es simplificado)
            var updatedUser = new User(user.Username, user.Email, newPasswordHash, user.Role)
            {
                Id = user.Id
            };

            await _userRepository.UpdateAsync(updatedUser);
            return true;
        }

        private string GenerateJwtToken(User user)
        {
            var jwtConfig = _configuration.GetSection("JwtConfig");
            var key = Encoding.ASCII.GetBytes(jwtConfig["Secret"]);

            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
                new Claim(ClaimTypes.Name, user.Username),
                new Claim(ClaimTypes.Email, user.Email),
                new Claim(ClaimTypes.Role, user.Role)
            };

            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(claims),
                Expires = DateTime.UtcNow.AddMinutes(GetJwtExpirationMinutes()),
                Issuer = jwtConfig["Issuer"],
                Audience = jwtConfig["Audience"],
                SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key),
                    SecurityAlgorithms.HmacSha256Signature)
            };

            var tokenHandler = new JwtSecurityTokenHandler();
            var token = tokenHandler.CreateToken(tokenDescriptor);
            return tokenHandler.WriteToken(token);
        }

        private int GetJwtExpirationMinutes()
        {
            var minutes = _configuration.GetSection("JwtConfig").GetValue<int>("ExpirationInMinutes", 1440);
            Console.WriteLine($"JWT Expiration configured for: {minutes} minutes");
            return minutes;
        }

        private UserDto MapUserToDto(User user)
        {
            return new UserDto
            {
                Id = user.Id,
                Username = user.Username,
                Email = user.Email,
                Role = user.Role,
                CreatedAt = user.CreatedAt,
                LastLoginAt = user.LastLoginAt
            };
        }
    }
}