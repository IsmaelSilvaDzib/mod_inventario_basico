namespace SistemaInventario.Domain.Entities
{
        public class User
        {
            public int Id { get;  set; }
            public string Username { get;  set; }
            public string Email { get;  set; }
            public string PasswordHash { get;  set; }
            public string Role { get;  set; }
            public DateTime CreatedAt { get;  set; }
            public DateTime? LastLoginAt { get;  set; }

            // Constructor privado para EF Core
            private User() { }

            // Constructor para crear nuevos usuarios
            public User(string username, string email, string passwordHash, string role = "User")
            {
                if (string.IsNullOrWhiteSpace(username))
                    throw new ArgumentException("El username no puede estar vacío", nameof(username));

                if (string.IsNullOrWhiteSpace(email))
                    throw new ArgumentException("El email no puede estar vacío", nameof(email));

                Username = username;
                Email = email;
                PasswordHash = passwordHash;
                Role = role;
                CreatedAt = DateTime.UtcNow;
            }

            // Métodos de negocio
            public void UpdateLastLogin()
            {
                LastLoginAt = DateTime.UtcNow;
            }

            public bool IsAdmin() => Role.Equals("Admin", StringComparison.OrdinalIgnoreCase);
        }
}
