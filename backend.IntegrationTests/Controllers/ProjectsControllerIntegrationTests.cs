using System.Net;
using System.Net.Http.Json;
using backend.Contracts;
using backend.IntegrationTests.Fixtures;
using backend.IntegrationTests.Helpers;
using FluentAssertions;
using Xunit;

namespace backend.IntegrationTests.Controllers;

public class ProjectsControllerIntegrationTests : IClassFixture<CustomWebApplicationFactory<Program>>
{
    private readonly HttpClient _client;
    private readonly CustomWebApplicationFactory<Program> _factory;

    public ProjectsControllerIntegrationTests(CustomWebApplicationFactory<Program> factory)
    {
        _factory = factory;
        _client = factory.CreateClient();
    }

    #region CRUD Tests

    [Fact]
    public async Task CreateProject_WithValidData_ReturnsCreatedProject()
    {
        // Arrange
        var createDto = TestDataGenerator.GenerateCreateProjectDto();

        // Act
        var response = await _client.PostAsJsonAsync("/api/projects", createDto);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Created);
        var project = await response.Content.ReadFromJsonAsync<ProjectDto>();
        
        project.Should().NotBeNull();
        project!.Name.Should().Be(createDto.Name);
        project.Code.Should().Be(createDto.Code);
        project.Status.Should().Be("Active");
        project.Phases.Should().HaveCount(4);
        project.Phases.Should().Contain(p => p.Name == "Inicio" && p.Order == 1);
        project.Phases.Should().Contain(p => p.Name == "Elaboración" && p.Order == 2);
        project.Phases.Should().Contain(p => p.Name == "Construcción" && p.Order == 3);
        project.Phases.Should().Contain(p => p.Name == "Transición" && p.Order == 4);
    }

    [Fact]
    public async Task CreateProject_WithDuplicateCode_ReturnsBadRequest()
    {
        // Arrange
        var createDto = TestDataGenerator.GenerateCreateProjectDto();
        await _client.PostAsJsonAsync("/api/projects", createDto);

        // Act - Intentar crear otro proyecto con el mismo código
        var response = await _client.PostAsJsonAsync("/api/projects", createDto);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task GetAllProjects_ReturnsListOfProjects()
    {
        // Arrange
        var project1 = TestDataGenerator.GenerateCreateProjectDto();
        var project2 = TestDataGenerator.GenerateCreateProjectDto();
        await _client.PostAsJsonAsync("/api/projects", project1);
        await _client.PostAsJsonAsync("/api/projects", project2);

        // Act
        var response = await _client.GetAsync("/api/projects");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var projects = await response.Content.ReadFromJsonAsync<List<ProjectListDto>>();
        projects.Should().NotBeNull();
        projects.Should().HaveCountGreaterOrEqualTo(2);
    }

    [Fact]
    public async Task GetProjectById_WithValidId_ReturnsProject()
    {
        // Arrange
        var createDto = TestDataGenerator.GenerateCreateProjectDto();
        var createResponse = await _client.PostAsJsonAsync("/api/projects", createDto);
        var createdProject = await createResponse.Content.ReadFromJsonAsync<ProjectDto>();

        // Act
        var response = await _client.GetAsync($"/api/projects/{createdProject!.Id}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var project = await response.Content.ReadFromJsonAsync<ProjectDto>();
        project.Should().NotBeNull();
        project!.Id.Should().Be(createdProject.Id);
        project.Name.Should().Be(createDto.Name);
    }

    [Fact]
    public async Task GetProjectById_WithInvalidId_ReturnsNotFound()
    {
        // Act
        var response = await _client.GetAsync("/api/projects/99999");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task GetProjectByCode_WithValidCode_ReturnsProject()
    {
        // Arrange
        var createDto = TestDataGenerator.GenerateCreateProjectDto();
        await _client.PostAsJsonAsync("/api/projects", createDto);

        // Act
        var response = await _client.GetAsync($"/api/projects/by-code/{createDto.Code}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var project = await response.Content.ReadFromJsonAsync<ProjectDto>();
        project.Should().NotBeNull();
        project!.Code.Should().Be(createDto.Code);
    }

    [Fact]
    public async Task UpdateProject_WithValidData_ReturnsUpdatedProject()
    {
        // Arrange
        var createDto = TestDataGenerator.GenerateCreateProjectDto();
        var createResponse = await _client.PostAsJsonAsync("/api/projects", createDto);
        var createdProject = await createResponse.Content.ReadFromJsonAsync<ProjectDto>();

        var updateDto = TestDataGenerator.GenerateUpdateProjectDto();

        // Act
        var response = await _client.PutAsJsonAsync($"/api/projects/{createdProject!.Id}", updateDto);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var updatedProject = await response.Content.ReadFromJsonAsync<ProjectDto>();
        updatedProject.Should().NotBeNull();
        updatedProject!.Name.Should().Be(updateDto.Name);
        updatedProject.Description.Should().Be(updateDto.Description);
    }

    #endregion

    #region Archive/Unarchive Tests

    [Fact]
    public async Task ArchiveProject_WithValidId_ArchivesSuccessfully()
    {
        // Arrange
        var createDto = TestDataGenerator.GenerateCreateProjectDto();
        var createResponse = await _client.PostAsJsonAsync("/api/projects", createDto);
        var createdProject = await createResponse.Content.ReadFromJsonAsync<ProjectDto>();

        // Act
        var response = await _client.PostAsync($"/api/projects/{createdProject!.Id}/archive", null);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        // Verificar que el proyecto fue archivado
        var getResponse = await _client.GetAsync($"/api/projects/{createdProject.Id}");
        var archivedProject = await getResponse.Content.ReadFromJsonAsync<ProjectDto>();
        archivedProject!.Status.Should().Be("Archived");
        archivedProject.ArchivedAt.Should().NotBeNull();
    }

    [Fact]
    public async Task UnarchiveProject_WithArchivedProject_UnarchivesSuccessfully()
    {
        // Arrange
        var createDto = TestDataGenerator.GenerateCreateProjectDto();
        var createResponse = await _client.PostAsJsonAsync("/api/projects", createDto);
        var createdProject = await createResponse.Content.ReadFromJsonAsync<ProjectDto>();
        await _client.PostAsync($"/api/projects/{createdProject!.Id}/archive", null);

        // Act
        var response = await _client.PostAsync($"/api/projects/{createdProject.Id}/unarchive", null);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        // Verificar que el proyecto fue desarchivado
        var getResponse = await _client.GetAsync($"/api/projects/{createdProject.Id}");
        var unarchivedProject = await getResponse.Content.ReadFromJsonAsync<ProjectDto>();
        unarchivedProject!.Status.Should().Be("Active");
        unarchivedProject.ArchivedAt.Should().BeNull();
    }

    [Fact]
    public async Task GetAllProjects_WithIncludeArchived_ReturnsArchivedProjects()
    {
        // Arrange
        var createDto = TestDataGenerator.GenerateCreateProjectDto();
        var createResponse = await _client.PostAsJsonAsync("/api/projects", createDto);
        var createdProject = await createResponse.Content.ReadFromJsonAsync<ProjectDto>();
        await _client.PostAsync($"/api/projects/{createdProject!.Id}/archive", null);

        // Act - Sin incluir archivados
        var response1 = await _client.GetAsync("/api/projects?includeArchived=false");
        var projects1 = await response1.Content.ReadFromJsonAsync<List<ProjectListDto>>();

        // Act - Incluyendo archivados
        var response2 = await _client.GetAsync("/api/projects?includeArchived=true");
        var projects2 = await response2.Content.ReadFromJsonAsync<List<ProjectListDto>>();

        // Assert
        var archivedCount1 = projects1!.Count(p => p.Status == "Archived");
        var archivedCount2 = projects2!.Count(p => p.Status == "Archived");
        archivedCount2.Should().BeGreaterThan(archivedCount1);
    }

    #endregion

    #region Delete Tests

    [Fact]
    public async Task DeleteProject_WithValidId_DeletesSuccessfully()
    {
        // Arrange
        var createDto = TestDataGenerator.GenerateCreateProjectDto();
        var createResponse = await _client.PostAsJsonAsync("/api/projects", createDto);
        var createdProject = await createResponse.Content.ReadFromJsonAsync<ProjectDto>();

        // Act
        var response = await _client.DeleteAsync($"/api/projects/{createdProject!.Id}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        // Verificar que el proyecto fue eliminado
        var getResponse = await _client.GetAsync($"/api/projects/{createdProject.Id}");
        getResponse.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task DeleteProject_WithInvalidId_ReturnsNotFound()
    {
        // Act
        var response = await _client.DeleteAsync("/api/projects/99999");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    #endregion

    #region Phase Tests

    [Fact]
    public async Task GetProjectPhases_WithValidProjectId_ReturnsPhases()
    {
        // Arrange
        var createDto = TestDataGenerator.GenerateCreateProjectDto();
        var createResponse = await _client.PostAsJsonAsync("/api/projects", createDto);
        var createdProject = await createResponse.Content.ReadFromJsonAsync<ProjectDto>();

        // Act
        var response = await _client.GetAsync($"/api/projects/{createdProject!.Id}/phases");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var phases = await response.Content.ReadFromJsonAsync<List<ProjectPhaseDto>>();
        phases.Should().NotBeNull();
        phases.Should().HaveCount(4);
        phases!.Select(p => p.Name).Should().Contain(new[] { "Inicio", "Elaboración", "Construcción", "Transición" });
    }

    #endregion

    #region Integration Flow Tests

    [Fact]
    public async Task CompleteProjectLifecycle_CreateUpdateArchiveUnarchiveDelete_WorksCorrectly()
    {
        // 1. Crear proyecto
        var createDto = TestDataGenerator.GenerateCreateProjectDto();
        var createResponse = await _client.PostAsJsonAsync("/api/projects", createDto);
        createResponse.StatusCode.Should().Be(HttpStatusCode.Created);
        var project = await createResponse.Content.ReadFromJsonAsync<ProjectDto>();
        project.Should().NotBeNull();

        // 2. Actualizar proyecto
        var updateDto = TestDataGenerator.GenerateUpdateProjectDto();
        var updateResponse = await _client.PutAsJsonAsync($"/api/projects/{project!.Id}", updateDto);
        updateResponse.StatusCode.Should().Be(HttpStatusCode.OK);

        // 3. Archivar proyecto
        var archiveResponse = await _client.PostAsync($"/api/projects/{project.Id}/archive", null);
        archiveResponse.StatusCode.Should().Be(HttpStatusCode.OK);

        // 4. Desarchivar proyecto
        var unarchiveResponse = await _client.PostAsync($"/api/projects/{project.Id}/unarchive", null);
        unarchiveResponse.StatusCode.Should().Be(HttpStatusCode.OK);

        // 5. Eliminar proyecto
        var deleteResponse = await _client.DeleteAsync($"/api/projects/{project.Id}");
        deleteResponse.StatusCode.Should().Be(HttpStatusCode.OK);

        // 6. Verificar que fue eliminado
        var getResponse = await _client.GetAsync($"/api/projects/{project.Id}");
        getResponse.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    #endregion
}
