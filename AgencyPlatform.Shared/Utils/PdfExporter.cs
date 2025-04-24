using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using System.Collections.Generic;
using AgencyPlatform.Shared.Exporting.Models;

namespace AgencyPlatform.Shared.Utils
{
    public static class PdfExporter
    {
        public static byte[] ExportSolicitudes(List<SolicitudAgenciaExportDto> solicitudes)
        {
            var document = Document.Create(container =>
            {
                container.Page(page =>
                {
                    page.Margin(20);
                    page.Header().Text("Historial de Solicitudes").FontSize(20).Bold();
                    page.Content().Table(table =>
                    {
                        table.ColumnsDefinition(columns =>
                        {
                            columns.RelativeColumn(); // ID
                            columns.RelativeColumn(); // Acompañante
                            columns.RelativeColumn(); // Agencia
                            columns.RelativeColumn(); // Estado
                            columns.RelativeColumn(); // FechaSolicitud
                            columns.RelativeColumn(); // FechaRespuesta
                        });

                        // Encabezado
                        table.Header(header =>
                        {
                            header.Cell().Text("ID").Bold();
                            header.Cell().Text("Acompañante").Bold();
                            header.Cell().Text("Agencia").Bold();
                            header.Cell().Text("Estado").Bold();
                            header.Cell().Text("Solicitud").Bold();
                            header.Cell().Text("Respuesta").Bold();
                        });

                        // Contenido
                        foreach (var item in solicitudes)
                        {
                            table.Cell().Text(item.Id.ToString());
                            table.Cell().Text(item.NombreAcompanante);
                            table.Cell().Text(item.NombreAgencia);
                            table.Cell().Text(item.Estado);
                            table.Cell().Text(item.FechaSolicitud);
                            table.Cell().Text(item.FechaRespuesta);
                        }
                    });
                    page.Footer().AlignCenter().Text(x =>
                    {
                        x.Span("Exportado por AgencyPlatform - ");
                        x.Span($"{DateTime.UtcNow:dd/MM/yyyy HH:mm}");
                    });
                });
            });

            return document.GeneratePdf();
        }
    }
}
