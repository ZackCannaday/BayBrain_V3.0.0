using System.Text.Json.Serialization;

namespace BayBrain.Models
{
    public enum UserRole
    {
        Advisor,
        Manager,
        Admin
    }

    public class UserAccount
    {
        public string Id { get; set; } = Guid.NewGuid().ToString();
        public string Username { get; set; } = string.Empty;
        public string DisplayName { get; set; } = string.Empty;
        public UserRole Role { get; set; } = UserRole.Advisor;
        public string AdvisorProfileId { get; set; } = string.Empty;
        public string PinHash { get; set; } = string.Empty;
        public string PinSalt { get; set; } = string.Empty;
        public bool IsActive { get; set; } = true;
        public DateTime CreatedAt { get; set; } = DateTime.Now;
        public DateTime LastLoginAt { get; set; } = DateTime.MinValue;

        [JsonIgnore]
        public bool CanAccessSettings => IsActive && Role is UserRole.Admin or UserRole.Manager;

        [JsonIgnore]
        public string RoleLabel => Role.ToString();

        [JsonIgnore]
        public string Initials
        {
            get
            {
                var name = string.IsNullOrWhiteSpace(DisplayName) ? Username : DisplayName;
                var parts = name.Split(' ', StringSplitOptions.RemoveEmptyEntries);
                if (parts.Length >= 2) return $"{parts[0][0]}{parts[^1][0]}".ToUpperInvariant();
                return name.Length > 0 ? name[..Math.Min(2, name.Length)].ToUpperInvariant() : "?";
            }
        }
    }
}
