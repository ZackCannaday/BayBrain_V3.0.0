using BayBrain.Models;
using System.Security.Cryptography;

namespace BayBrain.Services
{
    public class AuthService
    {
        private List<UserAccount> _users = new();

        public IReadOnlyList<UserAccount> Users => _users;

        public AuthService()
        {
            Load();
        }

        public void Load()
        {
            _users = DataLoaderService.LoadUsers();
            if (_users.Count == 0)
            {
                _users.Add(CreateUser("admin", "Administrator", UserRole.Admin, "0000", string.Empty));
                Save();
            }
        }

        public bool Save()
            => DataLoaderService.SaveUsers(_users);

        public UserAccount CreateUser(string username, string displayName, UserRole role, string pin, string advisorProfileId)
        {
            var salt = CreateSalt();
            return new UserAccount
            {
                Username = username.Trim(),
                DisplayName = string.IsNullOrWhiteSpace(displayName) ? username.Trim() : displayName.Trim(),
                Role = role,
                AdvisorProfileId = advisorProfileId.Trim(),
                PinSalt = salt,
                PinHash = HashPin(pin, salt)
            };
        }

        public bool AddUser(UserAccount account)
        {
            if (string.IsNullOrWhiteSpace(account.Username)) return false;
            if (_users.Any(u => string.Equals(u.Username, account.Username, StringComparison.OrdinalIgnoreCase))) return false;

            _users.Add(account);
            return Save();
        }

        public bool VerifyPin(UserAccount? account, string pin)
        {
            if (account == null || !account.IsActive) return false;
            if (string.IsNullOrWhiteSpace(account.PinHash) || string.IsNullOrWhiteSpace(account.PinSalt)) return false;

            try
            {
                var attempted = HashPin(pin, account.PinSalt);
                return CryptographicOperations.FixedTimeEquals(
                    Convert.FromBase64String(account.PinHash),
                    Convert.FromBase64String(attempted));
            }
            catch (FormatException)
            {
                return false;
            }
            catch (CryptographicException)
            {
                return false;
            }
        }

        public bool RecordLogin(UserAccount account)
        {
            account.LastLoginAt = DateTime.Now;
            return Save();
        }

        private static string CreateSalt()
        {
            var salt = RandomNumberGenerator.GetBytes(16);
            return Convert.ToBase64String(salt);
        }

        private static string HashPin(string pin, string salt)
        {
            var saltBytes = Convert.FromBase64String(salt);
            var hash = Rfc2898DeriveBytes.Pbkdf2(
                pin,
                saltBytes,
                100_000,
                HashAlgorithmName.SHA256,
                32);
            return Convert.ToBase64String(hash);
        }
    }
}
