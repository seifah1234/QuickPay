using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using QuickPay.BLL.Services.Interfaces;
using QuickPay.BLL.Settings;
using System.Diagnostics;
using Twilio;
using Twilio.Rest.Api.V2010.Account;
using Twilio.Types;

namespace QuickPay.BLL.Services.Implementation
{
    public class TwilioOtpDeliveryService : IOtpDeliveryService
    {
        private readonly TwilioSettings _settings;
        private readonly ILogger<TwilioOtpDeliveryService> _logger;
        private static bool _twilioInitialized;
        private static readonly object InitLock = new();

        public TwilioOtpDeliveryService(
            IOptions<TwilioSettings> options,
            ILogger<TwilioOtpDeliveryService> logger)
        {
            _settings = options.Value;
            _logger = logger;
        }

        public async Task SendAsync(
            string phoneNumber,
            string code,
            CancellationToken cancellationToken = default)
        {
            if (!_settings.IsConfigured)
            {
                _logger.LogWarning(
                    "[OTP] Twilio credentials are not configured yet (see .env.example) - " +
                    "falling back to console output. Code {Code} for {PhoneNumber} " +
                    "(would be sent via real SMS once Twilio__AccountSid/AuthToken/FromPhoneNumber are set).",
                    code,
                    phoneNumber);
                Console.WriteLine(
                    "[OTP] Twilio credentials are not configured yet (see .env.example) - " +
                    $"falling back to console output. Code {code} for {phoneNumber} " +
                    "(would be sent via real SMS once Twilio__AccountSid/AuthToken/FromPhoneNumber are set).");
                Debug.WriteLine(
                    "[OTP] Twilio credentials are not configured yet (see .env.example) - " +
                    $"falling back to console output. Code {code} for {phoneNumber} " +
                    "(would be sent via real SMS once Twilio__AccountSid/AuthToken/FromPhoneNumber are set).");

                return;
            }

            EnsureTwilioInitialized();

            try
            {
                await MessageResource.CreateAsync(
                    body: $"Your QuickPay verification code is {code}. It expires in 5 minutes.",
                    from: new PhoneNumber(_settings.FromPhoneNumber),
                    to: new PhoneNumber(phoneNumber));

                _logger.LogInformation(
                    "[OTP] SMS sent via Twilio to {PhoneNumber}.",
                    phoneNumber);
            }
            catch (Exception ex)
            {
                // Don't let a Twilio outage/bad-number error surface a raw
                // exception to the user - log it and let the caller's own
                // "we sent you a code" message stand; the OTP row still
                // exists so a resend/support flow can retry delivery.
                _logger.LogError(
                    ex,
                    "[OTP] Failed to send SMS via Twilio to {PhoneNumber}.",
                    phoneNumber);
            }
        }

        private void EnsureTwilioInitialized()
        {
            if (_twilioInitialized)
            {
                return;
            }

            lock (InitLock)
            {
                if (_twilioInitialized)
                {
                    return;
                }

                TwilioClient.Init(_settings.AccountSid, _settings.AuthToken);
                _twilioInitialized = true;
            }
        }
    }
}
