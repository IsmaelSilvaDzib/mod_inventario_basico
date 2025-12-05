# mod_inventario_basico

## 🚀 Instalación y Configuración

### Prerrequisitos
- [.NET 7 SDK](https://dotnet.microsoft.com/download/dotnet/7.0)
- SQL Server Express
- Visual Studio 2022 / VS Code / cualquier editor

### Pasos de instalación

1. **Clonar el repositorio**
```bash
   git clone https://github.com/IsmaelSilvaDzib/mod_inventario_basico.git
   cd mod_inventario_basico
```

2. **Restaurar dependencias**
```bash
   dotnet restore
```

3. **Configurar base de datos**
   
   Crear `appsettings.Development.json`:
```json
   {
     "ConnectionStrings": {
       "DefaultConnection": "Server=RUTADELSERVIDOR;Database=InventorySystemDb;Trusted_Connection=true;MultipleActiveResultSets=true;TrustServerCertificate=true"
     },
     "JwtConfig": {
       "Secret": "ThisIsMyVerySecretKeyForJWTTokensInventorySystem2024!",
       "Issuer": "SistemaInventario",
       "Audience": "SistemaInventario.Client",
       "ExpirationInMinutes": 1440
     }
   }
```

4. **Crear base de datos**
```bash
   dotnet ef database update
```

5. **Ejecutar proyecto**
```bash
   dotnet run
```

6. **Acceder a la aplicación**
   - API: https://localhost:7156/swagger
   - Frontend: https://localhost:7156/sistema-inventario-integrado.html
   - **Login:** admin / Admin123!

### Dependencias incluidas
- Entity Framework Core 7.0.20
- JWT Bearer Authentication 7.0.20
- BCrypt.Net-Next 4.0.3
- Swagger/OpenAPI

> 📝 **Nota:** No es necesario instalar manualmente las dependencias. `dotnet restore` las descarga automáticamente.
