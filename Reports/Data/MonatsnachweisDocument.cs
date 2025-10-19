using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using Zeiterfassung.Reports.Data;

namespace Zeiterfassung.Reports
{
    public class MonatsnachweisDocument : IDocument
    {
        private readonly List<ZeiterfassungsReportModel> _zeiten;
        private readonly string _mitarbeiterName;
        private readonly string _berichtszeitraum;

        public MonatsnachweisDocument(List<ZeiterfassungsReportModel> zeiten, string mitarbeiterName, string berichtszeitraum)
        {
            _zeiten = zeiten;
            _mitarbeiterName = mitarbeiterName;
            _berichtszeitraum = berichtszeitraum;
        }

        public DocumentMetadata GetMetadata() => DocumentMetadata.Default;

        public void Compose(IDocumentContainer container)
        {
            container.Page(page =>
            {
                // Allgemeine Seiteneinstellungen
                page.Margin(50);
                page.DefaultTextStyle(x => x.FontSize(12));

                // 1. Header der Seite
                page.Header().Element(ComposeHeader);

                // 2. Inhalt der Seite
                page.Content().Element(ComposeContent);

                // 3. Fußzeile der Seite
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
                column.Item().Text($"Monatsnachweis für {_mitarbeiterName}")
                    .SemiBold().FontSize(20);

                column.Item().Text(_berichtszeitraum)
                    .FontSize(15);

                column.Item().PaddingTop(20).BorderBottom(1).BorderColor(Colors.Grey.Lighten1);
            });
        }

        void ComposeContent(IContainer container)
        {
            // gruppieren der Zeiteinträge nach Datum
            var groupedZeiten = _zeiten
                .GroupBy(z => z.Datum.Date)
                .OrderBy(g => g.Key);

            container.PaddingTop(20).Table(table =>
            {
                // Spaltendefinition mit vier Spalten
                table.ColumnsDefinition(columns =>
                {
                    columns.ConstantColumn(80);    
                    columns.RelativeColumn(3);     
                    columns.RelativeColumn(5);     
                    columns.ConstantColumn(60);    
                });

                // Tabellenüberschriften mit Hintergrundfarbe und fetter Schrift
                table.Header(header =>
                {
                    // Eigene Stildefinition für die Kopfzeile
                    header.Cell().BorderBottom(1).Background(Colors.Grey.Lighten3).Padding(5).Text("Datum").SemiBold();
                    header.Cell().BorderBottom(1).Background(Colors.Grey.Lighten3).Padding(5).Text("Projekt").SemiBold();
                    header.Cell().BorderBottom(1).Background(Colors.Grey.Lighten3).Padding(5).Text("Tätigkeit").SemiBold();
                    header.Cell().BorderBottom(1).Background(Colors.Grey.Lighten3).Padding(5).AlignRight().Text("Dauer").SemiBold();
                });

                // Äußere Schleife für jede Tagesgruppe
                foreach (var group in groupedZeiten)
                {
                    // Die Datumszelle erstreckt sich über alle Einträge des Tages (Border unterhalb aller Tageseinträge)
                    table.Cell().RowSpan((uint)group.Count()).BorderBottom(1).BorderColor(Colors.Grey.Lighten2).Padding(5).Text(group.Key.ToString("dd.MM.yyyy"));

                    // Innere Schleife für jeden einzelnen Eintrag innerhalb des Tages (Border unterhalb jedes Eintrags bei den Spalten "Projekt", "Tätigkeit" und "Dauer".)
                    var isFirstInGroup = true;
                    foreach (var item in group)
                    {
                        table.Cell().BorderBottom(1).BorderColor(Colors.Grey.Lighten2).Padding(5).Text(item.ProjektName ?? "-");
                        table.Cell().BorderBottom(1).BorderColor(Colors.Grey.Lighten2).Padding(5).Text(item.Taetigkeit ?? "-");
                        table.Cell().BorderBottom(1).BorderColor(Colors.Grey.Lighten2).Padding(5).AlignRight().Text($"{item.Dauer:hh\\:mm}");

                        isFirstInGroup = false;
                    }
                }

                // Beginn-Summary
                var gesamtDauer = new TimeSpan(_zeiten.Sum(z => z.Dauer.Ticks));
                var totalHoursDecimal = gesamtDauer.TotalHours;

                // Summe für Stunden im HH:mm Format
                table.Cell().ColumnSpan(3).BorderTop(1).Padding(5).AlignRight().Text("Gesamtstunden (HH:mm):").Bold();
                table.Cell().BorderTop(1).Padding(5).AlignRight().Text($"{Math.Floor(gesamtDauer.TotalHours)}:{(gesamtDauer.Minutes):00}").Bold();

                // Summe als Dezimalzahl für die Rechnungsstellung
                table.Cell().ColumnSpan(3).Padding(5).AlignRight().Text("Gesamtstunden (Dezimal):").Bold();
                table.Cell().Padding(5).AlignRight().Text($"{totalHoursDecimal:F2} h").Bold();
            });
        }
    }
}