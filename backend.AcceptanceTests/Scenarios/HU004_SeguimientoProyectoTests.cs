using System.Net;
using System.Net.Http.Json;
using backend.AcceptanceTests.Fixtures;
using backend.AcceptanceTests.Helpers;
using backend.Contracts;
using FluentAssertions;
using Xunit;

namespace backend.AcceptanceTests.Scenarios;

/// <summary>
/// Pruebas de Aceptación para HU-004: Como miembro del equipo, quiero dar seguimiento al proyecto 
/// mediante iteraciones y microincrementos para monitorear el progreso
/// </summary>
public class HU004_SeguimientoProyectoTests : IClassFixture<AcceptanceTestWebApplicationFactory<Program>>
{
    private readonly HttpClient _client;
    private readonly AcceptanceTestWebApplicationFactory<Program> _factory;

    public HU004_SeguimientoProyectoTests(AcceptanceTestWebApplicationFactory<Program> factory)
    {
        _factory = factory;
        _client = factory.CreateClient();
    }

    [Fact(DisplayName = "AC1: Crear iteración para el proyecto con fechas de inicio y fin")]
    public async Task CriterioAceptacion1_CrearIteracion_DebeAsociarIteracionAlProyecto()
    {
        // Arrange - Crear un proyecto primero
        var projectDto = AcceptanceTestDataHelper.CreateRealisticProjectDto();
        var projectResponse = await _client.PostAsJsonAsync("/api/projects", projectDto);
        var project = await projectResponse.Content.ReadFromJsonAsync<ProjectDto>();

        // Act - Crear una iteración para el proyecto
        var iterationDto = AcceptanceTestDataHelper.CreateRealisticIterationDto();
        var response = await _client.PostAsJsonAsync($"/api/projects/{project!.Id}/iterations", iterationDto);

        // Assert - Verificar que se creó correctamente
        response.StatusCode.Should().Be(HttpStatusCode.Created, 
            "el sistema debe permitir crear iteraciones para dar seguimiento");

        var iteration = await response.Content.ReadFromJsonAsync<IterationDto>();
        iteration.Should().NotBeNull();
        iteration!.Name.Should().Be(iterationDto.Name);
        iteration.StartDate.Should().BeCloseTo(iterationDto.StartDate, TimeSpan.FromSeconds(1));
        iteration.EndDate.Should().BeCloseTo(iterationDto.EndDate, TimeSpan.FromSeconds(1));
        iteration.PercentageCompleted.Should().Be(0, "debe iniciar en 0%");
        iteration.ProjectId.Should().Be(project.Id, "debe estar asociada al proyecto");
    }

    [Fact(DisplayName = "AC2: Actualizar el progreso de la iteración con porcentaje de completitud")]
    public async Task CriterioAceptacion2_ActualizarProgresoIteracion_DebeReflejarPorcentajeYObservaciones()
    {
        // Arrange - Crear proyecto e iteración
        var projectDto = AcceptanceTestDataHelper.CreateRealisticProjectDto();
        var projectResponse = await _client.PostAsJsonAsync("/api/projects", projectDto);
        var project = await projectResponse.Content.ReadFromJsonAsync<ProjectDto>();

        var iterationDto = AcceptanceTestDataHelper.CreateRealisticIterationDto();
        var iterationResponse = await _client.PostAsJsonAsync($"/api/projects/{project!.Id}/iterations", iterationDto);
        var iteration = await iterationResponse.Content.ReadFromJsonAsync<IterationDto>();

        // Act - Actualizar el progreso de la iteración
        var updateDto = AcceptanceTestDataHelper.CreateIterationUpdateDto();
        var response = await _client.PutAsJsonAsync($"/api/iterations/{iteration!.Id}", updateDto);

        // Assert - Verificar que se actualizó el progreso
        response.StatusCode.Should().Be(HttpStatusCode.OK, 
            "el sistema debe permitir actualizar el progreso de la iteración");

        var updatedIteration = await response.Content.ReadFromJsonAsync<IterationDto>();
        updatedIteration.Should().NotBeNull();
        updatedIteration!.PercentageCompleted.Should().Be(75, 
            "el porcentaje debe reflejar el progreso actual");
        updatedIteration.Observations.Should().Contain("Se completó la configuración", 
            "las observaciones deben estar actualizadas");
    }

