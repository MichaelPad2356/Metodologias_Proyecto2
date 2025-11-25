# Sistema de Gestión de Proyectos OpenUP

Sistema web para gestionar proyectos siguiendo la metodología OpenUP (Open Unified Process), desarrollado con Angular y .NET Core.

## 📋 Descripción

Este sistema permite gestionar proyectos de desarrollo de software siguiendo las cuatro fases de OpenUP:
- **Incepción**: Definición inicial del proyecto
- **Elaboración**: Arquitectura y planificación detallada
- **Construcción**: Desarrollo e implementación
- **Transición**: Entrega y despliegue

## 🎯 Historias de Usuario Implementadas

### HU-001: Crear proyecto OpenUP
✅ **Implementado completamente**

**Criterios de aceptación cumplidos:**
- ✅ Opción "Nuevo proyecto" con formulario para nombre, identificador (código) y fecha de inicio
- ✅ Generación automática de las 4 fases de OpenUP al crear el proyecto
- ✅ Proyecto listado en panel con estado "Creado"
- ✅ Metadatos: responsable, descripción, tags

**Características técnicas:**
- Validación en frontend y backend
- Código único del proyecto (índice único en BD)
- Auditoría automática de creación
- Manejo profesional de errores

### HU-002: Eliminar/archivar proyecto
✅ **Implementado completamente**

**Criterios de aceptación cumplidos:**
- ✅ Opción "Archivar proyecto" en vista de lista y detalle
- ✅ Archivar conserva historial completo (soft delete)
- ✅ Opción "Eliminar proyecto" con confirmación doble
- ✅ Eliminación requiere confirmación explícita del usuario
- ✅ Auditoría completa de acciones (tabla `AuditLogs`)

**Características técnicas:**
- Archivar: actualiza estado a `Archived` y fecha de archivo
- Eliminar: borrado físico con confirmación de seguridad
- Sistema de auditoría registra usuario, fecha y detalles
- Permisos preparados para implementación futura

## 🏗️ Arquitectura

### Backend (.NET 9 + Entity Framework Core)

```
backend/
├── Controllers/         # API REST Controllers
│   └── ProjectsController.cs
├── Data/               # DbContext y configuración de BD
│   └── ApplicationDbContext.cs
├── Models/             # Entidades del dominio
│   ├── Project.cs
│   ├── ProjectPhase.cs
│   └── AuditLog.cs
├── Contracts/          # DTOs (Data Transfer Objects)
│   └── ProjectDtos.cs
├── Services/           # Lógica de negocio
│   ├── IProjectService.cs
│   ├── ProjectService.cs
│   ├── IAuditService.cs
│   └── AuditService.cs
└── Program.cs          # Configuración de la aplicación
```

**Patrón arquitectónico:** Clean Architecture con capas separadas
- **API Layer**: Controllers exponen endpoints REST
- **Service Layer**: Lógica de negocio encapsulada
- **Data Layer**: Persistencia con EF Core
- **Domain Layer**: Modelos y contratos

### Frontend (Angular 19 standalone)

```
frontend/src/app/
├── components/
│   ├── project-list/          # Lista de proyectos
│   ├── project-create/        # Formulario de creación
│   └── project-detail/        # Detalle y acciones
├── services/
│   └── project.service.ts     # Cliente HTTP API
├── models/
│   └── project.model.ts       # Interfaces TypeScript
├── app.routes.ts              # Configuración de rutas
└── app.config.ts              # Configuración global
```

**Características Angular:**
- Componentes standalone (sin módulos)
- Reactive programming con RxJS
- Rutas SPA con Angular Router
- Formularios template-driven y validación
- SCSS para estilos componentizados

### Base de Datos (SQLite)

**Tablas principales:**

```sql
Projects
├── Id (PK)
├── Name
├── Code (UNIQUE)
├── StartDate
├── Description
├── ResponsiblePerson
├── Tags
├── Status (Created, Active, Archived, Closed)
├── CreatedAt
└── ArchivedAt

ProjectPhases
├── Id (PK)
├── ProjectId (FK → Projects)
├── Name (Incepción, Elaboración, Construcción, Transición)
├── Order
└── Status (NotStarted, InProgress, Completed)

AuditLogs
├── Id (PK)
├── ProjectId (FK → Projects)
├── Action (CreateProject, ArchiveProject, DeleteProject, etc.)
├── EntityType
├── EntityId
├── UserName
├── Details
└── Timestamp
```

## 🚀 Instalación y Ejecución

### Prerrequisitos

