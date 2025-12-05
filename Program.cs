using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using SistemaInventario.Application.Services;
using SistemaInventario.Domain.Interfaces;
using SistemaInventario.Infrastructure.Data;
using SistemaInventario.Infrastructure.Data.Repositories;
using System.Reflection;
using System.Security.Claims;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

// Configurar Entity Framework
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));



// Configurar repositorios (Dependency Injection)
builder.Services.AddScoped<IProductRepository, ProductRepository>();
builder.Services.AddScoped<ICategoryRepository, CategoryRepository>();
builder.Services.AddScoped<IUserRepository, UserRepository>();

// Configurar servicios de negocio
builder.Services.AddScoped<ProductService>();
builder.Services.AddScoped<CategoryService>();
builder.Services.AddScoped<AuthService>();

// Configurar autenticación JWT
var jwtConfig = builder.Configuration.GetSection("JwtConfig");
var secret = jwtConfig["Secret"];
var issuer = jwtConfig["Issuer"];
var audience = jwtConfig["Audience"];
var secretKey = Encoding.ASCII.GetBytes(secret);

builder.Services.AddAuthentication(x =>
{
    x.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    x.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(x =>
{
    x.RequireHttpsMetadata = false;
    x.SaveToken = true;

    x.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuerSigningKey = true,
        IssuerSigningKey = new SymmetricSecurityKey(secretKey),
        ValidateIssuer = true,
        ValidIssuer = issuer,
        ValidateAudience = true,
        ValidAudience = audience,
        ValidateLifetime = true,
        ClockSkew = TimeSpan.Zero
    };
});




// Configurar controladores
builder.Services.AddControllers();

// Configurar Swagger/OpenAPI
builder.Services.AddEndpointsApiExplorer();
 builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "Inventory System API",
        Version = "v1",
        Description = "API REST para Sistema de Gestión de Inventario",
        Contact = new OpenApiContact
        {
            Name = "TecNM Campus Cancún",
            Email = "sistemas@cancun.tecnm.mx"
        }
    });

    // Incluir comentarios XML
    var xmlFile = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
    var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
    if (File.Exists(xmlPath))
    {
        c.IncludeXmlComments(xmlPath);
    }

    // Configurar JWT en Swagger
    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Description = "JWT Authorization header usando el esquema Bearer. \n\nEjemplo: \"Authorization: Bearer {token}\"",
        Name = "Authorization",
        In = ParameterLocation.Header,
        Type = SecuritySchemeType.Http,
        Scheme = "Bearer",
        BearerFormat = "JWT"
    });

    c.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = "Bearer"
                },
            },
            new string[] { }
        }
    });

    // Organizar por tags
    c.TagActionsBy(api => new[] { api.ActionDescriptor.RouteValues["controller"] });
});



// Configurar CORS para desarrollo
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowLocalhost",
        policy =>
        {
            policy.WithOrigins(
                 "http://localhost:3000",
                 "http://127.0.0.1:5500",
                 "http://localhost:5500",
                 "http://localhost:8080",
                 "http://127.0.0.1:8080",
                 "file://"
             )
             .AllowAnyHeader()
             .AllowAnyMethod()
             .AllowCredentials();
        });
});

// Configurar logging
builder.Logging.ClearProviders();
builder.Logging.AddConsole();

builder.Services.AddDirectoryBrowser();
var app = builder.Build();


// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "Inventory System API v1");
        c.DocumentTitle = "Inventory System API - Documentación";
        c.DefaultModelsExpandDepth(-1); // Ocultar modelos por defecto
        c.DocExpansion(Swashbuckle.AspNetCore.SwaggerUI.DocExpansion.None);
    });
}

app.UseHttpsRedirection();

// SERVIR ARCHIVOS ESTÁTICOS
app.UseStaticFiles();

// Para ver directorio de archivos
app.UseDirectoryBrowser();


// Habilitar CORS
app.UseCors("AllowLocalhost");


app.UseAuthentication();  // JWT authentication
app.UseAuthorization();
app.MapControllers();

app.Run();