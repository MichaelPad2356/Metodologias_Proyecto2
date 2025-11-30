using System.Net;
using System.Net.Http.Json;
using backend.AcceptanceTests.Fixtures;
using backend.AcceptanceTests.Helpers;
using backend.Contracts;
using FluentAssertions;
using Xunit;

namespace backend.AcceptanceTests.Scenarios;

/// <summary>
/// Pruebas de Aceptación para HU-003: Como miembro del equipo, quiero definir el plan del proyecto 
/// para establecer el alcance, objetivos y cronograma inicial
/// </summary>
public class HU003_DefinirPlanProyectoTests : IClassFixture<AcceptanceTestWebApplicationFactory<Program>>
{
    private readonly HttpClient _client;
    private readonly AcceptanceTestWebApplicationFactory<Program> _factory;

    public HU003_DefinirPlanProyectoTests(AcceptanceTestWebApplicationFactory<Program> factory)
    {
        _factory = factory;
        _client = factory.CreateClient();
    }

    [Fact(DisplayName = "AC1: Crear proyecto con todos los campos requeridos del plan inicial")]
    public async Task CriterioAceptacion1_CrearProyectoCompleto_DebeCrearProyectoConPlanInicial()
    {
        // Arrange - Preparar datos realistas del proyecto
        var projectDto = AcceptanceTestDataHelper.CreateRealisticProjectDto();

        // Act - Ejecutar la creación del proyecto
        var response = await _client.PostAsJsonAsync("/api/projects", projectDto);

        // Assert - Verificar criterios de aceptación
        response.StatusCode.Should().Be(HttpStatusCode.Created, 
            "el sistema debe crear el proyecto exitosamente");

        var createdProject = await response.Content.ReadFromJsonAsync<ProjectDto>();
        createdProject.Should().NotBeNull("debe retornar el proyecto creado");
        createdProject!.Name.Should().Be(projectDto.Name, "el nombre debe coincidir");
        createdProject.Code.Should().Be(projectDto.Code, "el código debe coincidir");
        createdProject.Alcance.Should().Be(projectDto.Alcance, "el alcance debe estar definido");
        createdProject.Objetivos.Should().Be(projectDto.Objetivos, "los objetivos deben estar definidos");
        createdProject.CronogramaInicial.Should().Be(projectDto.CronogramaInicial, "el cronograma debe estar definido");
        createdProject.Status.Should().Be("Active", "el proyecto debe estar activo");
        
        // Verificar que se crearon las fases de OpenUP automáticamente
        var phasesResponse = await _client.GetAsync($"/api/projects/{createdProject.Id}/phases");
        phasesResponse.StatusCode.Should().Be(HttpStatusCode.OK);
        
        var phases = await phasesResponse.Content.ReadFromJsonAsync<List<ProjectPhaseDto>>();
        phases.Should().NotBeNull().And.HaveCount(4, 
            "OpenUP define 4 fases: Inicio, Elaboración, Construcción, Transición");
        
        phases!.Select(p => p.Name).Should().Contain(new[] { "Inicio", "Elaboración", "Construcción", "Transición" });
    }

    [Fact(DisplayName = "AC2: Poder consultar el plan del proyecto después de crearlo")]
    public async Task CriterioAceptacion2_ConsultarPlanProyecto_DebeRetornarTodosLosCamposDelPlan()
    {
        // Arrange - Crear un proyecto primero
        var projectDto = AcceptanceTestDataHelper.CreateRealisticProjectDto();
        var createResponse = await _client.PostAsJsonAsync("/api/projects", projectDto);
        var createdProject = await createResponse.Content.ReadFromJsonAsync<ProjectDto>();

        // Act - Consultar el proyecto por código
        var response = await _client.GetAsync($"/api/projects/code/{createdProject!.Code}");

        // Assert - Verificar que se puede consultar el plan completo
        response.StatusCode.Should().Be(HttpStatusCode.OK, 
            "el usuario debe poder consultar el plan del proyecto");

        var project = await response.Content.ReadFromJsonAsync<ProjectDto>();
        project.Should().NotBeNull();
        project!.Alcance.Should().NotBeNullOrEmpty("el alcance debe estar visible");
        project.Objetivos.Should().NotBeNullOrEmpty("los objetivos deben estar visibles");
        project.Responsables.Should().NotBeNullOrEmpty("los responsables deben estar visibles");
        project.Hitos.Should().NotBeNullOrEmpty("los hitos deben estar visibles");
        project.CronogramaInicial.Should().NotBeNullOrEmpty("el cronograma debe estar visible");
    }

