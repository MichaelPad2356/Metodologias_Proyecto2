using backend.Data;
using backend.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.FileProviders;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using System.IO;

var builder = WebApplication.CreateBuilder(args);

// Configurar Entity Framework Core con SQLite
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlite(builder.Configuration.GetConnectionString("DefaultConnection") 
        ?? "Data Source=openup.db"));

// Configurar autenticación JWT
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(
                builder.Configuration["Jwt:Key"] ?? "OpenUpSecretKey2024SuperSecureKeyForJWT!")),
            ValidateIssuer = true,
            ValidIssuer = builder.Configuration["Jwt:Issuer"] ?? "OpenUpApp",
            ValidateAudience = true,
            ValidAudience = builder.Configuration["Jwt:Audience"] ?? "OpenUpUsers",
            ValidateLifetime = true,
            ClockSkew = TimeSpan.Zero
        };
    });

// Registrar servicios de aplicación
builder.Services.AddScoped<IProjectService, ProjectService>();
builder.Services.AddScoped<IAuditService, AuditService>();
builder.Services.AddScoped<IPdfExportService, PdfExportService>();
builder.Services.AddScoped<IPlanVersionService, PlanVersionService>();
builder.Services.AddScoped<IIterationService, IterationService>();
builder.Services.AddScoped<IMicroincrementService, MicroincrementService>();
builder.Services.AddScoped<IDefectService, DefectService>();

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
    
    // HU-015: Crear tabla Iteraciones si no existe
    try
    {
        context.Database.ExecuteSqlRaw(@"
            CREATE TABLE IF NOT EXISTS Iteraciones (
                Id INTEGER PRIMARY KEY AUTOINCREMENT,
                Nombre TEXT NOT NULL,
                FechaInicio TEXT NOT NULL,
                FechaFin TEXT NOT NULL,
                Objetivo TEXT,
                FaseOpenUP TEXT,
                ProjectId INTEGER,
                CapacidadEquipoHoras REAL DEFAULT 0,
                PuntosCompletados INTEGER DEFAULT 0,
                PuntosEstimados INTEGER DEFAULT 0,
                TareasJson TEXT DEFAULT '[]',
                FOREIGN KEY (ProjectId) REFERENCES Projects(Id)
            );
        ");
    }
    catch { /* La tabla ya existe */ }

    // HU-012: Crear tablas de Workflows si no existen
    try
    {
        context.Database.ExecuteSqlRaw(@"
            CREATE TABLE IF NOT EXISTS Workflows (
                Id INTEGER PRIMARY KEY AUTOINCREMENT,
                Name TEXT NOT NULL,
                Description TEXT NOT NULL
            );
        ");

        context.Database.ExecuteSqlRaw(@"
            CREATE TABLE IF NOT EXISTS WorkflowSteps (
                Id INTEGER PRIMARY KEY AUTOINCREMENT,
                Name TEXT NOT NULL,
                ""Order"" INTEGER NOT NULL,
                WorkflowId INTEGER NOT NULL,
                FOREIGN KEY (WorkflowId) REFERENCES Workflows(Id) ON DELETE CASCADE
            );
        ");
    }
    catch { /* Tablas ya existen */ }

    // HU-012: Agregar columnas de Workflow a Artifacts
    if (!ColumnExists("Artifacts", "WorkflowId"))
    {
        context.Database.ExecuteSqlRaw("ALTER TABLE Artifacts ADD COLUMN WorkflowId INTEGER NULL REFERENCES Workflows(Id);");
        Console.WriteLine("✓ Columna WorkflowId agregada");
    }

    if (!ColumnExists("Artifacts", "CurrentStepId"))
    {
        context.Database.ExecuteSqlRaw("ALTER TABLE Artifacts ADD COLUMN CurrentStepId INTEGER NULL REFERENCES WorkflowSteps(Id);");
        Console.WriteLine("✓ Columna CurrentStepId agregada");
    }

    // Seed Workflows si está vacío
    if (!context.Workflows.Any())
    {
        context.Database.ExecuteSqlRaw("INSERT INTO Workflows (Name, Description) VALUES ('Flujo de Aprobación Estándar', 'Flujo básico de revisión y aprobación');");
        var workflowId = context.Database.SqlQueryRaw<int>("SELECT last_insert_rowid()").AsEnumerable().FirstOrDefault();
        
        if (workflowId > 0)
        {
            context.Database.ExecuteSqlRaw($"INSERT INTO WorkflowSteps (Name, \"Order\", WorkflowId) VALUES ('Borrador', 0, {workflowId});");
            context.Database.ExecuteSqlRaw($"INSERT INTO WorkflowSteps (Name, \"Order\", WorkflowId) VALUES ('Revisión Técnica', 1, {workflowId});");
            context.Database.ExecuteSqlRaw($"INSERT INTO WorkflowSteps (Name, \"Order\", WorkflowId) VALUES ('Aprobación Final', 2, {workflowId});");
            Console.WriteLine("✓ Workflow por defecto creado");
        }
    }

    // HU-018: Crear tablas de configuración del sistema
    try
    {
        context.Database.ExecuteSqlRaw(@"
            CREATE TABLE IF NOT EXISTS SystemRoles (
                Id INTEGER PRIMARY KEY AUTOINCREMENT,
                Name TEXT NOT NULL UNIQUE,
                Description TEXT,
                PermissionsJson TEXT,
                IsSystem INTEGER DEFAULT 0,
                CreatedAt TEXT NOT NULL,
                UpdatedAt TEXT
            );
        ");

        context.Database.ExecuteSqlRaw(@"
            CREATE TABLE IF NOT EXISTS CustomArtifactTypes (
                Id INTEGER PRIMARY KEY AUTOINCREMENT,
                Name TEXT NOT NULL,
                Description TEXT,
                DefaultPhase TEXT,
                IsMandatoryByDefault INTEGER DEFAULT 0,
                CustomFieldsJson TEXT,
                IsActive INTEGER DEFAULT 1,
                CreatedAt TEXT NOT NULL
            );
        ");

        context.Database.ExecuteSqlRaw(@"
            CREATE TABLE IF NOT EXISTS CustomPhaseDefinitions (
                Id INTEGER PRIMARY KEY AUTOINCREMENT,
                Name TEXT NOT NULL,
                Description TEXT,
                ""Order"" INTEGER NOT NULL,
                MandatoryArtifactTypesJson TEXT,
                IsActive INTEGER DEFAULT 1,
                CreatedAt TEXT NOT NULL
            );
        ");

        context.Database.ExecuteSqlRaw(@"
            CREATE TABLE IF NOT EXISTS ConfigurationHistory (
                Id INTEGER PRIMARY KEY AUTOINCREMENT,
                EntityType TEXT NOT NULL,
                EntityId INTEGER NOT NULL,
                Action TEXT NOT NULL,
                OldValuesJson TEXT,
                NewValuesJson TEXT,
                ChangedBy TEXT,
                ChangedAt TEXT NOT NULL
            );
        ");
        Console.WriteLine("✓ Tablas HU-018 creadas");
    }
    catch { /* Tablas ya existen */ }

    // HU-019: Crear tabla de plantillas OpenUP
    try
    {
        context.Database.ExecuteSqlRaw(@"
            CREATE TABLE IF NOT EXISTS OpenUpTemplates (
                Id INTEGER PRIMARY KEY AUTOINCREMENT,
                Name TEXT NOT NULL,
                Description TEXT,
                Version INTEGER DEFAULT 1,
                IsDefault INTEGER DEFAULT 0,
                IsActive INTEGER DEFAULT 1,
                PhasesJson TEXT DEFAULT '[]',
                RolesJson TEXT DEFAULT '[]',
                ArtifactTypesJson TEXT DEFAULT '[]',
                WorkflowsJson TEXT DEFAULT '[]',
                CreatedBy TEXT,
                CreatedAt TEXT NOT NULL,
                UpdatedAt TEXT,
                ParentTemplateId INTEGER,
                FOREIGN KEY (ParentTemplateId) REFERENCES OpenUpTemplates(Id)
            );
        ");
        Console.WriteLine("✓ Tabla HU-019 creada");
    }
    catch { /* Tabla ya existe */ }

    // HU-025: Crear tabla de miembros del proyecto
    try
    {
        context.Database.ExecuteSqlRaw(@"
            CREATE TABLE IF NOT EXISTS ProjectMembers (
                Id INTEGER PRIMARY KEY AUTOINCREMENT,
                ProjectId INTEGER NOT NULL,
                UserEmail TEXT NOT NULL,
                UserName TEXT,
                Role TEXT NOT NULL,
                Status TEXT NOT NULL,
                InvitedAt TEXT NOT NULL,
                AcceptedAt TEXT,
                InvitedBy TEXT,
                FOREIGN KEY (ProjectId) REFERENCES Projects(Id)
            );
        ");
        Console.WriteLine("✓ Tabla HU-025 creada");
    }
    catch { /* Tabla ya existe */ }

    // HU-020: Crear tabla de movimientos de entregables
    try
    {
        context.Database.ExecuteSqlRaw(@"
            CREATE TABLE IF NOT EXISTS DeliverableMovements (
                Id INTEGER PRIMARY KEY AUTOINCREMENT,
                DeliverableId INTEGER NOT NULL,
                FromPhaseId INTEGER NOT NULL,
                ToPhaseId INTEGER NOT NULL,
                Reason TEXT,
                MovedBy TEXT,
                MovedAt TEXT NOT NULL,
                RequiredConfirmation INTEGER DEFAULT 0,
                WarningsJson TEXT,
                FOREIGN KEY (DeliverableId) REFERENCES Deliverables(Id)
            );
        ");
        Console.WriteLine("✓ Tabla HU-020 creada");
    }
    catch { /* Tabla ya existe */ }

    // HU-026: Crear tabla de cierres de proyecto
    try
    {
        context.Database.ExecuteSqlRaw(@"
            CREATE TABLE IF NOT EXISTS ProjectClosures (
                Id INTEGER PRIMARY KEY AUTOINCREMENT,
                ProjectId INTEGER NOT NULL,
                ClosedAt TEXT NOT NULL,
                ClosedBy TEXT,
                IsForcedClose INTEGER DEFAULT 0,
                ForceCloseJustification TEXT,
                ValidationResultJson TEXT,
                ArtifactsSummaryJson TEXT,
                TeamMembersJson TEXT,
                ClosureDocumentPath TEXT,
                FOREIGN KEY (ProjectId) REFERENCES Projects(Id)
            );
        ");
        Console.WriteLine("✓ Tabla HU-026 creada");
    }
    catch { /* Tabla ya existe */ }

    // Crear tabla Users para autenticación
    try
    {
        context.Database.ExecuteSqlRaw(@"
            CREATE TABLE IF NOT EXISTS Users (
                Id INTEGER PRIMARY KEY AUTOINCREMENT,
                Name TEXT NOT NULL,
                Email TEXT NOT NULL UNIQUE,
                PasswordHash TEXT NOT NULL,
                Role TEXT DEFAULT 'Usuario',
                IsActive INTEGER DEFAULT 1,
                CreatedAt TEXT NOT NULL,
                LastLoginAt TEXT
            );
        ");
        Console.WriteLine("✓ Tabla Users creada");
    }
    catch { /* Tabla ya existe */ }

    // Agregar columna CreatedByEmail a Projects si no existe
    try
    {
        var hasCreatedByEmail = context.Database.SqlQueryRaw<int>(
            "SELECT COUNT(*) as Value FROM pragma_table_info('Projects') WHERE name = 'CreatedByEmail'"
        ).First();
        
        if (hasCreatedByEmail == 0)
        {
            context.Database.ExecuteSqlRaw("ALTER TABLE Projects ADD COLUMN CreatedByEmail TEXT;");
            Console.WriteLine("✓ Columna CreatedByEmail agregada a Projects");
        }
    }
    catch { /* Columna ya existe */ }

    // Seed roles del sistema si está vacío
    if (!context.SystemRoles.Any())
    {
        var defaultRoles = new[]
        {
            ("Administrador", "Control total del sistema", true),
            ("Product Owner", "Responsable del producto y priorización", true),
            ("Scrum Master", "Facilitador del equipo", true),
            ("Desarrollador", "Miembro del equipo de desarrollo", true),
            ("Tester", "Responsable de pruebas", true),
            ("Revisor", "Responsable de revisiones", true)
        };

        foreach (var (name, desc, isSystem) in defaultRoles)
        {
            context.Database.ExecuteSqlRaw(
                $"INSERT INTO SystemRoles (Name, Description, IsSystem, CreatedAt) VALUES ('{name}', '{desc}', {(isSystem ? 1 : 0)}, '{DateTime.UtcNow:O}');");
        }
        Console.WriteLine("✓ Roles por defecto creados");
    }
    
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
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();

app.Run();
