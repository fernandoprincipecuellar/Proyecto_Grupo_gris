using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using Proyecto_Grupo_gris.Api.Mappings;
using Proyecto_Grupo_gris.Api.Repositories;
using Proyecto_Grupo_gris.Api.Repositories.Interfaces;
using Proyecto_Grupo_gris.Api.Services;
using Proyecto_Grupo_gris.Api.Services.Interfaces;
using Proyecto_Grupo_gris.Data;
using Proyecto_Grupo_gris.Models;
using Microsoft.AspNetCore.HttpOverrides;

var builder = WebApplication.CreateBuilder(args);
var configuration = builder.Configuration;

// 🔥 DB
var connectionString = configuration.GetConnectionString("Default")
    ?? throw new InvalidOperationException("Connection string 'Default' not found.");

builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseNpgsql(connectionString));

builder.Services.AddDatabaseDeveloperPageExceptionFilter();

// 🔥 IDENTITY
builder.Services.AddIdentity<ApplicationUser, IdentityRole>(options =>
{
    options.SignIn.RequireConfirmedAccount = false;
    options.User.RequireUniqueEmail = true;
    options.Password.RequireDigit = true;
    options.Password.RequireLowercase = true;
    options.Password.RequireUppercase = true;
    options.Password.RequireNonAlphanumeric = false;
    options.Password.RequiredLength = 8;
})
.AddEntityFrameworkStores<ApplicationDbContext>()
.AddDefaultTokenProviders();

// 🔥 AUTHENTICATION JWT + COOKIES
var jwtSettings = configuration.GetSection("JwtSettings");
var jwtKey = jwtSettings["Secret"] ?? throw new InvalidOperationException("JwtSettings:Secret no configurado.");
var issuer = jwtSettings["Issuer"] ?? "EcoRutApi";
var audience = jwtSettings["Audience"] ?? "EcoRutClients";
var tokenValidationParameters = new TokenValidationParameters
{
    ValidateIssuer = true,
    ValidIssuer = issuer,
    ValidateAudience = true,
    ValidAudience = audience,
    ValidateLifetime = true,
    IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey)),
    ValidateIssuerSigningKey = true,
    ClockSkew = TimeSpan.FromMinutes(2)
};

builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = IdentityConstants.ApplicationScheme;
    options.DefaultChallengeScheme = IdentityConstants.ApplicationScheme;
})
.AddJwtBearer(JwtBearerDefaults.AuthenticationScheme, options =>
{
    options.TokenValidationParameters = tokenValidationParameters;
})
.AddGoogle("Google", options =>
{
    options.ClientId = configuration["Authentication:Google:ClientId"] ?? string.Empty;
    options.ClientSecret = configuration["Authentication:Google:ClientSecret"] ?? string.Empty;
    options.SignInScheme = IdentityConstants.ExternalScheme;
    options.SaveTokens = true;
});

builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("ApiAccess", policy =>
        policy.AddAuthenticationSchemes(JwtBearerDefaults.AuthenticationScheme)
              .RequireAuthenticatedUser());
    options.AddPolicy("Admin", policy => policy.RequireRole("Admin"));
});

// 🔥 AutoMapper
builder.Services.AddAutoMapper(typeof(AutoMapperProfile));

// 🔥 HttpClient for external services
builder.Services.AddHttpClient();
builder.Services.AddHttpClient<IGoogleMapsService, GoogleMapsService>();
builder.Services.AddHttpClient<IWeatherService, WeatherService>();

// 🔥 Repositorios y servicios API
builder.Services.AddScoped<IUserRepository, UserRepository>();
builder.Services.AddScoped<IEcoRouteRepository, EcoRouteRepository>();
builder.Services.AddScoped<IForumPostRepository, ForumPostRepository>();
builder.Services.AddScoped<ICommentRepository, CommentRepository>();

builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<IUserService, UserService>();
builder.Services.AddScoped<IEcoRouteService, EcoRouteService>();
builder.Services.AddScoped<IForumPostService, ForumPostService>();
builder.Services.AddScoped<ICommentService, CommentService>();
builder.Services.AddScoped<ICloudinaryService, CloudinaryService>();

