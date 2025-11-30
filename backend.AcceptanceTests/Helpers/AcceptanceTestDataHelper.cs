using backend.Contracts;
using Bogus;

namespace backend.AcceptanceTests.Helpers;

public static class AcceptanceTestDataHelper
{
    private static readonly Faker _faker = new Faker("es");

    public static CreateProjectDto CreateRealisticProjectDto()
    {
        return new CreateProjectDto
        {
            Name = "Sistema de Gestión de Inventario para Farmacia Central",
            Code = $"FARM-{_faker.Random.Number(1000, 9999)}",
            Description = "Desarrollo de un sistema web para la gestión integral del inventario de medicamentos, control de stock, alertas de vencimiento y reportes de ventas.",
            StartDate = DateTime.UtcNow.AddDays(7),
            ResponsiblePerson = "María González",
            Alcance = "El sistema permitirá gestionar el inventario completo de productos farmacéuticos, incluyendo medicamentos controlados, suplementos y productos de venta libre. Incluirá módulos de compras, ventas, reportes y alertas automáticas.",
            Objetivos = "1. Automatizar el control de inventario\n2. Reducir pérdidas por vencimiento de productos\n3. Mejorar la trazabilidad de medicamentos controlados\n4. Generar reportes de ventas en tiempo real",
            Responsables = "Equipo de desarrollo: 3 desarrolladores, 1 tester, 1 analista",
            Hitos = "Mes 1: Módulo de inventario básico\nMes 2: Módulo de ventas\nMes 3: Reportes y alertas\nMes 4: Pruebas y despliegue",
            CronogramaInicial = "Duración estimada: 4 meses\nInicio: " + DateTime.UtcNow.AddDays(7).ToString("dd/MM/yyyy") + "\nFin estimado: " + DateTime.UtcNow.AddMonths(4).ToString("dd/MM/yyyy"),
            Tags = "farmacia,inventario,salud,medicamentos"
        };
    }

    public static CreateIterationDto CreateRealisticIterationDto()
    {
        var startDate = DateTime.UtcNow.AddDays(1);
        var endDate = startDate.AddDays(14);
        
        return new CreateIterationDto(
            Name: "Sprint 1 - Arquitectura Base",
            StartDate: startDate,
            EndDate: endDate,
            Observations: "Primera iteración enfocada en establecer la arquitectura del proyecto y configuración inicial del entorno de desarrollo."
        );
    }

    public static UpdateIterationDto CreateIterationUpdateDto()
    {
        return new UpdateIterationDto(
            Name: "Sprint 1 - Arquitectura Base (Actualizado)",
            StartDate: null,
            EndDate: null,
            PercentageCompleted: 75,
            Blockages: null,
            Observations: "Se completó la configuración del proyecto. Se identificaron algunos riesgos técnicos que requieren atención en la siguiente iteración."
        );
    }

    public static CreateMicroincrementDto CreateRealisticMicroincrementDto(int phaseId)
    {
        return new CreateMicroincrementDto
        {
            ProjectPhaseId = phaseId,
            Title = "Configuración del repositorio Git y estructura inicial del proyecto",
            Description = "Se creó el repositorio en GitHub, se configuró la estructura de carpetas del proyecto (backend/frontend), se inicializó el proyecto .NET y Angular, y se configuró el archivo .gitignore.",
            Author = "Juan Pérez"
        };
    }

    public static SavePlanVersionDto CreateRealisticPlanVersionDto()
    {
        return new SavePlanVersionDto
        {
            Observaciones = "Se extendió el alcance del proyecto para incluir facturación electrónica según nueva normativa DIAN. Se requiere especialista adicional."
        };
    }
}
