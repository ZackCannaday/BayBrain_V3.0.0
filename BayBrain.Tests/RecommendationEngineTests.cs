using BayBrain.Models;
using BayBrain.Services;
using Xunit;

namespace BayBrain.Tests
{
    public class RecommendationEngineTests
    {
        private readonly UrgencyService _urgencyService;

        public RecommendationEngineTests()
        {
            _urgencyService = new UrgencyService();
        }

        [Fact]
        public void Calculate_BaseUrgency_ReturnsCorrectScore()
        {
            // Arrange
            var service = new ServiceItem { Name = "Oil Change", BaseUrgency = 5 };

            // Act
            var result = _urgencyService.Calculate(service);

            // Assert
            Assert.Equal(5, result.Score);
            Assert.Contains("Base urgency", result.Reasoning);
        }

        [Theory]
        [InlineData(1000, 5)]   // < 2000 => no bonus
        [InlineData(2500, 6)]   // > 2000 => +1
        [InlineData(6000, 7)]   // > 5000 => +2
        [InlineData(12000, 8)]  // > 10000 => +3
        public void Calculate_MilesOverdue_AddsCorrectModifier(int milesOverdue, int expectedScore)
        {
            // Arrange
            var service = new ServiceItem { Name = "Oil Change", BaseUrgency = 5 };

            // Act
            var result = _urgencyService.Calculate(service, milesOverdue: milesOverdue);

            // Assert
            Assert.Equal(expectedScore, result.Score);
        }

        [Fact]
        public void Calculate_HasSymptoms_AddsModifier()
        {
            // Arrange
            var service = new ServiceItem { Name = "Brake Pads", BaseUrgency = 6 };

            // Act
            var result = _urgencyService.Calculate(service, hasSymptoms: true);

            // Assert
            Assert.Equal(8, result.Score); // 6 + 2
            Assert.Contains("+2 for active customer-reported symptoms", result.Reasoning);
        }

        [Fact]
        public void Calculate_MaxScore_IsCappedAt10()
        {
            // Arrange
            var service = new ServiceItem { Name = "Engine Replacement", BaseUrgency = 9 };

            // Act
            var result = _urgencyService.Calculate(service, milesOverdue: 15000, hasSymptoms: true);

            // Assert
            Assert.Equal(10, result.Score); // 9 + 3 + 2 = 14 => capped at 10
        }
    }
}
