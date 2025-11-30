using System.Net;
using System.Net.Http.Json;
using backend.Contracts;
using backend.IntegrationTests.Fixtures;
using backend.IntegrationTests.Helpers;
using FluentAssertions;
using Xunit;

namespace backend.IntegrationTests.Controllers;

public class MicroincrementsControllerIntegrationTests : IClassFixture<CustomWebApplicationFactory<Program>>
{
    private readonly HttpClient _client;

    public MicroincrementsControllerIntegrationTests(CustomWebApplicationFactory<Program> factory)
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
    public async Task CreateMicroincrement_WithValidData_ReturnsCreatedMicroincrement()
    {
        // Arrange
        var project = await CreateTestProject();
        var firstPhase = project.Phases.First();
        var createDto = TestDataGenerator.GenerateCreateMicroincrementDto(firstPhase.Id);

        // Act
        var response = await _client.PostAsJsonAsync("/api/microincrements", createDto);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Created);
        var microincrement = await response.Content.ReadFromJsonAsync<MicroincrementDto>();
        microincrement.Should().NotBeNull();
        microincrement!.Title.Should().Be(createDto.Title);
        microincrement.ProjectPhaseId.Should().Be(firstPhase.Id);
    }

    [Fact]
    public async Task GetAllMicroincrements_ReturnsMicroincrementsList()
    {
        // Arrange
        var project = await CreateTestProject();
        var firstPhase = project.Phases.First();
        var createDto1 = TestDataGenerator.GenerateCreateMicroincrementDto(firstPhase.Id);
        var createDto2 = TestDataGenerator.GenerateCreateMicroincrementDto(firstPhase.Id);
        
        await _client.PostAsJsonAsync("/api/microincrements", createDto1);
        await _client.PostAsJsonAsync("/api/microincrements", createDto2);

        // Act
        var response = await _client.GetAsync("/api/microincrements");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var microincrements = await response.Content.ReadFromJsonAsync<List<MicroincrementDto>>();
        microincrements.Should().NotBeNull();
        microincrements.Should().HaveCountGreaterOrEqualTo(2);
    }

    [Fact]
    public async Task GetMicroincrementById_WithValidId_ReturnsMicroincrement()
    {
        // Arrange
        var project = await CreateTestProject();
        var firstPhase = project.Phases.First();
        var createDto = TestDataGenerator.GenerateCreateMicroincrementDto(firstPhase.Id);
        var createResponse = await _client.PostAsJsonAsync("/api/microincrements", createDto);
        var created = await createResponse.Content.ReadFromJsonAsync<MicroincrementDto>();

        // Act
        var response = await _client.GetAsync($"/api/microincrements/{created!.Id}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var microincrement = await response.Content.ReadFromJsonAsync<MicroincrementDto>();
        microincrement.Should().NotBeNull();
        microincrement!.Id.Should().Be(created.Id);
    }

    [Fact]
    public async Task GetMicroincrementsByPhase_ReturnsPhaseIncrements()
    {
        // Arrange
        var project = await CreateTestProject();
        var firstPhase = project.Phases.First();
        var createDto = TestDataGenerator.GenerateCreateMicroincrementDto(firstPhase.Id);
        await _client.PostAsJsonAsync("/api/microincrements", createDto);

        // Act
        var response = await _client.GetAsync($"/api/microincrements/by-iteration/{firstPhase.Id}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var microincrements = await response.Content.ReadFromJsonAsync<List<MicroincrementDto>>();
        microincrements.Should().NotBeNull();
        microincrements.Should().HaveCountGreaterOrEqualTo(1);
        microincrements!.All(m => m.ProjectPhaseId == firstPhase.Id).Should().BeTrue();
    }

    [Fact]
    public async Task GetMicroincrementsByAuthor_ReturnsAuthorIncrements()
    {
        // Arrange
        var project = await CreateTestProject();
        var firstPhase = project.Phases.First();
        var author = "Juan Pérez";
        var createDto = TestDataGenerator.GenerateCreateMicroincrementDto(firstPhase.Id);
        createDto.Author = author;
        await _client.PostAsJsonAsync("/api/microincrements", createDto);

        // Act
        var response = await _client.GetAsync($"/api/microincrements/by-author/{author}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var microincrements = await response.Content.ReadFromJsonAsync<List<MicroincrementDto>>();
        microincrements.Should().NotBeNull();
        microincrements.Should().HaveCountGreaterOrEqualTo(1);
        microincrements!.All(m => m.Author == author).Should().BeTrue();
    }

    [Fact]
    public async Task UpdateMicroincrement_WithValidData_ReturnsUpdatedMicroincrement()
    {
        // Arrange
        var project = await CreateTestProject();
        var firstPhase = project.Phases.First();
        var createDto = TestDataGenerator.GenerateCreateMicroincrementDto(firstPhase.Id);
        var createResponse = await _client.PostAsJsonAsync("/api/microincrements", createDto);
        var created = await createResponse.Content.ReadFromJsonAsync<MicroincrementDto>();

        var updateDto = new UpdateMicroincrementDto
        {
            Title = "Microincremento Actualizado",
            Description = "Nueva descripción"
        };

        // Act
        var response = await _client.PutAsJsonAsync($"/api/microincrements/{created!.Id}", updateDto);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var updated = await response.Content.ReadFromJsonAsync<MicroincrementDto>();
        updated.Should().NotBeNull();
        updated!.Title.Should().Be(updateDto.Title);
        updated.Description.Should().Be(updateDto.Description);
    }

    [Fact]
    public async Task DeleteMicroincrement_WithValidId_DeletesSuccessfully()
    {
        // Arrange
        var project = await CreateTestProject();
        var firstPhase = project.Phases.First();
        var createDto = TestDataGenerator.GenerateCreateMicroincrementDto(firstPhase.Id);
        var createResponse = await _client.PostAsJsonAsync("/api/microincrements", createDto);
        var created = await createResponse.Content.ReadFromJsonAsync<MicroincrementDto>();

        // Act
        var response = await _client.DeleteAsync($"/api/microincrements/{created!.Id}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NoContent);

        // Verificar eliminación
        var getResponse = await _client.GetAsync($"/api/microincrements/{created.Id}");
        getResponse.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task CompleteMicroincrementLifecycle_CreateUpdateDelete_WorksCorrectly()
    {
        // Arrange
        var project = await CreateTestProject();
        var firstPhase = project.Phases.First();

        // 1. Crear microincremento
        var createDto = TestDataGenerator.GenerateCreateMicroincrementDto(firstPhase.Id);
        var createResponse = await _client.PostAsJsonAsync("/api/microincrements", createDto);
        createResponse.StatusCode.Should().Be(HttpStatusCode.Created);
        var microincrement = await createResponse.Content.ReadFromJsonAsync<MicroincrementDto>();

        // 2. Actualizar microincremento
        var updateDto = new UpdateMicroincrementDto
        {
            Title = "Microincremento Modificado",
            Description = "Actualización de descripción"
        };
        var updateResponse = await _client.PutAsJsonAsync($"/api/microincrements/{microincrement!.Id}", updateDto);
        updateResponse.StatusCode.Should().Be(HttpStatusCode.OK);

        // 3. Eliminar microincremento
        var deleteResponse = await _client.DeleteAsync($"/api/microincrements/{microincrement.Id}");
        deleteResponse.StatusCode.Should().Be(HttpStatusCode.NoContent);

        // 4. Verificar eliminación
        var getResponse = await _client.GetAsync($"/api/microincrements/{microincrement.Id}");
        getResponse.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }
}
