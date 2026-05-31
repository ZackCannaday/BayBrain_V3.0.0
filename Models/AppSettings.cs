namespace BayBrain.Models
{
    /// <summary>
    /// Persisted application settings — stored in settings.json beside the exe.
    /// </summary>
    public class AppSettings
    {
        // ── Dealership ────────────────────────────────────────────────────
        public string DealershipName     { get; set; } = "Bay Auto Group";
        public string DealershipLocation { get; set; } = "Augusta, GA";
        public string DealershipPhone    { get; set; } = string.Empty;

        // ── Advisor defaults ──────────────────────────────────────────────
        public string DefaultAdvisorId        { get; set; } = string.Empty;
        public string DefaultAdvisorSignature { get; set; } = string.Empty;

        // ── Display preferences ───────────────────────────────────────────
        public bool ShowPricesInSearch    { get; set; } = true;
        public bool AutoGenerateScript    { get; set; } = true;
        public bool ShowUrgencyInBrowser  { get; set; } = true;
        public bool ShowMileageWarnings   { get; set; } = true;

        // ── RO behaviour ─────────────────────────────────────────────────
        public bool AutoSaveROsOnClose        { get; set; } = true;
        public int  DefaultUrgencyThreshold   { get; set; } = 5;

        // ── Quiz settings ─────────────────────────────────────────────────
        public int  DefaultQuizQuestionCount  { get; set; } = 10;
        public bool SaveQuizResultsToProfile  { get; set; } = true;
        public bool PlayQuizSounds            { get; set; } = true;

        // ── Print / export ────────────────────────────────────────────────
        public bool   ConfirmBeforePrint { get; set; } = false;
        public string ExportFolder       { get; set; } = string.Empty;

        // ── Metadata ──────────────────────────────────────────────────────
        public DateTime LastBackup  { get; set; } = DateTime.MinValue;
        public string   AppVersion  { get; set; } = "3.0.0";
    }
}
