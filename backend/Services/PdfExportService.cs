using backend.Data;
using backend.Models;
using Microsoft.EntityFrameworkCore;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using System.Text.Json;

namespace backend.Services;

public class PdfExportService : IPdfExportService
{
    private readonly ApplicationDbContext _context;

    public PdfExportService(ApplicationDbContext context)
    {
        _context = context;
        QuestPDF.Settings.License = LicenseType.Community;
    }

    public byte[] GenerateProjectPlanPdf(int projectId)
    {
        try
        {
            var project = _context.Projects
                .Include(p => p.Phases)
                .FirstOrDefault(p => p.Id == projectId);

            if (project == null)
            {
                throw new InvalidOperationException($"Proyecto con ID {projectId} no encontrado");
            }

            var document = Document.Create(container =>
            {
            container.Page(page =>
            {
                page.Size(PageSizes.A4);
                page.Margin(2, Unit.Centimetre);
                page.PageColor(Colors.White);
                page.DefaultTextStyle(x => x.FontSize(11).FontFamily("Arial"));

                page.Header()
                    .Text($"Plan del Proyecto: {project.Name}")
                    .SemiBold().FontSize(20).FontColor(Colors.Blue.Medium);

                page.Content()
                    .PaddingVertical(1, Unit.Centimetre)
                    .Column(column =>
                    {
                        column.Spacing(15);

                        // Información básica
                        column.Item().Element(container => RenderSection(container, "Información Básica", c =>
                        {
                            c.Row(row =>
                            {
                                row.RelativeItem().Column(col =>
                                {
                                    col.Item().Text($"Código: {project.Code}").FontSize(10);
                                    col.Item().Text($"Fecha de inicio: {project.StartDate:dd/MM/yyyy}").FontSize(10);
                                    col.Item().Text($"Estado: {project.Status}").FontSize(10);
                                });
                                row.RelativeItem().Column(col =>
                                {
                                    if (!string.IsNullOrEmpty(project.ResponsiblePerson))
                                        col.Item().Text($"Responsable: {project.ResponsiblePerson}").FontSize(10);
                                    if (!string.IsNullOrEmpty(project.Tags))
                                        col.Item().Text($"Etiquetas: {project.Tags}").FontSize(10);
                                });
                            });
                        }));

                        // Descripción
                        if (!string.IsNullOrEmpty(project.Description))
                        {
                            column.Item().Element(container => RenderSection(container, "Descripción", c =>
                            {
                                c.Text(project.Description).FontSize(10);
                            }));
                        }

                        // Objetivos
                        if (!string.IsNullOrEmpty(project.Objetivos))
                        {
                            column.Item().Element(container => RenderSection(container, "🎯 Objetivos", c =>
                            {
                                c.Text(project.Objetivos).FontSize(10);
                            }));
                        }

                        // Alcance
                        if (!string.IsNullOrEmpty(project.Alcance))
                        {
                            column.Item().Element(container => RenderSection(container, "📐 Alcance", c =>
                            {
                                c.Text(project.Alcance).FontSize(10);
                            }));
                        }

                        // Cronograma Inicial
                        if (!string.IsNullOrEmpty(project.CronogramaInicial))
                        {
                            column.Item().Element(container => RenderSection(container, "📅 Cronograma Inicial", c =>
                            {
                                var cronograma = ParseCronograma(project.CronogramaInicial);
                                foreach (var item in cronograma)
                                {
                                    c.Row(row =>
                                    {
                                        row.ConstantItem(100).Text($"{item.Date:dd/MM/yyyy}").FontSize(9).Bold();
                                        row.RelativeItem().Text(item.Description).FontSize(9);
                                    });
                                }
                            }));
                        }

                        // Responsables
                        if (!string.IsNullOrEmpty(project.Responsables))
                        {
                            column.Item().Element(container => RenderSection(container, "👥 Responsables", c =>
                            {
                                var responsables = ParseStringArray(project.Responsables);
                                foreach (var resp in responsables)
                                {
                                    c.Text($"• {resp}").FontSize(10);
                                }
                            }));
                        }

                        // Hitos
                        if (!string.IsNullOrEmpty(project.Hitos))
                        {
                            column.Item().Element(container => RenderSection(container, "🏁 Hitos", c =>
                            {
                                var hitos = ParseStringArray(project.Hitos);
                                var index = 1;
                                foreach (var hito in hitos)
                                {
                                    c.Text($"{index}. {hito}").FontSize(10);
                                    index++;
                                }
                            }));
                        }

                        // Fases de OpenUP
                        column.Item().Element(container => RenderSection(container, "Fases de OpenUP", c =>
                        {
                            foreach (var phase in project.Phases.OrderBy(p => p.Order))
                            {
                                c.Row(row =>
                                {
                                    row.ConstantItem(30).Text($"{phase.Order}.").FontSize(10).Bold();
                                    row.RelativeItem().Column(col =>
                                    {
                                        col.Item().Text(phase.Name).FontSize(10).Bold();
                                        col.Item().Text($"Estado: {phase.Status}").FontSize(9).FontColor(Colors.Grey.Darken2);
                                    });
                                });
                            }
                        }));
                    });

                page.Footer()
                    .AlignCenter()
                    .Text($"Generado el: {DateTime.Now:dd/MM/yyyy HH:mm}")
                    .FontSize(9);
            });
        });

        return document.GeneratePdf();
        }
        catch (Exception ex)
        {
            try
            {
                var logPath = Path.Combine(AppContext.BaseDirectory, "pdf-error.log");
                File.WriteAllText(logPath, ex.ToString());
            }
            catch { }

            // Re-throw to be handled by controller
            throw;
        }
    }

    private void RenderSection(IContainer container, string title, Action<IContainer> content)
    {
        container.Column(column =>
        {
            column.Item()
                .BorderBottom(2)
                .BorderColor(Colors.Blue.Lighten2)
                .PaddingBottom(5)
                .Text(title)
                .Bold()
                .FontSize(13)
                .FontColor(Colors.Blue.Medium);

            column.Item()
                .PaddingTop(5)
                .Element(content);
        });
    }

    private List<CronogramaItem> ParseCronograma(string json)
    {
        try
        {
            return JsonSerializer.Deserialize<List<CronogramaItem>>(json) ?? new List<CronogramaItem>();
        }
        catch
        {
            return new List<CronogramaItem>();
        }
    }

    private List<string> ParseStringArray(string json)
    {
        try
        {
            return JsonSerializer.Deserialize<List<string>>(json) ?? new List<string>();
        }
        catch
        {
            return new List<string>();
        }
    }

    private class CronogramaItem
    {
        public DateTime Date { get; set; }
        public string Description { get; set; } = string.Empty;
    }
}
