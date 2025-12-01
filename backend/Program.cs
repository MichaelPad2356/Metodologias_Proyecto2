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
builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        // Configurar JSON para usar camelCase (compatible con frontend)
        options.JsonSerializerOptions.PropertyNamingPolicy = System.Text.Json.JsonNamingPolicy.CamelCase;
        options.JsonSerializerOptions.PropertyNameCaseInsensitive = true;
        // Serializar enums como strings
        options.JsonSerializerOptions.Converters.Add(new System.Text.Json.Serialization.JsonStringEnumConverter());
    });
builder.Services.AddOpenApi();


var app = builder.Build();

// Crear/actualizar la base de datos automáticamente en desarrollo
if (app.Environment.IsDevelopment())
{
    using var scope = app.Services.CreateScope();
    var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
    
    // Asegurarse de que la base de datos y tablas existan
    context.Database.EnsureCreated();
    
    // Función helper para verificar si una columna existe
    bool ColumnExists(string tableName, string columnName)
    {
        var result = context.Database.SqlQueryRaw<int>(
            $"SELECT COUNT(*) as Value FROM pragma_table_info('{tableName}') WHERE name = '{columnName}'"
        ).ToList();
        return result.Count > 0 && result[0] > 0;
    }
    
    // HU-009: Agregar columnas de transición si no existen
    if (!ColumnExists("Artifacts", "BuildIdentifier"))
    {
        context.Database.ExecuteSqlRaw("ALTER TABLE Artifacts ADD COLUMN BuildIdentifier TEXT NULL;");
        Console.WriteLine("✓ Columna BuildIdentifier agregada");
    }
    
    if (!ColumnExists("Artifacts", "BuildDownloadUrl"))
    {
        context.Database.ExecuteSqlRaw("ALTER TABLE Artifacts ADD COLUMN BuildDownloadUrl TEXT NULL;");
        Console.WriteLine("✓ Columna BuildDownloadUrl agregada");
    }
    
    if (!ColumnExists("Artifacts", "ClosureChecklistJson"))
    {
        context.Database.ExecuteSqlRaw("ALTER TABLE Artifacts ADD COLUMN ClosureChecklistJson TEXT NULL;");
        Console.WriteLine("✓ Columna ClosureChecklistJson agregada");
    }
    
    // HU-010: Agregar columnas de control de versiones
    if (!ColumnExists("ArtifactVersions", "Observations"))
    {
        context.Database.ExecuteSqlRaw("ALTER TABLE ArtifactVersions ADD COLUMN Observations TEXT NULL;");
        Console.WriteLine("✓ Columna Observations agregada");
    }
    
    if (!ColumnExists("ArtifactVersions", "FileSize"))
    {
        context.Database.ExecuteSqlRaw("ALTER TABLE ArtifactVersions ADD COLUMN FileSize INTEGER NULL;");
        Console.WriteLine("✓ Columna FileSize agregada");
    }
    
    // HU-014: Crear tabla Defects si no existe
    context.Database.ExecuteSqlRaw(@"
        CREATE TABLE IF NOT EXISTS Defects (
            Id INTEGER PRIMARY KEY AUTOINCREMENT,
            Title TEXT NOT NULL,
            Description TEXT,
            Severity TEXT NOT NULL,
            Status TEXT NOT NULL,
            ProjectId INTEGER NOT NULL,
            ArtifactId INTEGER,
            ReportedBy TEXT,
            AssignedTo TEXT,
            CreatedAt TEXT NOT NULL,
            UpdatedAt TEXT,
            FOREIGN KEY (ProjectId) REFERENCES Projects(Id),
            FOREIGN KEY (ArtifactId) REFERENCES Artifacts(Id)
        );
    ");
    
    // HU-014: Crear tabla Defects si no existe
    try
    {
        context.Database.ExecuteSqlRaw(@"
            CREATE TABLE IF NOT EXISTS Defects (
                Id INTEGER PRIMARY KEY AUTOINCREMENT,
                Title TEXT NOT NULL,
                Description TEXT,
                Severity TEXT NOT NULL,
                Status TEXT NOT NULL,
                ProjectId INTEGER NOT NULL,
                ArtifactId INTEGER,
                ReportedBy TEXT,
                AssignedTo TEXT,
                CreatedAt TEXT NOT NULL,
                UpdatedAt TEXT,
                FOREIGN KEY (ProjectId) REFERENCES Projects(Id),
                FOREIGN KEY (ArtifactId) REFERENCES Artifacts(Id)
            );
        ");
    }
    catch { /* La tabla ya existe */ }
    
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