    [Fact(DisplayName = "AC3: Registrar microincrementos en las fases del proyecto")]
    public async Task CriterioAceptacion3_RegistrarMicroincrementos_DebeAsociarAFaseDelProyecto()
    {
        // Arrange - Crear proyecto y obtener sus fases
        var projectDto = AcceptanceTestDataHelper.CreateRealisticProjectDto();
        var projectResponse = await _client.PostAsJsonAsync("/api/projects", projectDto);
        var project = await projectResponse.Content.ReadFromJsonAsync<ProjectDto>();

        var phasesResponse = await _client.GetAsync($"/api/projects/{project!.Id}/phases");
        var phases = await phasesResponse.Content.ReadFromJsonAsync<List<ProjectPhaseDto>>();
        var inicioPhase = phases!.First(p => p.Name == "Inicio");

        // Act - Registrar un microincremento en la fase de Inicio
        var microDto = AcceptanceTestDataHelper.CreateRealisticMicroincrementDto(inicioPhase.Id);
        var response = await _client.PostAsJsonAsync($"/api/projects/{project.Id}/microincrements", microDto);

        // Assert - Verificar que se registró correctamente
        response.StatusCode.Should().Be(HttpStatusCode.Created, 
            "el sistema debe permitir registrar microincrementos");

        var microincrement = await response.Content.ReadFromJsonAsync<MicroincrementDto>();
        microincrement.Should().NotBeNull();
        microincrement!.Title.Should().Be(microDto.Title);
        microincrement.ProjectPhaseId.Should().Be(inicioPhase.Id, 
            "debe estar asociado a la fase correspondiente");
        microincrement.Author.Should().Be(microDto.Author);
        microincrement.Date.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(5), "debe tener fecha de registro");
    }

    [Fact(DisplayName = "AC4: Consultar todas las iteraciones del proyecto ordenadas por fecha")]
    public async Task CriterioAceptacion4_ConsultarIteraciones_DebeRetornarListaOrdenada()
    {
        // Arrange - Crear proyecto con múltiples iteraciones
        var projectDto = AcceptanceTestDataHelper.CreateRealisticProjectDto();
        var projectResponse = await _client.PostAsJsonAsync("/api/projects", projectDto);
        var project = await projectResponse.Content.ReadFromJsonAsync<ProjectDto>();

        // Crear 3 iteraciones
        for (int i = 1; i <= 3; i++)
        {
            var iterationDto = new CreateIterationDto(
                Name: $"Sprint {i}",
                StartDate: DateTime.UtcNow.AddDays(i * 14),
                EndDate: DateTime.UtcNow.AddDays((i + 1) * 14),
                Observations: $"Iteración {i}"
            );
            await _client.PostAsJsonAsync($"/api/projects/{project!.Id}/iterations", iterationDto);
        }

        // Act - Consultar las iteraciones del proyecto
        var response = await _client.GetAsync($"/api/projects/{project!.Id}/iterations");

        // Assert - Verificar que retorna todas las iteraciones
        response.StatusCode.Should().Be(HttpStatusCode.OK, 
            "el equipo debe poder consultar todas las iteraciones");

        var iterations = await response.Content.ReadFromJsonAsync<List<IterationDto>>();
        iterations.Should().NotBeNull().And.HaveCount(3, 
            "debe retornar las 3 iteraciones creadas");
        iterations!.Should().BeInAscendingOrder(i => i.StartDate, 
            "deben estar ordenadas por fecha de inicio");
    }

    [Fact(DisplayName = "AC5: Consultar microincrementos por fase para ver el progreso")]
    public async Task CriterioAceptacion5_ConsultarMicroincrementosPorFase_DebeRetornarSoloDeEsaFase()
    {
        // Arrange - Crear proyecto y registrar microincrementos en diferentes fases
        var projectDto = AcceptanceTestDataHelper.CreateRealisticProjectDto();
        var projectResponse = await _client.PostAsJsonAsync("/api/projects", projectDto);
        var project = await projectResponse.Content.ReadFromJsonAsync<ProjectDto>();

        var phasesResponse = await _client.GetAsync($"/api/projects/{project!.Id}/phases");
        var phases = await phasesResponse.Content.ReadFromJsonAsync<List<ProjectPhaseDto>>();
        
        var inicioPhase = phases!.First(p => p.Name == "Inicio");
        var elaboracionPhase = phases!.First(p => p.Name == "Elaboración");

        // Registrar 2 microincrementos en Inicio y 1 en Elaboración
        for (int i = 0; i < 2; i++)
        {
            var microDto = new CreateMicroincrementDto
            {
                ProjectPhaseId = inicioPhase.Id,
                Title = $"Actividad {i + 1} de Inicio",
                Description = $"Descripción {i + 1}",
                Author = "Juan Pérez"
            };
            await _client.PostAsJsonAsync($"/api/projects/{project.Id}/microincrements", microDto);
        }

        var microElaboracion = new CreateMicroincrementDto
        {
            ProjectPhaseId = elaboracionPhase.Id,
            Title = "Actividad de Elaboración",
            Description = "Descripción elaboración",
            Author = "María González"
        };
        await _client.PostAsJsonAsync($"/api/projects/{project.Id}/microincrements", microElaboracion);

        // Act - Filtrar microincrementos por fase de Inicio
        var response = await _client.GetAsync($"/api/projects/{project.Id}/microincrements?phaseId={inicioPhase.Id}");

        // Assert - Verificar que solo retorna los de la fase solicitada
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var microincrements = await response.Content.ReadFromJsonAsync<List<MicroincrementDto>>();
        microincrements.Should().NotBeNull().And.HaveCount(2, 
            "debe retornar solo los microincrementos de la fase Inicio");
        microincrements!.Should().OnlyContain(m => m.ProjectPhaseId == inicioPhase.Id);
    }

    [Fact(DisplayName = "AC6: Agregar tareas a una iteración para detallar el trabajo")]
    public async Task CriterioAceptacion6_AgregarTareasAIteracion_DebeAsociarTareasCorrectamente()
    {
        // Arrange - Crear proyecto, iteración y obtener fases
        var projectDto = AcceptanceTestDataHelper.CreateRealisticProjectDto();
        var projectResponse = await _client.PostAsJsonAsync("/api/projects", projectDto);
        var project = await projectResponse.Content.ReadFromJsonAsync<ProjectDto>();

        var phasesResponse = await _client.GetAsync($"/api/projects/{project!.Id}/phases");
        var phases = await phasesResponse.Content.ReadFromJsonAsync<List<ProjectPhaseDto>>();
        var inicioPhase = phases!.First(p => p.Name == "Inicio");

        var iterationDto = AcceptanceTestDataHelper.CreateRealisticIterationDto();
        var iterationResponse = await _client.PostAsJsonAsync($"/api/projects/{project.Id}/iterations", iterationDto);
        var iteration = await iterationResponse.Content.ReadFromJsonAsync<IterationDto>();

        // Act - Agregar tarea a la iteración
        var taskDto = new CreateIterationTaskDto(
            Name: "Diseñar modelo de base de datos",
            Description: "Crear el diseño del modelo entidad-relación para el módulo de inventario",
            StartDate: DateTime.UtcNow,
            EndDate: DateTime.UtcNow.AddDays(3),
            AssignedTo: "Carlos Rodríguez",
            ProjectPhaseId: inicioPhase.Id
        );
        var response = await _client.PostAsJsonAsync($"/api/iterations/{iteration!.Id}/tasks", taskDto);

        // Assert - Verificar que se agregó la tarea
        response.StatusCode.Should().Be(HttpStatusCode.Created, 
            "el sistema debe permitir agregar tareas a las iteraciones");

        // Consultar la iteración para verificar que incluye la tarea
        var getIterationResponse = await _client.GetAsync($"/api/iterations/{iteration.Id}");
        var updatedIteration = await getIterationResponse.Content.ReadFromJsonAsync<IterationDto>();
        updatedIteration!.Tasks.Should().NotBeNull().And.HaveCount(1);
        updatedIteration.Tasks![0].Name.Should().Be(taskDto.Name);
    }

    [Fact(DisplayName = "Escenario Completo: Seguimiento completo de un proyecto con iteraciones y microincrementos")]
    public async Task EscenarioCompleto_SeguimientoProyecto_DebePermitirMonitoreoCompleto()
    {
        // Paso 1: Como equipo, creamos el proyecto
        var projectDto = AcceptanceTestDataHelper.CreateRealisticProjectDto();
        var projectResponse = await _client.PostAsJsonAsync("/api/projects", projectDto);
        var project = await projectResponse.Content.ReadFromJsonAsync<ProjectDto>();

        // Paso 2: Obtenemos las fases del proyecto
        var phasesResponse = await _client.GetAsync($"/api/projects/{project!.Id}/phases");
        var phases = await phasesResponse.Content.ReadFromJsonAsync<List<ProjectPhaseDto>>();
        var inicioPhase = phases!.First(p => p.Name == "Inicio");

        // Paso 3: Creamos la primera iteración (Sprint 1)
        var iteration1Dto = new CreateIterationDto(
            Name: "Sprint 1 - Configuración Inicial",
            StartDate: DateTime.UtcNow,
            EndDate: DateTime.UtcNow.AddDays(14),
            Observations: "Primera iteración del proyecto"
        );
        var iteration1Response = await _client.PostAsJsonAsync($"/api/projects/{project.Id}/iterations", iteration1Dto);
        var iteration1 = await iteration1Response.Content.ReadFromJsonAsync<IterationDto>();
        iteration1Response.StatusCode.Should().Be(HttpStatusCode.Created);

        // Paso 4: Agregamos tareas a la iteración
        var task1 = new CreateIterationTaskDto(
            Name: "Configurar repositorio Git",
            Description: "Crear repositorio y configurar estructura inicial",
            StartDate: DateTime.UtcNow,
            EndDate: DateTime.UtcNow.AddDays(2),
            AssignedTo: "Juan Pérez",
            ProjectPhaseId: inicioPhase.Id
        );
        await _client.PostAsJsonAsync($"/api/iterations/{iteration1!.Id}/tasks", task1);

        // Paso 5: Registramos microincrementos completados
        var micro1 = new CreateMicroincrementDto
        {
            ProjectPhaseId = inicioPhase.Id,
            Title = "Repositorio Git configurado",
            Description = "Se creó el repositorio en GitHub con estructura de carpetas",
            Author = "Juan Pérez"
        };
        var microResponse = await _client.PostAsJsonAsync($"/api/projects/{project.Id}/microincrements", micro1);
        microResponse.StatusCode.Should().Be(HttpStatusCode.Created);

        // Paso 6: Actualizamos el progreso de la iteración
        var updateIterationDto = new UpdateIterationDto(
            Name: iteration1.Name,
            StartDate: null,
            EndDate: null,
            PercentageCompleted: 25,
            Blockages: null,
            Observations: "Se completó la configuración del repositorio. Avance según lo planificado."
        );
        var updateResponse = await _client.PutAsJsonAsync($"/api/iterations/{iteration1.Id}", updateIterationDto);
        updateResponse.StatusCode.Should().Be(HttpStatusCode.OK);

        // Paso 7: Consultamos el progreso general del proyecto
        var iterationsListResponse = await _client.GetAsync($"/api/projects/{project.Id}/iterations");
        var allIterations = await iterationsListResponse.Content.ReadFromJsonAsync<List<IterationDto>>();
        allIterations.Should().HaveCount(1);

        var microincrementsListResponse = await _client.GetAsync($"/api/projects/{project.Id}/microincrements");
        var allMicroincrements = await microincrementsListResponse.Content.ReadFromJsonAsync<List<MicroincrementDto>>();
        allMicroincrements.Should().HaveCount(1);

        // Paso 8: Creamos la segunda iteración para continuar el seguimiento
        var iteration2Dto = new CreateIterationDto(
            Name: "Sprint 2 - Módulo de Inventario",
            StartDate: DateTime.UtcNow.AddDays(14),
            EndDate: DateTime.UtcNow.AddDays(28),
            Observations: "Desarrollo del módulo principal"
        );
        var iteration2Response = await _client.PostAsJsonAsync($"/api/projects/{project.Id}/iterations", iteration2Dto);
        iteration2Response.StatusCode.Should().Be(HttpStatusCode.Created);

        // Resultado: El equipo puede dar seguimiento completo al proyecto mediante iteraciones,
        // tareas y microincrementos, monitoreando el progreso de manera efectiva
        var finalIterationsResponse = await _client.GetAsync($"/api/projects/{project.Id}/iterations");
        var finalIterations = await finalIterationsResponse.Content.ReadFromJsonAsync<List<IterationDto>>();
        finalIterations.Should().HaveCount(2, "debe haber 2 iteraciones registradas");
    }

    [Fact(DisplayName = "Escenario: Visualizar progreso por fase mediante microincrementos")]
    public async Task EscenarioProgresoPorFase_ConsultarAvancePorFase_DebeReflejarTrabajoRealizado()
    {
        // Arrange - Crear proyecto
        var projectDto = AcceptanceTestDataHelper.CreateRealisticProjectDto();
        var projectResponse = await _client.PostAsJsonAsync("/api/projects", projectDto);
        var project = await projectResponse.Content.ReadFromJsonAsync<ProjectDto>();

        var phasesResponse = await _client.GetAsync($"/api/projects/{project!.Id}/phases");
        var phases = await phasesResponse.Content.ReadFromJsonAsync<List<ProjectPhaseDto>>();

        // Simular trabajo en fase de Inicio (3 microincrementos)
        var inicioPhase = phases!.First(p => p.Name == "Inicio");
        for (int i = 1; i <= 3; i++)
        {
            var micro = new CreateMicroincrementDto
            {
                ProjectPhaseId = inicioPhase.Id,
                Title = $"Actividad de Inicio #{i}",
                Description = $"Descripción de actividad {i}",
                Author = "Equipo"
            };
            await _client.PostAsJsonAsync($"/api/projects/{project.Id}/microincrements", micro);
        }

        // Simular trabajo en fase de Elaboración (2 microincrementos)
        var elaboracionPhase = phases.First(p => p.Name == "Elaboración");
        for (int i = 1; i <= 2; i++)
        {
            var micro = new CreateMicroincrementDto
            {
                ProjectPhaseId = elaboracionPhase.Id,
                Title = $"Actividad de Elaboración #{i}",
                Description = $"Descripción de actividad {i}",
                Author = "Equipo"
            };
            await _client.PostAsJsonAsync($"/api/projects/{project.Id}/microincrements", micro);
        }

        // Act - Consultar progreso por fase
        var inicioMicrosResponse = await _client.GetAsync($"/api/projects/{project.Id}/microincrements?phaseId={inicioPhase.Id}");
        var elaboracionMicrosResponse = await _client.GetAsync($"/api/projects/{project.Id}/microincrements?phaseId={elaboracionPhase.Id}");

        // Assert - Verificar que el progreso se refleja correctamente por fase
        var inicioMicros = await inicioMicrosResponse.Content.ReadFromJsonAsync<List<MicroincrementDto>>();
        var elaboracionMicros = await elaboracionMicrosResponse.Content.ReadFromJsonAsync<List<MicroincrementDto>>();

        inicioMicros.Should().HaveCount(3, "fase de Inicio tiene 3 microincrementos completados");
        elaboracionMicros.Should().HaveCount(2, "fase de Elaboración tiene 2 microincrementos completados");

        // El equipo puede visualizar el progreso distribuido por fases del proyecto
    }
}
