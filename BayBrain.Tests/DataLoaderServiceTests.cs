using BayBrain.Models;
using BayBrain.Services;
using System.Text.Json;

namespace BayBrain.Tests;

public class DataLoaderServiceTests
{
    [Fact]
    public void ExportAllToCsv_ShortRepairOrderId_DoesNotThrow()
    {
        var orders = new List<RepairOrder>
        {
            new()
            {
                Id = "A12",
                Year = 2018,
                Make = "Ford",
                Model = "Focus",
                Mileage = 128000,
                CustomerName = "Test Customer",
                AdvisorName = "Test Advisor"
            }
        };

        var csv = DataLoaderService.ExportAllToCsv(orders);

        Assert.Contains("A12", csv);
        Assert.Contains("2018 Ford Focus", csv);
    }

    [Fact]
    public void ExportAllToCsv_UsesInvariantDecimalFormatting()
    {
        var orders = new List<RepairOrder>
        {
            new()
            {
                Id = "12345678",
                RecommendedServices = new List<RecommendedService>
                {
                    new() { ServiceId = "oil", ServiceName = "Oil Change" }
                },
                PerformedServices = new List<PerformedService>
                {
                    new() { ServiceId = "oil", ServiceName = "Oil Change", ActualPrice = 149.95m }
                }
            }
        };

        var csv = DataLoaderService.ExportAllToCsv(orders);

        Assert.Contains("149.95", csv);
    }

    [Fact]
    public void SaveSettings_ReplacesPrimaryAndCreatesBackupWithoutLeavingTempFile()
    {
        var primaryPath = Path.Combine(AppContext.BaseDirectory, "settings.json");
        using var files = new FileStateScope(primaryPath);

        var original = new AppSettings { DealershipName = "Original Store" };
        File.WriteAllText(primaryPath, JsonSerializer.Serialize(original));

        var saved = DataLoaderService.SaveSettings(
            new AppSettings { DealershipName = "Updated Store" });

        Assert.True(saved);
        Assert.False(File.Exists(primaryPath + ".tmp"));
        Assert.True(File.Exists(primaryPath + ".bak"));

        var current = JsonSerializer.Deserialize<AppSettings>(File.ReadAllText(primaryPath));
        var backup = JsonSerializer.Deserialize<AppSettings>(File.ReadAllText(primaryPath + ".bak"));

        Assert.NotNull(current);
        Assert.NotNull(backup);
        Assert.Equal("Updated Store", current.DealershipName);
        Assert.Equal("Original Store", backup.DealershipName);
    }

    [Fact]
    public void LoadSettings_CorruptPrimary_RecoversFromBackup()
    {
        var primaryPath = Path.Combine(AppContext.BaseDirectory, "settings.json");
        using var files = new FileStateScope(primaryPath);

        File.WriteAllText(primaryPath, "{ invalid json");
        File.WriteAllText(
            primaryPath + ".bak",
            JsonSerializer.Serialize(new AppSettings { DealershipName = "Recovered Store" }));

        var loaded = DataLoaderService.LoadSettings();

        Assert.Equal("Recovered Store", loaded.DealershipName);
    }

    private sealed class FileStateScope : IDisposable
    {
        private readonly Dictionary<string, byte[]?> _originalFiles;

        public FileStateScope(string primaryPath)
        {
            _originalFiles = new[]
                {
                    primaryPath,
                    primaryPath + ".bak",
                    primaryPath + ".tmp"
                }
                .ToDictionary(
                    path => path,
                    path => File.Exists(path) ? File.ReadAllBytes(path) : null);

            foreach (var path in _originalFiles.Keys)
                File.Delete(path);
        }

        public void Dispose()
        {
            foreach (var (path, content) in _originalFiles)
            {
                File.Delete(path);
                if (content != null)
                    File.WriteAllBytes(path, content);
            }
        }
    }
}
