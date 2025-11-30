using System.Net;
using System.Net.Http.Json;
using backend.Contracts;
using backend.IntegrationTests.Fixtures;
using backend.IntegrationTests.Helpers;
using FluentAssertions;
using Xunit;

namespace backend.IntegrationTests.Controllers;

public class PlanVersionsControllerIntegrationTests : IClassFixture<CustomWebApplicationFactory<Program>>
{
    private readonly HttpClient _client;

    public PlanVersionsControllerIntegrationTests(CustomWebApplicationFactory<Program> factory)
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
    public async Task CreatePlanVersion_WithValidData_ReturnsCreatedVersion()
    {
        // Arrange
        var project = await CreateTestProject();
        var createDto = TestDataGenerator.GenerateCreatePlanVersionDto();

        // Act
        var response = await _client.PostAsJsonAsync($"/api/projects/{project.Id}/plan-versions", createDto);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Created);
        var planVersion = await response.Content.ReadFromJsonAsync<PlanVersionDto>();
        planVersion.Should().NotBeNull();
        planVersion!.ProjectId.Should().Be(project.Id);
        planVersion.Version.Should().Be(1);
        planVersion.Observaciones.Should().Be(createDto.Observaciones);
    }

    [Fact]
    public async Task CreateMultiplePlanVersions_IncrementsVersionNumber()
    {
        // Arrange
        var project = await CreateTestProject();
        var createDto1 = TestDataGenerator.GenerateCreatePlanVersionDto();
        var createDto2 = TestDataGenerator.GenerateCreatePlanVersionDto();

        // Act
        var response1 = await _client.PostAsJsonAsync($"/api/projects/{project.Id}/plan-versions", createDto1);
        var response2 = await _client.PostAsJsonAsync($"/api/projects/{project.Id}/plan-versions", createDto2);

        // Assert
        response1.StatusCode.Should().Be(HttpStatusCode.Created);
        response2.StatusCode.Should().Be(HttpStatusCode.Created);

        var version1 = await response1.Content.ReadFromJsonAsync<PlanVersionDto>();
        var version2 = await response2.Content.ReadFromJsonAsync<PlanVersionDto>();

        version1!.Version.Should().Be(1);
        version2!.Version.Should().Be(2);
    }

    [Fact]
    public async Task GetAllPlanVersions_ReturnsProjectVersions()
    {
        // Arrange
        var project = await CreateTestProject();
        var createDto1 = TestDataGenerator.GenerateCreatePlanVersionDto();
        var createDto2 = TestDataGenerator.GenerateCreatePlanVersionDto();
        
        await _client.PostAsJsonAsync($"/api/projects/{project.Id}/plan-versions", createDto1);
        await _client.PostAsJsonAsync($"/api/projects/{project.Id}/plan-versions", createDto2);

        // Act
        var response = await _client.GetAsync($"/api/projects/{project.Id}/plan-versions");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var versions = await response.Content.ReadFromJsonAsync<List<PlanVersionDto>>();
        versions.Should().NotBeNull();
        versions.Should().HaveCount(2);
        versions!.All(v => v.ProjectId == project.Id).Should().BeTrue();
    }

    [Fact]
    public async Task GetPlanVersionByNumber_WithValidVersion_ReturnsVersion()
    {
        // Arrange
        var project = await CreateTestProject();
        var createDto = TestDataGenerator.GenerateCreatePlanVersionDto();
        var createResponse = await _client.PostAsJsonAsync($"/api/projects/{project.Id}/plan-versions", createDto);
        var created = await createResponse.Content.ReadFromJsonAsync<PlanVersionDto>();

        // Act
        var response = await _client.GetAsync($"/api/projects/{project.Id}/plan-versions/{created!.Version}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var version = await response.Content.ReadFromJsonAsync<PlanVersionDto>();
        version.Should().NotBeNull();
        version!.Version.Should().Be(created.Version);
        version.ProjectId.Should().Be(project.Id);
    }

    [Fact]
    public async Task GetPlanVersionByNumber_WithInvalidVersion_ReturnsNotFound()
    {
        // Arrange
        var project = await CreateTestProject();

        // Act
        var response = await _client.GetAsync($"/api/projects/{project.Id}/plan-versions/999");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task PlanVersionLifecycle_CreateMultipleVersionsAndQuery_WorksCorrectly()
    {
        // Arrange
        var project = await CreateTestProject();

        // 1. Crear primera versión del plan
        var version1Dto = TestDataGenerator.GenerateCreatePlanVersionDto();
        version1Dto.Observaciones = "Versión inicial del plan";
        var response1 = await _client.PostAsJsonAsync($"/api/projects/{project.Id}/plan-versions", version1Dto);
        response1.StatusCode.Should().Be(HttpStatusCode.Created);

        // 2. Crear segunda versión del plan
        var version2Dto = TestDataGenerator.GenerateCreatePlanVersionDto();
        version2Dto.Observaciones = "Actualización del cronograma";
        var response2 = await _client.PostAsJsonAsync($"/api/projects/{project.Id}/plan-versions", version2Dto);
        response2.StatusCode.Should().Be(HttpStatusCode.Created);

        // 3. Crear tercera versión del plan
        var version3Dto = TestDataGenerator.GenerateCreatePlanVersionDto();
        version3Dto.Observaciones = "Revisión de objetivos";
        var response3 = await _client.PostAsJsonAsync($"/api/projects/{project.Id}/plan-versions", version3Dto);
        response3.StatusCode.Should().Be(HttpStatusCode.Created);

        // 4. Obtener todas las versiones
        var allVersionsResponse = await _client.GetAsync($"/api/projects/{project.Id}/plan-versions");
        var allVersions = await allVersionsResponse.Content.ReadFromJsonAsync<List<PlanVersionDto>>();
        allVersions.Should().HaveCount(3);

        // 5. Obtener versión específica
        var specificVersionResponse = await _client.GetAsync($"/api/projects/{project.Id}/plan-versions/2");
        var specificVersion = await specificVersionResponse.Content.ReadFromJsonAsync<PlanVersionDto>();
        specificVersion!.Version.Should().Be(2);
        specificVersion.Observaciones.Should().Be("Actualización del cronograma");
    }
}
