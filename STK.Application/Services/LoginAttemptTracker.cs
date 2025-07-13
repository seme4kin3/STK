using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;


namespace STK.Application.Services
{
    public class LoginAttemptTracker
    {
        private readonly IMemoryCache _cache;
        private readonly ILogger<LoginAttemptTracker> _logger;

        public LoginAttemptTracker(IMemoryCache cache, ILogger<LoginAttemptTracker> logger)
        {
            _cache = cache ?? throw new ArgumentNullException(nameof(cache));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public Task<bool> IsLockedOutAsync(string email)
        {
            if (string.IsNullOrEmpty(email))
                throw new ArgumentException("Email cannot be null or empty", nameof(email));

            var key = $"login_attempts_{email}";
            var attempts = _cache.Get<int>(key); // Указываем тип int

            if (attempts >= 5) // Максимум 5 попыток
            {
                _logger.LogWarning("Account {Email} is locked due to too many failed attempts", email);
                return Task.FromResult(true);
            }

            return Task.FromResult(false);
        }

        public void RecordFailedAttempt(string email)
        {
            var key = $"login_attempts_{email}";
            var attempts = _cache.Get<int>(key);
            _cache.Set(key, attempts + 1, TimeSpan.FromMinutes(15)); // Блокировка на 15 минут
        }

        public void ResetAttempts(string email)
        {
            var key = $"login_attempts_{email}";
            _cache.Remove(key);
        }
    }
}
