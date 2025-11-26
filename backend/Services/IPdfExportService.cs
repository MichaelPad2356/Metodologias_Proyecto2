namespace backend.Services;

public interface IPdfExportService
{
    byte[] GenerateProjectPlanPdf(int projectId);
}
