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
    /// Files currently live beside the executable; a later stabilization step
    /// will migrate mutable data to the user's LocalAppData directory.
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

        // ── Local Users ─────────────────────────────────────────────────

        public static List<UserAccount> LoadUsers()
            => Load<List<UserAccount>>("users.json") ?? new();

        public static bool SaveUsers(List<UserAccount> users)
            => Save("users.json", users);

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
                var id = ro.Id ?? string.Empty;
                var shortId = id[..Math.Min(8, id.Length)];

                sb.AppendLine(string.Join(",",
                    CsvEscape(shortId),
                    CsvEscape(ro.CreatedAt.ToString("yyyy-MM-dd")),
                    CsvEscape(ro.CustomerName),
                    CsvEscape(ro.VehicleLabel),
                    ro.Mileage,
                    CsvEscape(ro.AdvisorName),
                    ro.RecommendedCount,
                    ro.ApprovedCount,
                    ro.TotalPerformed.ToString("F2", System.Globalization.CultureInfo.InvariantCulture),
                    ro.ApprovalRate.ToString("F0", System.Globalization.CultureInfo.InvariantCulture) + "%"
                ));
            }
            return sb.ToString();
        }

        public static bool SaveCsvToFile(string csv, string filename)
        {
            try
            {
                var path = ResolveDataPath(filename);
                WriteTextAtomically(path, csv, System.Text.Encoding.UTF8);
                return true;
            }
            catch (Exception ex)
            {
                Warn($"Could not save {filename}: {ex.Message}");
                return false;
            }
        }

        // ── Generic load / save ──────────────────────────────────────────

        private static T? Load<T>(string filename) where T : class
        {
            var path = ResolveDataPath(filename);
            if (!File.Exists(path)) return null;

            var primary = TryDeserialize<T>(path, filename);
            if (primary != null) return primary;

            var backupPath = GetBackupPath(path);
            if (!File.Exists(backupPath)) return null;

            var recovered = TryDeserialize<T>(backupPath, $"{filename}.bak");
            if (recovered != null)
                Warn($"{filename} was recovered from its backup copy.");

            return recovered;
        }

        private static T? TryDeserialize<T>(string path, string displayName) where T : class
        {
            try
            {
                var json = File.ReadAllText(path);
                return JsonSerializer.Deserialize<T>(json, _opts);
            }
            catch (Exception ex)
            {
                Warn($"{displayName} failed to load: {ex.Message}");
                return null;
            }
        }

        private static bool Save<T>(string filename, T data)
        {
            try
            {
                var path = ResolveDataPath(filename);
                var json = JsonSerializer.Serialize(data, _opts);

                // Validate the serialized document before replacing any valid file.
                using (JsonDocument.Parse(json)) { }

                WriteTextAtomically(path, json, System.Text.Encoding.UTF8);
                return true;
            }
            catch (Exception ex)
            {
                Warn($"Could not save {filename}: {ex.Message}");
                return false;
            }
        }

        private static void WriteTextAtomically(string path, string content, System.Text.Encoding encoding)
        {
            var directory = Path.GetDirectoryName(path);
            if (!string.IsNullOrWhiteSpace(directory))
                Directory.CreateDirectory(directory);

            var tempPath = path + ".tmp";
            var backupPath = GetBackupPath(path);

            try
            {
                using (var stream = new FileStream(
                           tempPath,
                           FileMode.Create,
                           FileAccess.Write,
                           FileShare.None,
                           bufferSize: 4096,
                           options: FileOptions.WriteThrough))
                using (var writer = new StreamWriter(stream, encoding))
                {
                    writer.Write(content);
                    writer.Flush();
                    stream.Flush(flushToDisk: true);
                }

                if (File.Exists(path))
                {
                    try
                    {
                        File.Replace(tempPath, path, backupPath, ignoreMetadataErrors: true);
                    }
                    catch (PlatformNotSupportedException)
                    {
                        File.Copy(path, backupPath, overwrite: true);
                        File.Move(tempPath, path, overwrite: true);
                    }
                    catch (IOException)
                    {
                        File.Copy(path, backupPath, overwrite: true);
                        File.Move(tempPath, path, overwrite: true);
                    }
                }
                else
                {
                    File.Move(tempPath, path);
                }
            }
            finally
            {
                if (File.Exists(tempPath))
                    File.Delete(tempPath);
            }
        }

        private static string GetBackupPath(string path) => path + ".bak";

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

            return Path.Combine(exeDir, filename);   // temporary default until LocalAppData migration
        }

        private static string CsvEscape(string? s)
        {
            s ??= string.Empty;
            return s.Contains(',') || s.Contains('"') || s.Contains('\n') || s.Contains('\r')
                ? $"\"{s.Replace("\"", "\"\"")}\"" : s;
        }

        private static void Warn(string msg)
            => System.Diagnostics.Debug.WriteLine($"[DataLoader] WARNING: {msg}");
    }
}
