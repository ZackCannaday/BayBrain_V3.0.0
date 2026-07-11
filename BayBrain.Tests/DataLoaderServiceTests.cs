using BayBrain.Models;
using BayBrain.Services;

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
}