// 🔥 Swagger
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "EcoRut API",
        Version = "v1",
        Description = "API REST para usuarios, rutas ecológicas, foro y comentarios"
    });

    options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = SecuritySchemeType.Http,
        Scheme = "bearer",
        BearerFormat = "JWT",
        In = ParameterLocation.Header,
        Description = "Ingresa JWT con el prefijo Bearer"
    });

    options.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            Array.Empty<string>()
        }
    });
});

builder.Services.AddControllersWithViews();
builder.Services.AddRazorPages();

// 🔥 REDIS - Caché distribuida y sesiones
var redisConnection = configuration.GetConnectionString("Redis");

// En entorno de desarrollo usamos caché en memoria para evitar depender
// de un Redis remoto que pueda no estar disponible durante el desarrollo local.
if (builder.Environment.IsDevelopment())
{
    builder.Services.AddDistributedMemoryCache();
}
else if (!string.IsNullOrEmpty(redisConnection) && !redisConnection.Contains("#{"))
{
    // Si la cadena viene en formato URI (redis:// o rediss://), la parseamos al formato de StackExchange.Redis
    if (redisConnection.StartsWith("redis://", StringComparison.OrdinalIgnoreCase) || 
        redisConnection.StartsWith("rediss://", StringComparison.OrdinalIgnoreCase))
    {
        try
        {
            var uri = new Uri(redisConnection);
            var host = uri.Host;
            var port = uri.Port;
            var userInfo = uri.UserInfo;
            var password = userInfo.Contains(':') ? userInfo.Split(':')[1] : userInfo;
            var isSsl = redisConnection.StartsWith("rediss://", StringComparison.OrdinalIgnoreCase);
            
            redisConnection = $"{host}:{port},password={password},ssl={isSsl},abortConnect=false";
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error parseando la URL de Redis: {ex.Message}");
        }
    }
    else if (!redisConnection.Contains("abortConnect="))
    {
        // Añadir abortConnect=false si no está presente en la cadena directa
        redisConnection = redisConnection.Contains(",") 
            ? $"{redisConnection},abortConnect=false" 
            : $"{redisConnection}:6379,abortConnect=false";
    }

    builder.Services.AddStackExchangeRedisCache(options =>
    {
        options.Configuration = redisConnection;
        options.InstanceName = "EcoRuta_";
    });
}
else
{
    builder.Services.AddDistributedMemoryCache();
}

builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(30);
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
    options.Cookie.Name = ".EcoRuta.Session";
});

var app = builder.Build();

// 🔥 ENTRENAR MODELO ML.NET (SOLO SI NO EXISTE)
string mlnetPath = System.IO.Path.Combine("ML", "SentimentAnalysis", "SentimentAnalysis.mlnet");
if (!System.IO.File.Exists(mlnetPath))
{
    try { Proyecto_Grupo_gris.ML.SentimentAnalysis.SentimentAnalysisTraining.TrainModel(); } 
    catch (Exception e) { Console.WriteLine("Error entrenando ML: " + e.Message); }
}
app.UseForwardedHeaders(new ForwardedHeadersOptions
{
    ForwardedHeaders = ForwardedHeaders.XForwardedFor |
                       ForwardedHeaders.XForwardedProto
});

// 🔥 APLICAR MIGRACIONES AUTOMÁTICAMENTE EN ARRANQUE
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
    var userManager = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();
    var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();

    try
    {
        db.Database.Migrate();
    }
    catch (Exception ex)
    {
        var logger = scope.ServiceProvider.GetRequiredService<ILogger<Program>>();
        logger.LogError(ex, "Ocurrió un error al migrar la base de datos. Si estás en local, verifica que PostgreSQL esté en ejecución.");
    }

    try
    {
        await IdentityDataSeeder.SeedRolesAsync(roleManager);
        await IdentityDataSeeder.SeedAdminUserAsync(userManager, roleManager, configuration);
    }
    catch (Exception ex)
    {
        var logger = scope.ServiceProvider.GetRequiredService<ILogger<Program>>();
        logger.LogError(ex, "Error seeding identity data.");
    }
}

// 🔥 PIPELINE
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(options =>
    {
        options.SwaggerEndpoint("/swagger/v1/swagger.json", "EcoRut API v1");
        options.RoutePrefix = "swagger";
    });
    app.UseMigrationsEndPoint();
}
else
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();
app.UseSession();

app.MapStaticAssets();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}")
    .WithStaticAssets();

app.MapRazorPages()
   .WithStaticAssets();

app.MapControllers();

await app.RunAsync();