- **.NET 9 SDK**: [Descargar aquí](https://dotnet.microsoft.com/download/dotnet/9.0)
- **Node.js 18+**: [Descargar aquí](https://nodejs.org/)

### Backend

```powershell
cd backend
dotnet restore
dotnet build
dotnet run
```

El backend estará disponible en: `http://localhost:5277`

La base de datos SQLite (`openup.db`) se crea automáticamente en la primera ejecución.

### Frontend

```powershell
cd frontend
npm install
npm start
```

El frontend estará disponible en: `http://localhost:4200`

### Ejecutar ambos simultáneamente

**Terminal 1 (Backend):**
```powershell
cd backend
dotnet run
```

**Terminal 2 (Frontend):**
```powershell
cd frontend
npm start
```

## 📡 API Endpoints

### Proyectos

| Método | Endpoint | Descripción |
|--------|----------|-------------|
| GET | `/api/projects` | Lista todos los proyectos |
| GET | `/api/projects?includeArchived=true` | Incluye proyectos archivados |
| GET | `/api/projects/{id}` | Obtiene proyecto por ID |
| GET | `/api/projects/by-code/{code}` | Obtiene proyecto por código |
| POST | `/api/projects` | Crea nuevo proyecto |
| PUT | `/api/projects/{id}` | Actualiza proyecto |
| POST | `/api/projects/{id}/archive` | Archiva proyecto |
| DELETE | `/api/projects/{id}` | Elimina proyecto permanentemente |

### Ejemplo: Crear proyecto

```http
POST /api/projects
Content-Type: application/json

{
  "name": "Sistema de Inventario",
  "code": "PROJ-2025-001",
  "startDate": "2025-01-15",
  "description": "Sistema web para gestión de inventario",
  "responsiblePerson": "Juan Pérez",
  "tags": "backend, frontend, web"
}
```

**Respuesta:**
```json
{
  "id": 1,
  "name": "Sistema de Inventario",
  "code": "PROJ-2025-001",
  "startDate": "2025-01-15T00:00:00",
  "description": "Sistema web para gestión de inventario",
  "responsiblePerson": "Juan Pérez",
  "tags": "backend, frontend, web",
  "status": "Created",
  "createdAt": "2025-11-24T20:30:00Z",
  "phases": [
    { "id": 1, "name": "Incepción", "order": 1, "status": "NotStarted" },
    { "id": 2, "name": "Elaboración", "order": 2, "status": "NotStarted" },
    { "id": 3, "name": "Construcción", "order": 3, "status": "NotStarted" },
    { "id": 4, "name": "Transición", "order": 4, "status": "NotStarted" }
  ]
}
```

## 🧪 Testing

### Backend
```powershell
cd backend
dotnet test
```

### Frontend
```powershell
cd frontend
npm test
```

## 📦 Build para Producción

### Backend
```powershell
cd backend
dotnet publish -c Release -o ./publish
```

### Frontend
```powershell
cd frontend
npm run build
```

Los archivos compilados quedarán en `frontend/dist/frontend`

## 🔧 Configuración

### Backend: appsettings.json

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Data Source=openup.db"
  },
  "Logging": {
    "LogLevel": {
      "Default": "Information"
    }
  }
}
```

### Frontend: proxy.conf.json

```json
{
  "/api": {
    "target": "http://localhost:5277",
    "secure": false,
    "changeOrigin": true
  }
}
```

## 🎨 Características de UI/UX

- **Diseño responsive**: Funciona en desktop, tablet y móvil
- **Feedback visual**: Estados de carga, errores y éxitos
- **Confirmaciones**: Diálogos de confirmación para acciones destructivas
- **Badges de estado**: Visualización clara del estado de proyectos y fases
- **Cards interactivas**: Hover effects y navegación intuitiva
- **Formularios validados**: Validación en tiempo real

## 🔐 Seguridad (Preparado para Sprint 2)

- Estructura lista para JWT authentication
- Matriz de permisos por rol definida en servicio
- Auditoría completa de acciones
- Validación de entrada en ambos lados (frontend y backend)

## 🛠️ Tecnologías Utilizadas

### Backend
- .NET 9
- Entity Framework Core 9
- SQLite
- ASP.NET Core Web API
- Dependency Injection
- Async/Await pattern

### Frontend
- Angular 19
- TypeScript
- RxJS
- SCSS
- Angular Router
- Standalone Components

## 📝 Próximos Pasos (Sprint 2 y 3)

- [ ] HU-003: Definir plan del proyecto
- [ ] HU-004: Seguimiento del plan
- [ ] HU-005: Registrar microincrementos
- [ ] HU-006-009: Artefactos por fase
- [ ] HU-010: Control de versiones de entregables
- [ ] HU-015: Gestión de iteraciones
- [ ] Sistema de autenticación y autorización
- [ ] Exportación de reportes
- [ ] Notificaciones

## 👥 Equipo de Desarrollo

Proyecto desarrollado para la materia de Metodologías de Desarrollo de Sistemas.

## 📄 Licencia

Proyecto académico - Universidad.
