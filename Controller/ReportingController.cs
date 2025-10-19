using Microsoft.AspNetCore.Mvc;
using Zeiterfassung.Data;
using Zeiterfassung.Reports.Data;
using QuestPDF.Fluent;
using Zeiterfassung.Reports;
using Microsoft.EntityFrameworkCore;

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
        QuestPDF.Settings.License = QuestPDF.Infrastructure.LicenseType.Community; // set QuestPdf license

        // load time entries for the specified employee and month
        var vonDatum = new DateTime(jahr, monat, 1);
        var mitarbeiter = await _context.Users.FirstOrDefaultAsync(x => x.Id == mitarbeiterId);
        if (mitarbeiter == null) return NotFound("Mitarbeiter nicht gefunden.");

        var zeiten = _context.TimeEntries
            .Where(z => z.UserId == mitarbeiterId && z.StartTime.Date >= vonDatum.Date && z.StartTime.Date <= vonDatum.AddMonths(1).AddDays(-1))
            .Select(z => new MonatsnachweisReportModel
            {
                Datum = z.StartTime.Date,
                ProjektName = z.Project.Name,
                Taetigkeit = z.Description,
                Dauer = z.EndTime - z.StartTime
            })
            .OrderBy(z => z.Datum)
            .ToList();

        var document = new MonatsnachweisDocument(
            zeiten,
            $"{mitarbeiter.UserName}",
            vonDatum.ToString("MMMM yyyy")
        );

        var pdfBytes = document.GeneratePdf();
        string fileName = $"Monatsnachweis_{mitarbeiter.UserName}_{jahr}-{monat:00}.pdf";
        return File(pdfBytes, "application/pdf", fileName);
    }


    [HttpGet("projektnachweis")]
    public async Task<IActionResult> GetProjektnachweis([FromQuery] int projektId, [FromQuery] DateTime vonDatum, [FromQuery] DateTime bisDatum)
    {
        QuestPDF.Settings.License = QuestPDF.Infrastructure.LicenseType.Community; // set QuestPdf license

        var projekt = await _context.Projects.FirstOrDefaultAsync(p => p.Id == projektId);
        if (projekt == null)
        {
            return NotFound("Projekt nicht gefunden.");
        }

        var zeiten = await _context.TimeEntries
            .Where(z => z.ProjectId == projektId && z.StartTime.Date >= vonDatum.Date && z.StartTime.Date <= bisDatum.Date)
            .Select(z => new ProjektnachweisReportModel
            {
                Datum = z.StartTime.Date,
                MitarbeiterName = z.User.NormalizedUserName,
                Taetigkeit = z.Description,
                Dauer = z.EndTime - z.StartTime
            })
            .OrderBy(z => z.Datum)
            .ToListAsync();

        var leistungszeitraum = $"{vonDatum:dd.MM.yyyy} - {bisDatum:dd.MM.yyyy}";

        var document = new ProjektnachweisDocument(
            zeiten,
            projekt.Name,
            leistungszeitraum
        );

        var pdfBytes = document.GeneratePdf();
        string fileName = $"Projektnachweis_{projekt.Name}_{vonDatum:yyyy-MM-dd}.pdf";
        return File(pdfBytes, "application/pdf", fileName);
    }

    [HttpGet("gesamtauswertung")]
    public async Task<IActionResult> GetGesamtauswertung([FromQuery] DateTime vonDatum, [FromQuery] DateTime bisDatum)
    {
        QuestPDF.Settings.License = QuestPDF.Infrastructure.LicenseType.Community;

        // load time entries within the date range
        var zeiten = await _context.TimeEntries
            .Include(z => z.Project)
            .Include(z => z.User)
            .Where(z => z.StartTime.Date >= vonDatum.Date && z.StartTime.Date <= bisDatum.Date)
            .ToListAsync();

        // Group by project
        var projektdaten = zeiten
            .GroupBy(z => z.Project)
            .Select(g =>
            {
                var topMitarbeiter = g
                    .GroupBy(e => e.User)
                    .Select(ug => new
                    {
                        User = ug.Key.NormalizedUserName,
                        TotalDuration = new TimeSpan(ug.Sum(e => (e.EndTime - e.StartTime).Ticks))
                    })
                    .OrderByDescending(x => x.TotalDuration)
                    .FirstOrDefault();

                return new GesamtauswertungReportModel
                {
                    ProjektName = g.Key.Name,
                    GesamtDauer = new TimeSpan(g.Sum(e => (e.EndTime - e.StartTime).Ticks)),
                    AnzahlMitarbeiter = g.Select(e => e.UserId).Distinct().Count(),
                    TopMitarbeiter = topMitarbeiter != null ? $"{topMitarbeiter.User} ({Math.Floor(topMitarbeiter.TotalDuration.TotalHours)}:{(topMitarbeiter.TotalDuration.Minutes):00}h)" : "-"
                };
            })
            .OrderByDescending(p => p.GesamtDauer)
            .ToList();

        var zeitraum = $"{vonDatum:dd.MM.yyyy} - {bisDatum:dd.MM.yyyy}";

        var document = new GesamtauswertungDocument(projektdaten, zeitraum);

        var pdfBytes = document.GeneratePdf();
        string fileName = $"Gesamtauswertung_{vonDatum:yyyy-MM-dd}_{bisDatum:yyyy-MM-dd}.pdf";
        return File(pdfBytes, "application/pdf", fileName);
    }

}