    [Fact(DisplayName = "AC3: Poder actualizar el plan del proyecto cuando cambian los requerimientos")]
    public async Task CriterioAceptacion3_ActualizarPlanProyecto_DebePermitirModificarAlcanceYObjetivos()
    {
        // Arrange - Crear un proyecto
        var projectDto = AcceptanceTestDataHelper.CreateRealisticProjectDto();
        var createResponse = await _client.PostAsJsonAsync("/api/projects", projectDto);
        var createdProject = await createResponse.Content.ReadFromJsonAsync<ProjectDto>();

        // Act - Actualizar el plan del proyecto con nuevos requerimientos
        var updateDto = new UpdateProjectDto
        {
            Name = createdProject!.Name,
            Description = createdProject.Description + " - ACTUALIZADO: Se agregó módulo de reportes avanzados",
            Alcance = createdProject.Alcance + "\n\nNUEVO ALCANCE: Incluye dashboard ejecutivo con indicadores KPI",
            Objetivos = createdProject.Objetivos + "\n5. Proveer dashboard ejecutivo para toma de decisiones",
            Responsables = createdProject.Responsables + ", 1 analista de datos",
            Hitos = createdProject.Hitos + "\nMes 5: Dashboard ejecutivo",
            CronogramaInicial = "Duración actualizada: 5 meses (se extendió por nuevo alcance)"
        };

        var updateResponse = await _client.PutAsJsonAsync($"/api/projects/{createdProject.Id}", updateDto);

        // Assert - Verificar que se actualizó correctamente
        updateResponse.StatusCode.Should().Be(HttpStatusCode.OK, 
            "el sistema debe permitir actualizar el plan del proyecto");

        var updatedProject = await updateResponse.Content.ReadFromJsonAsync<ProjectDto>();
        updatedProject.Should().NotBeNull();
        updatedProject!.Alcance.Should().Contain("dashboard ejecutivo", 
            "el nuevo alcance debe estar reflejado");
        updatedProject.Objetivos.Should().Contain("dashboard ejecutivo", 
            "los nuevos objetivos deben estar reflejados");
        updatedProject.CronogramaInicial.Should().Contain("5 meses", 
            "el cronograma actualizado debe estar reflejado");
    }

