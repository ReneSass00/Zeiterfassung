namespace Zeiterfassung.Reports.Data
{
    public class ZeiterfassungsReportModel
    {
        public DateTime Datum { get; set; }
        public string ProjektName { get; set; }
        public string Taetigkeit { get; set; }
        public TimeSpan Dauer { get; set; }
    }
}
