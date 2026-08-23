namespace QuickPay.BLL.Services.Interfaces
{
    public interface IOtpDeliveryService
    {
        Task SendAsync(
            string phoneNumber,
            string code,
            CancellationToken cancellationToken = default);
    }
}
