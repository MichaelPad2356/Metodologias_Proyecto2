using backend.Data;
using backend.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.FileProviders;
using System.IO;

var builder = WebApplication.CreateBuilder(args);

// Configurar Entity Framework Core con SQLite
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlite(builder.Configuration.GetConnectionString("DefaultConnection") 
        ?? "Data Source=openup.db"));

// Registrar servicios de aplicación
builder.Services.AddScoped<IProjectService, ProjectService>();
builder.Services.AddScoped<IAuditService, AuditService>();
builder.Services.AddScoped<IPdfExportService, PdfExportService>();
builder.Services.AddScoped<IPlanVersionService, PlanVersionService>();
builder.Services.AddScoped<IIterationService, IterationService>();
builder.Services.AddScoped<IMicroincrementService, MicroincrementService>();

// Configurar CORS para desarrollo
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowFrontend", builder =>
    {
        builder.WithOrigins("http://localhost:4200")
               .AllowAnyMethod()
               .AllowAnyHeader();
    });
});


// Add services to the container.
builder.Services.AddControllers();
builder.Services.AddOpenApi();


var app = builder.Build();

// Crear/actualizar la base de datos automáticamente en desarrollo
if (app.Environment.IsDevelopment())
{
    using var scope = app.Services.CreateScope();
    var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
    
    // Asegurarse de que la base de datos y tablas existan
    context.Database.EnsureCreated();
    
    app.MapOpenApi();
}

app.UseHttpsRedirection();

// >> CÓDIGO MODIFICADO <<
var uploadsPath = Path.Combine(builder.Environment.ContentRootPath, "uploads");
if (!Directory.Exists(uploadsPath))
{
    Directory.CreateDirectory(uploadsPath);
}

app.UseStaticFiles(new StaticFileOptions
{
    FileProvider = new PhysicalFileProvider(uploadsPath),
    RequestPath = "/uploads"
});
// >> FIN DE LA MODIFICACIÓN <<

app.UseCors("AllowFrontend");
app.UseRouting();
app.UseAuthorization();
app.MapControllers();

app.Run();

// Hacer la clase Program accesible para pruebas de integración
public partial class Program { }
