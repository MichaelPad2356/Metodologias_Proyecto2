using Bogus;
using backend.Contracts;

namespace backend.IntegrationTests.Helpers;

public static class TestDataGenerator
{
    private static readonly Faker Faker = new Faker("es");

    public static CreateProjectDto GenerateCreateProjectDto()
    {
        return new CreateProjectDto
        {
            Name = Faker.Commerce.ProductName(),
            Code = $"PROJ-{Faker.Random.Number(1000, 9999)}",
            StartDate = DateTime.Now.AddDays(Faker.Random.Number(-30, 30)),
            Description = Faker.Lorem.Paragraph(),
            Objetivos = Faker.Lorem.Sentences(3),
            Alcance = Faker.Lorem.Paragraph(),
            CronogramaInicial = Faker.Lorem.Sentence(),
            Responsables = Faker.Name.FullName(),
            Hitos = Faker.Lorem.Sentences(2),
            ResponsiblePerson = Faker.Name.FullName(),
            Tags = string.Join(", ", Faker.Lorem.Words(3))
        };
    }

    public static UpdateProjectDto GenerateUpdateProjectDto()
    {
        return new UpdateProjectDto
        {
            Name = Faker.Commerce.ProductName(),
            Description = Faker.Lorem.Paragraph(),
            Objetivos = Faker.Lorem.Sentences(3),
            Alcance = Faker.Lorem.Paragraph(),
            CronogramaInicial = Faker.Lorem.Sentence(),
            Responsables = Faker.Name.FullName(),
            Hitos = Faker.Lorem.Sentences(2),
            ResponsiblePerson = Faker.Name.FullName(),
            Tags = string.Join(", ", Faker.Lorem.Words(3))
        };
    }

    public static CreateIterationDto GenerateCreateIterationDto(int projectId, int phaseId)
    {
        var startDate = DateTime.Now.AddDays(Faker.Random.Number(1, 30));
        return new CreateIterationDto(
            Name: $"Iteración {Faker.Random.Number(1, 10)}",
            StartDate: startDate,
            EndDate: startDate.AddDays(Faker.Random.Number(7, 21)),
            Observations: Faker.Lorem.Sentence()
        );
    }

    public static CreateMicroincrementDto GenerateCreateMicroincrementDto(int projectPhaseId)
    {
        return new CreateMicroincrementDto
        {
            ProjectPhaseId = projectPhaseId,
            Title = Faker.Lorem.Sentence(3),
            Description = Faker.Lorem.Paragraph(),
            Author = Faker.Name.FullName()
        };
    }

    public static SavePlanVersionDto GenerateCreatePlanVersionDto()
    {
        return new SavePlanVersionDto
        {
            Observaciones = Faker.Lorem.Sentence()
        };
    }

    public static string GenerateRandomCode()
    {
        return $"PROJ-{Faker.Random.Number(1000, 9999)}";
    }

    public static string GenerateRandomName()
    {
        return Faker.Commerce.ProductName();
    }
}
