using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using Zeiterfassung.Reports.Data;

namespace Zeiterfassung.Reports
{
    public class ProjektnachweisDocument : IDocument
    {
        private readonly List<ProjektnachweisReportModel> _zeiten;
        private readonly string _projektName;
        private readonly string _leistungszeitraum;

        public ProjektnachweisDocument(List<ProjektnachweisReportModel> zeiten, string projektName, string leistungszeitraum)
        {
            _zeiten = zeiten;
            _projektName = projektName;
            _leistungszeitraum = leistungszeitraum;
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
                column.Item().Text($"Leistungsnachweis für Projekt: {_projektName}")
                    .SemiBold().FontSize(20);

                column.Item().Text($"Leistungszeitraum: {_leistungszeitraum}").FontSize(15);

                column.Item().PaddingTop(20).BorderBottom(1).BorderColor(Colors.Grey.Lighten1);
            });
        }

        void ComposeContent(IContainer container)
        {
            var groupedZeiten = _zeiten
                .GroupBy(z => z.Datum.Date)
                .OrderBy(g => g.Key);

            container.PaddingTop(20).Table(table =>
            {
                table.ColumnsDefinition(columns =>
                {
                    columns.ConstantColumn(80);    // Datum
                    columns.RelativeColumn(3);     // Mitarbeiter
                    columns.RelativeColumn(5);     // Tätigkeit
                    columns.ConstantColumn(60);    // Dauer
                });

                table.Header(header =>
                {
                    header.Cell().BorderBottom(1).Background(Colors.Grey.Lighten3).Padding(5).Text("Datum").SemiBold();
                    header.Cell().BorderBottom(1).Background(Colors.Grey.Lighten3).Padding(5).Text("Mitarbeiter").SemiBold();
                    header.Cell().BorderBottom(1).Background(Colors.Grey.Lighten3).Padding(5).Text("Tätigkeit").SemiBold();
                    header.Cell().BorderBottom(1).Background(Colors.Grey.Lighten3).Padding(5).AlignRight().Text("Dauer").SemiBold();
                });

                foreach (var group in groupedZeiten)
                {
                    table.Cell().RowSpan((uint)group.Count()).BorderBottom(1).BorderColor(Colors.Grey.Lighten2).Padding(5).Text(group.Key.ToString("dd.MM.yyyy"));

                    foreach (var item in group)
                    {
                        table.Cell().BorderBottom(1).BorderColor(Colors.Grey.Lighten2).Padding(5).Text(item.MitarbeiterName);
                        table.Cell().BorderBottom(1).BorderColor(Colors.Grey.Lighten2).Padding(5).Text(item.Taetigkeit ?? "-");
                        table.Cell().BorderBottom(1).BorderColor(Colors.Grey.Lighten2).Padding(5).AlignRight().Text($"{item.Dauer:hh\\:mm}");
                    }
                }

                var gesamtDauer = new TimeSpan(_zeiten.Sum(z => z.Dauer.Ticks));

                table.Cell().ColumnSpan(3).BorderTop(1).Padding(5).AlignRight().Text("Gesamtstunden:").Bold();
                table.Cell().BorderTop(1).Padding(5).AlignRight().Text($"{Math.Floor(gesamtDauer.TotalHours)}:{(gesamtDauer.Minutes):00}").Bold();
            });
        }
    }
}