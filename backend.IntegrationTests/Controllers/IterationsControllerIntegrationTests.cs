using System.Net;
using System.Net.Http.Json;
using backend.Contracts;
using backend.IntegrationTests.Fixtures;
using backend.IntegrationTests.Helpers;
using FluentAssertions;
using Xunit;

namespace backend.IntegrationTests.Controllers;

public class IterationsControllerIntegrationTests : IClassFixture<CustomWebApplicationFactory<Program>>
{
    private readonly HttpClient _client;

    public IterationsControllerIntegrationTests(CustomWebApplicationFactory<Program> factory)
    {
        _client = factory.CreateClient();
    }

    private async Task<ProjectDto> CreateTestProject()
    {
        var createDto = TestDataGenerator.GenerateCreateProjectDto();
        var response = await _client.PostAsJsonAsync("/api/projects", createDto);
        return (await response.Content.ReadFromJsonAsync<ProjectDto>())!;
    }

    [Fact]
    public async Task CreateIteration_WithValidData_ReturnsCreatedIteration()
    {
        // Arrange
        var project = await CreateTestProject();
        var firstPhase = project.Phases.First();
        var createDto = TestDataGenerator.GenerateCreateIterationDto(project.Id, firstPhase.Id);

        // Act
        var response = await _client.PostAsJsonAsync($"/api/projects/{project.Id}/iterations", createDto);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Created);
        var iteration = await response.Content.ReadFromJsonAsync<IterationDto>();
        iteration.Should().NotBeNull();
        iteration!.Name.Should().Be(createDto.Name);
        iteration.ProjectId.Should().Be(project.Id);
    }

    [Fact]
    public async Task GetIterationsByProject_ReturnsProjectIterations()
    {
        // Arrange
        var project = await CreateTestProject();
        var firstPhase = project.Phases.First();
        var createDto1 = TestDataGenerator.GenerateCreateIterationDto(project.Id, firstPhase.Id);
        var createDto2 = TestDataGenerator.GenerateCreateIterationDto(project.Id, firstPhase.Id);
        
        await _client.PostAsJsonAsync($"/api/projects/{project.Id}/iterations", createDto1);
        await _client.PostAsJsonAsync($"/api/projects/{project.Id}/iterations", createDto2);

        // Act
        var response = await _client.GetAsync($"/api/projects/{project.Id}/iterations");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var iterations = await response.Content.ReadFromJsonAsync<List<IterationDto>>();
        iterations.Should().NotBeNull();
        iterations.Should().HaveCountGreaterOrEqualTo(2);
    }

    [Fact]
    public async Task GetIterationById_WithValidId_ReturnsIteration()
    {
        // Arrange
        var project = await CreateTestProject();
        var firstPhase = project.Phases.First();
        var createDto = TestDataGenerator.GenerateCreateIterationDto(project.Id, firstPhase.Id);
        var createResponse = await _client.PostAsJsonAsync($"/api/projects/{project.Id}/iterations", createDto);
        var createdIteration = await createResponse.Content.ReadFromJsonAsync<IterationDto>();

        // Act
        var response = await _client.GetAsync($"/api/projects/{project.Id}/iterations/{createdIteration!.Id}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var iteration = await response.Content.ReadFromJsonAsync<IterationDto>();
        iteration.Should().NotBeNull();
        iteration!.Id.Should().Be(createdIteration.Id);
    }

    [Fact]
    public async Task UpdateIteration_WithValidData_ReturnsUpdatedIteration()
    {
        // Arrange
        var project = await CreateTestProject();
        var firstPhase = project.Phases.First();
        var createDto = TestDataGenerator.GenerateCreateIterationDto(project.Id, firstPhase.Id);
        var createResponse = await _client.PostAsJsonAsync($"/api/projects/{project.Id}/iterations", createDto);
        var createdIteration = await createResponse.Content.ReadFromJsonAsync<IterationDto>();

        var updateDto = new UpdateIterationDto(
            Name: "Iteración Actualizada",
            StartDate: null,
            EndDate: null,
            PercentageCompleted: 50,
            Blockages: "Ninguno",
            Observations: "Nueva observación"
        );

        // Act
        var response = await _client.PutAsJsonAsync($"/api/projects/{project.Id}/iterations/{createdIteration!.Id}", updateDto);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var updatedIteration = await response.Content.ReadFromJsonAsync<IterationDto>();
        updatedIteration.Should().NotBeNull();
        updatedIteration!.Name.Should().Be(updateDto.Name);
        updatedIteration.PercentageCompleted.Should().Be(50);
    }

    [Fact]
    public async Task DeleteIteration_WithValidId_DeletesSuccessfully()
    {
        // Arrange
        var project = await CreateTestProject();
        var firstPhase = project.Phases.First();
        var createDto = TestDataGenerator.GenerateCreateIterationDto(project.Id, firstPhase.Id);
        var createResponse = await _client.PostAsJsonAsync($"/api/projects/{project.Id}/iterations", createDto);
        var createdIteration = await createResponse.Content.ReadFromJsonAsync<IterationDto>();

        // Act
        var response = await _client.DeleteAsync($"/api/projects/{project.Id}/iterations/{createdIteration!.Id}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        // Verificar que fue eliminada
        var getResponse = await _client.GetAsync($"/api/projects/{project.Id}/iterations/{createdIteration.Id}");
        getResponse.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task AddTaskToIteration_WithValidData_AddsTaskSuccessfully()
    {
        // Arrange
        var project = await CreateTestProject();
        var firstPhase = project.Phases.First();
        var createIterationDto = TestDataGenerator.GenerateCreateIterationDto(project.Id, firstPhase.Id);
        var createResponse = await _client.PostAsJsonAsync($"/api/projects/{project.Id}/iterations", createIterationDto);
        var iteration = await createResponse.Content.ReadFromJsonAsync<IterationDto>();

        var taskDto = new CreateIterationTaskDto(
            Name: "Tarea de prueba",
            Description: "Descripción de la tarea",
            StartDate: null,
            EndDate: null,
            AssignedTo: "Juan Pérez",
            ProjectPhaseId: firstPhase.Id
        );

        // Act
        var response = await _client.PostAsJsonAsync($"/api/projects/{project.Id}/iterations/{iteration!.Id}/tasks", taskDto);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Created);
    }

    [Fact]
    public async Task CompleteIterationLifecycle_CreateUpdateDelete_WorksCorrectly()
    {
        // Arrange
        var project = await CreateTestProject();
        var firstPhase = project.Phases.First();

        // 1. Crear iteración
        var createDto = TestDataGenerator.GenerateCreateIterationDto(project.Id, firstPhase.Id);
        var createResponse = await _client.PostAsJsonAsync($"/api/projects/{project.Id}/iterations", createDto);
        createResponse.StatusCode.Should().Be(HttpStatusCode.Created);
        var iteration = await createResponse.Content.ReadFromJsonAsync<IterationDto>();

        // 2. Actualizar iteración
        var updateDto = new UpdateIterationDto(
            Name: "Iteración Modificada",
            StartDate: null,
            EndDate: null,
            PercentageCompleted: 100,
            Blockages: null,
            Observations: "Iteración completada"
        );
        var updateResponse = await _client.PutAsJsonAsync($"/api/projects/{project.Id}/iterations/{iteration!.Id}", updateDto);
        updateResponse.StatusCode.Should().Be(HttpStatusCode.OK);

        // 3. Eliminar iteración
        var deleteResponse = await _client.DeleteAsync($"/api/projects/{project.Id}/iterations/{iteration.Id}");
        deleteResponse.StatusCode.Should().Be(HttpStatusCode.OK);

        // 4. Verificar eliminación
        var getResponse = await _client.GetAsync($"/api/projects/{project.Id}/iterations/{iteration.Id}");
        getResponse.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }
}