    [Fact(DisplayName = "AC4: El sistema debe validar que no se creen proyectos con códigos duplicados")]
    public async Task CriterioAceptacion4_CrearProyectoConCodigoDuplicado_DebeRechazarCreacion()
    {
        // Arrange - Crear un proyecto
        var projectDto = AcceptanceTestDataHelper.CreateRealisticProjectDto();
        await _client.PostAsJsonAsync("/api/projects", projectDto);

        // Act - Intentar crear otro proyecto con el mismo código
        var baseDto = AcceptanceTestDataHelper.CreateRealisticProjectDto();
        var duplicateDto = new CreateProjectDto
        {
            Name = baseDto.Name,
            Code = projectDto.Code, // Mismo código
            Description = baseDto.Description,
            StartDate = baseDto.StartDate,
            ResponsiblePerson = baseDto.ResponsiblePerson,
            Alcance = baseDto.Alcance,
            Objetivos = baseDto.Objetivos,
            Responsables = baseDto.Responsables,
            Hitos = baseDto.Hitos,
            CronogramaInicial = baseDto.CronogramaInicial,
            Tags = baseDto.Tags
        };
        var response = await _client.PostAsJsonAsync("/api/projects", duplicateDto);

        // Assert - Verificar que se rechaza
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest, 
            "no debe permitir códigos duplicados para evitar confusión");
    }

    [Fact(DisplayName = "AC5: Listar todos los proyectos activos para el equipo")]
    public async Task CriterioAceptacion5_ListarProyectosActivos_DebeRetornarSoloProyectosActivos()
    {
        // Arrange - Crear múltiples proyectos
        var project1 = AcceptanceTestDataHelper.CreateRealisticProjectDto();
        var project1Dto = new CreateProjectDto
        {
            Name = project1.Name,
            Code = "PROJ-001",
            Description = project1.Description,
            StartDate = project1.StartDate,
            ResponsiblePerson = project1.ResponsiblePerson,
            Alcance = project1.Alcance,
            Objetivos = project1.Objetivos,
            Responsables = project1.Responsables,
            Hitos = project1.Hitos,
            CronogramaInicial = project1.CronogramaInicial,
            Tags = project1.Tags
        };
        await _client.PostAsJsonAsync("/api/projects", project1Dto);

        var project2 = AcceptanceTestDataHelper.CreateRealisticProjectDto();
        var project2Dto = new CreateProjectDto
        {
            Name = project2.Name,
            Code = "PROJ-002",
            Description = project2.Description,
            StartDate = project2.StartDate,
            ResponsiblePerson = project2.ResponsiblePerson,
            Alcance = project2.Alcance,
            Objetivos = project2.Objetivos,
            Responsables = project2.Responsables,
            Hitos = project2.Hitos,
            CronogramaInicial = project2.CronogramaInicial,
            Tags = project2.Tags
        };
        var createResponse = await _client.PostAsJsonAsync("/api/projects", project2Dto);
        var createdProject2 = await createResponse.Content.ReadFromJsonAsync<ProjectDto>();

        // Archivar uno de los proyectos
        await _client.PostAsync($"/api/projects/{createdProject2!.Id}/archive", null);

        // Act - Listar proyectos activos
        var response = await _client.GetAsync("/api/projects");

        // Assert - Verificar que solo retorna proyectos activos
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var projects = await response.Content.ReadFromJsonAsync<List<ProjectListDto>>();
        projects.Should().NotBeNull();
        projects!.Should().Contain(p => p.Code == "PROJ-001", "debe incluir el proyecto activo");
        projects.Should().NotContain(p => p.Code == "PROJ-002", "no debe incluir proyectos archivados");
    }

    [Fact(DisplayName = "Escenario Completo: Definir y gestionar el plan de un proyecto real")]
    public async Task EscenarioCompleto_GestionCompletaPlanProyecto_DebePermitirCicloVidaCompleto()
    {
        // Paso 1: Como miembro del equipo, defino el plan inicial del proyecto
        var projectDto = AcceptanceTestDataHelper.CreateRealisticProjectDto();
        var createResponse = await _client.PostAsJsonAsync("/api/projects", projectDto);
        createResponse.StatusCode.Should().Be(HttpStatusCode.Created);
        var project = await createResponse.Content.ReadFromJsonAsync<ProjectDto>();

        // Paso 2: Consulto el proyecto para verificar que se guardó correctamente
        var getResponse = await _client.GetAsync($"/api/projects/{project!.Id}");
        getResponse.StatusCode.Should().Be(HttpStatusCode.OK);

        // Paso 3: Verifico que las fases de OpenUP se crearon automáticamente
        var phasesResponse = await _client.GetAsync($"/api/projects/{project.Id}/phases");
        var phases = await phasesResponse.Content.ReadFromJsonAsync<List<ProjectPhaseDto>>();
        phases.Should().HaveCount(4);

        // Paso 4: Durante el proyecto, necesito actualizar el alcance
        var updateDto = new UpdateProjectDto
        {
            Name = project.Name,
            Description = project.Description + " [Actualizado]",
            Alcance = project.Alcance + "\n\nActualización: Se agregó integración con sistema existente",
            Objetivos = project.Objetivos,
            Responsables = project.Responsables,
            Hitos = project.Hitos,
            CronogramaInicial = project.CronogramaInicial
        };
        var updateResponse = await _client.PutAsJsonAsync($"/api/projects/{project.Id}", updateDto);
        updateResponse.StatusCode.Should().Be(HttpStatusCode.OK);

        // Paso 5: Verifico que el proyecto aparece en la lista de proyectos del equipo
        var listResponse = await _client.GetAsync("/api/projects");
        var projectsList = await listResponse.Content.ReadFromJsonAsync<List<ProjectListDto>>();
        projectsList.Should().Contain(p => p.Id == project.Id);

        // Resultado: El equipo puede definir, consultar, actualizar y gestionar el plan del proyecto exitosamente
    }
}
