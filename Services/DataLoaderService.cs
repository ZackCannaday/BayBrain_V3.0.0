using BayBrain.Models;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace BayBrain.Services
{
    /// <summary>
    /// Loads and saves all persistent data — services, quiz questions,
    /// advisor profiles, repair orders, and app settings.
    /// Files live beside the executable; no recompile needed to edit content.
    /// </summary>
    public static class DataLoaderService
    {
        private static readonly JsonSerializerOptions _opts = new()
        {
            PropertyNameCaseInsensitive = true,
            DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
            ReadCommentHandling = JsonCommentHandling.Skip,
            AllowTrailingCommas = true,
            WriteIndented = true
        };

        // ── Services ────────────────────────────────────────────────────

        public static List<ServiceItem> LoadServices()
            => Load<List<ServiceItem>>("services.json") ?? Data.ServiceDatabase.GetAll();

        public static bool SaveServices(List<ServiceItem> services)
            => Save("services.json", services);

        // ── Quiz Questions ───────────────────────────────────────────────

        public static List<QuizQuestion> LoadQuizQuestions()
            => Load<List<QuizQuestion>>("quiz.json") ?? Data.QuizDatabase.GetAll();

        // Alias used by MainViewModel for count stats
        public static List<QuizQuestion> LoadQuiz()
            => LoadQuizQuestions();

        // ── Advisor Profiles ─────────────────────────────────────────────

        public static List<AdvisorProfile> LoadProfiles()
            => Load<List<AdvisorProfile>>("advisor_profiles.json") ?? new();

        public static bool SaveProfiles(List<AdvisorProfile> profiles)
            => Save("advisor_profiles.json", profiles);

        // ── Repair Orders ────────────────────────────────────────────────

        public static List<RepairOrder> LoadRepairOrders()
            => Load<List<RepairOrder>>("repair_orders.json") ?? new();

        public static bool SaveRepairOrders(List<RepairOrder> orders)
            => Save("repair_orders.json", orders);

        // ── App Settings ─────────────────────────────────────────────────

        public static AppSettings LoadSettings()
            => Load<AppSettings>("settings.json") ?? new AppSettings();

        public static bool SaveSettings(AppSettings settings)
            => Save("settings.json", settings);

        // ── Export helpers ───────────────────────────────────────────────

        public static string ExportAllToCsv(List<RepairOrder> orders)
        {
            var sb = new System.Text.StringBuilder();
            sb.AppendLine("RO ID,Date,Customer,Vehicle,Mileage,Advisor,Recommended,Performed,Total,Approval Rate");
            foreach (var ro in orders)
            {
                sb.AppendLine(string.Join(",",
                    CsvEscape(ro.Id[..8]),
                    CsvEscape(ro.CreatedAt.ToString("yyyy-MM-dd")),
                    CsvEscape(ro.CustomerName),
                    CsvEscape(ro.VehicleLabel),
                    ro.Mileage,
                    CsvEscape(ro.AdvisorName),
                    ro.RecommendedCount,
                    ro.ApprovedCount,
                    ro.TotalPerformed.ToString("F2"),
                    ro.ApprovalRate.ToString("F0") + "%"
                ));
            }
            return sb.ToString();
        }

        public static bool SaveCsvToFile(string csv, string filename)
        {
            try
            {
                var path = ResolveDataPath(filename);
                File.WriteAllText(path, csv, System.Text.Encoding.UTF8);
                return true;
            }
            catch { return false; }
        }

        // ── Generic load / save ──────────────────────────────────────────

        private static T? Load<T>(string filename) where T : class
        {
            var path = ResolveDataPath(filename);
            if (!File.Exists(path)) return null;
            try
            {
                var json = File.ReadAllText(path);
                return JsonSerializer.Deserialize<T>(json, _opts);
            }
            catch (Exception ex)
            {
                Warn($"{filename} failed to load: {ex.Message}");
                return null;
            }
        }

        private static bool Save<T>(string filename, T data)
        {
            try
            {
                var path = ResolveDataPath(filename);
                File.WriteAllText(path, JsonSerializer.Serialize(data, _opts));
                return true;
            }
            catch (Exception ex)
            {
                Warn($"Could not save {filename}: {ex.Message}");
                return false;
            }
        }

        private static string ResolveDataPath(string filename)
        {
            var exeDir = AppContext.BaseDirectory;
            foreach (var candidate in new[]
            {
                Path.Combine(exeDir, filename),
                Path.Combine(exeDir, "Data", filename),
                Path.Combine(Directory.GetCurrentDirectory(), filename)
            })
                if (File.Exists(candidate)) return candidate;

            return Path.Combine(exeDir, filename);   // default write location
        }

        private static string CsvEscape(string? s)
        {
            s ??= string.Empty;
            return s.Contains(',') || s.Contains('"') || s.Contains('\n')
                ? $"\"{s.Replace("\"", "\"\"")}\"" : s;
        }

        private static void Warn(string msg)
            => System.Diagnostics.Debug.WriteLine($"[DataLoader] WARNING: {msg}");
    }
}
