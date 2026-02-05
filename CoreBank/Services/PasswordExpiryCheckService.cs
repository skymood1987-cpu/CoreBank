// Services/PasswordExpiryCheckService.cs
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using MinCoreBank.Models;

namespace MinCoreBank.Services
{
    public class PasswordExpiryCheckService : IHostedService, IDisposable
    {
        private readonly ILogger<PasswordExpiryCheckService> _logger;
        private readonly IServiceProvider _serviceProvider;
        private Timer? _timer;

        public PasswordExpiryCheckService(ILogger<PasswordExpiryCheckService> logger, IServiceProvider serviceProvider)
        {
            _logger = logger;
            _serviceProvider = serviceProvider;
        }

        public Task StartAsync(CancellationToken cancellationToken)
        {
            _logger.LogInformation("Password Expiry Check Service started.");

            // Run immediately on startup
            CheckPasswords();

            // Then run every 24 hours
            _timer = new Timer(CheckPasswords, null, TimeSpan.Zero, TimeSpan.FromHours(24));

            return Task.CompletedTask;
        }

        private async void CheckPasswords(object? state = null)
        {
            try
            {
                using (var scope = _serviceProvider.CreateScope())
                {
                    var userService = scope.ServiceProvider.GetRequiredService<IUserService>();

                    // Get users with passwords expiring in 7 days
                    var result = await userService.GetUsersWithExpiringPasswords(7);

                    if (result.Success && result.Data != null)
                    {
                        foreach (var user in result.Data)
                        {
                            var daysUntilExpiry = (user.LastPasswordChange.AddDays(90) - DateTime.UtcNow).Days;

                            if (daysUntilExpiry <= 7 && daysUntilExpiry > 0)
                            {
                                _logger.LogWarning($"User {user.Name_en} (ID: {user.Id}) password expires in {daysUntilExpiry} days.");

                                // Here you can add email notification or other alerts
                                // For now, we just log it
                            }
                            else if (daysUntilExpiry <= 0)
                            {
                                // Password already expired, force change
                                await userService.RequirePasswordChange(user.Id);
                                _logger.LogWarning($"User {user.Name_en} (ID: {user.Id}) password expired. Forcing change on next login.");
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error checking password expiry");
            }
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            _logger.LogInformation("Password Expiry Check Service stopped.");

            _timer?.Change(Timeout.Infinite, 0);
            return Task.CompletedTask;
        }

        public void Dispose()
        {
            _timer?.Dispose();
        }
    }
}