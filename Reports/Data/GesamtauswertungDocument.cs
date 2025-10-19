using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using Zeiterfassung.Reports.Data;

namespace Zeiterfassung.Reports
{
    public class GesamtauswertungDocument : IDocument
    {
        private readonly List<GesamtauswertungReportModel> _projektdaten;
        private readonly string _zeitraum;

        public GesamtauswertungDocument(List<GesamtauswertungReportModel> projektdaten, string zeitraum)
        {
            _projektdaten = projektdaten;
            _zeitraum = zeitraum;
        }

        public DocumentMetadata GetMetadata() => DocumentMetadata.Default;

        public void Compose(IDocumentContainer container)
        {
            container.Page(page =>
            {
                page.Margin(50);
                page.Header().Element(ComposeHeader);
                page.Content().Element(ComposeContent);
                page.Footer().AlignCenter().Text(text =>
                {
                    text.CurrentPageNumber();
                    text.Span(" / ");
                    text.TotalPages();
                });
            });
        }

        void ComposeHeader(IContainer container)
        {
            container.Column(column =>
            {
                column.Item().Text("Gesamt-Zeitauswertung")
                    .SemiBold().FontSize(20);

                column.Item().Text($"Analysezeitraum: {_zeitraum}").FontSize(15);

                column.Item().PaddingTop(20).BorderBottom(1).BorderColor(Colors.Grey.Lighten1);
            });
        }

        void ComposeContent(IContainer container)
        {
            var gesamtstundenTotal = new TimeSpan(_projektdaten.Sum(p => p.GesamtDauer.Ticks));

            container.PaddingTop(20).Column(column =>
            {
                column.Item().AlignLeft().Text(text =>
                {
                    text.Span("Gesamtstunden aller Projekte: ").FontSize(14);
                    text.Span($"{Math.Floor(gesamtstundenTotal.TotalHours)}:{(gesamtstundenTotal.Minutes):00}").Bold().FontSize(14);
                });

                column.Item().PaddingTop(25).Table(table =>
                {
                    table.ColumnsDefinition(columns =>
                    {
                        columns.RelativeColumn(4); 
                        columns.RelativeColumn(3); 
                        columns.RelativeColumn(2); 
                        columns.RelativeColumn(3); 
                    });

                    table.Header(header =>
                    {
                        header.Cell().BorderBottom(1).Background(Colors.Grey.Lighten3).Padding(5).Text("Projekt").SemiBold();
                        header.Cell().BorderBottom(1).Background(Colors.Grey.Lighten3).Padding(5).AlignRight().Text("Gesamtstunden").SemiBold();
                        header.Cell().BorderBottom(1).Background(Colors.Grey.Lighten3).Padding(5).AlignCenter().Text("Mitarbeiter").SemiBold();
                        header.Cell().BorderBottom(1).Background(Colors.Grey.Lighten3).Padding(5).Text("Top-Mitarbeiter").SemiBold();
                    });

                    foreach (var projekt in _projektdaten)
                    {
                        table.Cell().BorderBottom(1).BorderColor(Colors.Grey.Lighten2).Padding(5).Text(projekt.ProjektName);
                        table.Cell().BorderBottom(1).BorderColor(Colors.Grey.Lighten2).Padding(5).AlignRight().Text($"{Math.Floor(projekt.GesamtDauer.TotalHours)}:{(projekt.GesamtDauer.Minutes):00}");
                        table.Cell().BorderBottom(1).BorderColor(Colors.Grey.Lighten2).Padding(5).AlignCenter().Text(projekt.AnzahlMitarbeiter);
                        table.Cell().BorderBottom(1).BorderColor(Colors.Grey.Lighten2).Padding(5).Text(projekt.TopMitarbeiter);
                    }
                });
            });
        }
    }
}