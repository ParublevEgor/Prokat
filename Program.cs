using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.FileProviders;
using Microsoft.IdentityModel.Tokens;
using Prokat.API.Data;
using Prokat.API.Services;
using System.Reflection;
using System.Text;
using System.Text.Json.Serialization;

static string? FindWwwRootDirectory()
{
    static string? TryFrom(string? startPath)
    {
        if (string.IsNullOrEmpty(startPath)) return null;
        var dir = new DirectoryInfo(startPath);
        for (var i = 0; i < 8 && dir != null; i++)
        {
            var candidate = Path.Combine(dir.FullName, "wwwroot");
            if (Directory.Exists(candidate) && File.Exists(Path.Combine(candidate, "index.html")))
                return candidate;
            dir = dir.Parent;
        }
        return null;
    }

    return TryFrom(AppContext.BaseDirectory)
        ?? TryFrom(Directory.GetCurrentDirectory())
        ?? TryFrom(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location));
}

var wwwRoot = FindWwwRootDirectory();

var builder = WebApplication.CreateBuilder(new WebApplicationOptions
{
    Args = args,
    WebRootPath = wwwRoot ?? Path.Combine(Directory.GetCurrentDirectory(), "wwwroot"),
});

var jwtSettings = builder.Configuration.GetSection("Jwt");
var jwtKeyStr = jwtSettings["Key"] ?? "";
if (Encoding.UTF8.GetByteCount(jwtKeyStr) < 32)
    throw new InvalidOperationException("В appsettings задайте Jwt:Key длиной не менее 32 байт в UTF-8 (секрет для подписи HS256).");
var key = Encoding.UTF8.GetBytes(jwtKeyStr);

builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.AddScoped<IOrderPricingService, OrderPricingService>();
builder.Services.AddScoped<IInventoryAvailabilityService, InventoryAvailabilityService>();
builder.Services.AddScoped<IRentalBookingService, RentalBookingService>();
builder.Services.AddScoped<IClientReportService, ClientReportService>();
builder.Services.AddScoped<ISiteSettingsService, SiteSettingsService>();
builder.Services.AddSingleton<IJwtTokenService, JwtTokenService>();

builder.Services.AddControllers()
    .AddApplicationPart(typeof(Program).Assembly)
    .AddJsonOptions(o =>
    {
        o.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
    });

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new Microsoft.OpenApi.Models.OpenApiInfo { Title = "Прокат API", Version = "v1" });
    c.AddSecurityDefinition("Bearer", new Microsoft.OpenApi.Models.OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = Microsoft.OpenApi.Models.SecuritySchemeType.Http,
        Scheme = "bearer",
        BearerFormat = "JWT",
        In = Microsoft.OpenApi.Models.ParameterLocation.Header,
        Description = "JWT: Bearer {token}",
    });
    c.AddSecurityRequirement(new Microsoft.OpenApi.Models.OpenApiSecurityRequirement
    {
        {
            new Microsoft.OpenApi.Models.OpenApiSecurityScheme
            {
                Reference = new Microsoft.OpenApi.Models.OpenApiReference
                {
                    Type = Microsoft.OpenApi.Models.ReferenceType.SecurityScheme,
                    Id = "Bearer",
                },
            },
            Array.Empty<string>()
        },
    });
});

builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(policy =>
    {
        policy.SetIsOriginAllowed(_ => true)
              .AllowAnyHeader()
              .AllowAnyMethod();
    });
});

builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.RequireHttpsMetadata = false;
    options.SaveToken = true;
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidateIssuerSigningKey = true,
        ValidateLifetime = true,
        RequireExpirationTime = true,
        ClockSkew = TimeSpan.FromMinutes(1),
        ValidIssuer = jwtSettings["Issuer"],
        ValidAudience = jwtSettings["Audience"],
        IssuerSigningKey = new SymmetricSecurityKey(key),
        RoleClaimType = System.Security.Claims.ClaimTypes.Role,
        NameClaimType = System.Security.Claims.ClaimTypes.Name,
    };
});

var app = builder.Build();

try
{
    using var scope = app.Services.CreateScope();
    var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
    db.Database.Migrate();
    Prokat.API.Data.DbSeed.EnsureDefaults(db);
}
catch (Exception ex) when (app.Environment.IsDevelopment())
{
    Console.WriteLine($"Миграция БД: {ex.Message}");
}

app.UseSwagger();
app.UseSwaggerUI();

PhysicalFileProvider? uiProvider = null;
if (wwwRoot != null && Directory.Exists(wwwRoot))
{
    uiProvider = new PhysicalFileProvider(wwwRoot);
    var staticOpts = new StaticFileOptions { FileProvider = uiProvider };
    app.UseDefaultFiles(new DefaultFilesOptions { FileProvider = uiProvider });
    app.UseStaticFiles(staticOpts);
    Console.WriteLine($"Статика UI: {wwwRoot}");
}
else
{
    app.UseDefaultFiles();
    app.UseStaticFiles();
    Console.WriteLine("Предупреждение: wwwroot не найден. Выполните dotnet build в папке проекта (файлы должны копироваться в bin).");
}

app.UseAuthentication();
app.UseAuthorization();

app.UseCors();

app.MapControllers();

if (uiProvider != null)
    app.MapFallbackToFile("index.html", new StaticFileOptions { FileProvider = uiProvider });
else
    app.MapFallbackToFile("index.html");

app.Run();
