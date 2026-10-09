using System.Text;
using FluentValidation;
using InventoryApp.Api.Data;
using InventoryApp.Api.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi;

var builder = WebApplication.CreateBuilder(args);

// ==========================================
// ФАЗА 1: Реєстрація сервісів (DI Container)
// Аналог: providers: [] в Angular
// ==========================================

builder.Services.AddControllers();          // Додаємо підтримку класичних контролерів
builder.Services.AddValidatorsFromAssemblyContaining<InventoryApp.Api.Validators.CreateItemDtoValidator>();
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));
builder.Services.AddRouting(options => options.LowercaseUrls = true);
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAngular", policy =>
    {
        policy.WithOrigins("http://localhost:4200", "https://localhost:4200")
              .AllowAnyHeader()
              .AllowAnyMethod()
              .AllowCredentials();
    });
});
builder.Services.AddEndpointsApiExplorer(); // Допомагає Swagger знаходити наші маршрути
builder.Services.AddSwaggerGen(c =>
{
    // Додаємо кнопку "Authorize" в Swagger
    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Description = "Provide JWT token in format: Bearer {your_token}",
        Name = "Authorization",
        In = ParameterLocation.Header,
        Type = SecuritySchemeType.ApiKey,
        Scheme = "Bearer"
    });

    // Змушуємо Swagger додавати цей заголовок до кожного запиту
    c.AddSecurityRequirement(document => new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecuritySchemeReference("Bearer", document),
            []
        }
    });
});
builder.Services.AddScoped<IInventoryService, InventoryService>(); // Реєструємо наш сервіс у DI контейнері
builder.Services.AddScoped<IAuthService, AuthService>();           // Реєструємо AuthService у DI контейнері
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        var jwtSettings = builder.Configuration.GetSection("JwtSettings");
        var secretKey = jwtSettings["Key"] ?? throw new InvalidOperationException("JWT Key is missing");

        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,            // Хто видав токен? (Має співпадати з Issuer)
            ValidIssuer = jwtSettings["Issuer"],
            
            ValidateAudience = true,          // Для кого видано? (Має співпадати з Audience)
            ValidAudience = jwtSettings["Audience"],
            
            ValidateLifetime = true,          // Чи не закінчився час дії токена?
            
            ValidateIssuerSigningKey = true,  // Чи співпадає секретний підпис?
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey))
        };
    });

// Додаємо підтримку Авторизації
builder.Services.AddAuthorization();

var app = builder.Build(); // Збираємо додаток

// --- ДОДАЄМО ЦЕЙ БЛОК ---
// Автоматично застосовуємо міграції при старті програми
using (var scope = app.Services.CreateScope())
{
    var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    dbContext.Database.Migrate();

    // Seed default categories if none exist
    if (!dbContext.Categories.Any())
    {
        dbContext.Categories.AddRange(
            new InventoryApp.Api.Models.Category { Name = "Gunpla" },
            new InventoryApp.Api.Models.Category { Name = "Figures" },
            new InventoryApp.Api.Models.Category { Name = "Accessories" },
            new InventoryApp.Api.Models.Category { Name = "Other" }
        );
        dbContext.SaveChanges();
    }

    // Seed users with hashed passwords if none exist
    if (!dbContext.Users.Any())
    {
        var adminUsername = builder.Configuration["AdminSettings:Username"]
            ?? throw new InvalidOperationException("AdminSettings:Username is not configured");
        var adminPassword = builder.Configuration["AdminSettings:Password"]
            ?? throw new InvalidOperationException("AdminSettings:Password is not configured");

        dbContext.Users.AddRange(
            new InventoryApp.Api.Models.User
            {
                Username = adminUsername,
                PasswordHash = BCrypt.Net.BCrypt.HashPassword(adminPassword),
                Role = "Admin"
            },
            new InventoryApp.Api.Models.User
            {
                Username = "user",
                PasswordHash = BCrypt.Net.BCrypt.HashPassword("user123"),
                Role = "Customer"
            }
        );
        dbContext.SaveChanges();
    }
}
// ------------------------

// ==========================================
// ФАЗА 2: Налаштування Middleware (Конвеєр)
// Важливо: Порядок виклику методів тут має значення!
// ==========================================

app.UseMiddleware<InventoryApp.Api.Middlewares.ExceptionHandlingMiddleware>();

app.UseMiddleware<InventoryApp.Api.Middlewares.RequestTimingMiddleware>();

// Вмикаємо інтерфейс Swagger лише для локальної розробки
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(); // Генерує красивий HTML за адресою /swagger
}

app.UseHttpsRedirection(); // Перенаправлення HTTP на HTTPS
app.UseCors("AllowAngular");
var imagesPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "images");
if (!Directory.Exists(imagesPath))
{
    Directory.CreateDirectory(imagesPath);
}

app.UseStaticFiles(new StaticFileOptions
{
    FileProvider = new Microsoft.Extensions.FileProviders.PhysicalFileProvider(imagesPath),
    RequestPath = "/images"
});
// Порядок має значення! Спочатку перевіряємо хто ти (Authentication), потім чи маєш права (Authorization)
app.UseAuthentication(); 
app.UseAuthorization();
app.MapControllers();      // Підключення маршрутів з наших контролерів

app.Run(); // Запуск сервера