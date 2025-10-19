using Microsoft.AspNetCore.Mvc;
using Zeiterfassung.Data;
using Zeiterfassung.Reports.Data;
using QuestPDF.Fluent; // Wichtig!
using Zeiterfassung.Reports;
using Microsoft.EntityFrameworkCore; // Namespace deiner Report-Klasse

[Route("api/[controller]")]
[ApiController]
public class ReportingController : ControllerBase
{
    private readonly ZeiterfassungContext _context;

    public ReportingController(ZeiterfassungContext context)
    {
        _context = context;
    }

    [HttpGet("monatsnachweis")]
    public async Task<IActionResult> GetMonatsnachweis([FromQuery] string mitarbeiterId, [FromQuery] int jahr, [FromQuery] int monat)
    {
        // 1. Lizenz für den Community-Modus setzen (einmalig)
        QuestPDF.Settings.License = QuestPDF.Infrastructure.LicenseType.Community;

        // 2. Daten laden
        var vonDatum = new DateTime(jahr, monat, 1);
        var mitarbeiter = await _context.Users.FirstOrDefaultAsync(x => x.Id == mitarbeiterId);
        if (mitarbeiter == null) return NotFound("Mitarbeiter nicht gefunden.");

        var zeiten =  _context.TimeEntries
            .Where(z => z.UserId == mitarbeiterId && z.StartTime.Date >= vonDatum.Date && z.StartTime.Date <= vonDatum.AddMonths(1).AddDays(-1))
            .Select(z => new ZeiterfassungsReportModel
            {
                Datum = z.StartTime.Date,
                ProjektName = z.Project.Name,
                Taetigkeit = z.Description,
                Dauer = z.EndTime - z.StartTime
            })
            .OrderBy(z => z.Datum)
            .ToList();

        // 3. Report-Objekt erstellen und Daten übergeben
        var document = new MonatsnachweisDocument(
            zeiten,
            $"{mitarbeiter.UserName}",
            vonDatum.ToString("MMMM yyyy")
        );

        // 4. PDF generieren und als Datei zurückgeben
        var pdfBytes = document.GeneratePdf();
        string fileName = $"Monatsnachweis_{mitarbeiter.UserName}_{jahr}-{monat:00}.pdf";
        return File(pdfBytes, "application/pdf", fileName);
    }
}