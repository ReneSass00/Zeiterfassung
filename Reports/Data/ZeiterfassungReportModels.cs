namespace Zeiterfassung.Reports.Data
{
    public class MonatsnachweisReportModel
    {
        public DateTime Datum { get; set; }
        public string ProjektName { get; set; }
        public string Taetigkeit { get; set; }
        public TimeSpan Dauer { get; set; }
    }

    public class ProjektnachweisReportModel
    {
        public DateTime Datum { get; set; }
        public string MitarbeiterName { get; set; }
        public string Taetigkeit { get; set; }
        public TimeSpan Dauer { get; set; }
    }

    public class GesamtauswertungReportModel
    {
        public string ProjektName { get; set; }
        public TimeSpan GesamtDauer { get; set; }
        public int AnzahlMitarbeiter { get; set; }
        public string TopMitarbeiter { get; set; }
    }
}
