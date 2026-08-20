using Microsoft.Extensions.Logging;
using QuickPay.BLL.Services.Interfaces;

namespace QuickPay.BLL.Services.Implementation
{
    public class LogOtpDeliveryService : IOtpDeliveryService
    {
        private readonly ILogger<LogOtpDeliveryService> _logger;

        public LogOtpDeliveryService(ILogger<LogOtpDeliveryService> logger)
        {
            _logger = logger;
        }

        public Task SendAsync(
            string phoneNumber,
            string code,
            CancellationToken cancellationToken = default)
        {
            _logger.LogInformation(
                "[DEV OTP] Code {Code} for {PhoneNumber} (would be sent via SMS in production)",
                code,
                phoneNumber);

            return Task.CompletedTask;
        }
    }
}
