using QuickPay.BLL.DTOs.PaymentGateway;

namespace QuickPay.BLL.Services.Interfaces
{
    public interface ILinkedAccountsService
    {
        Task<IEnumerable<BankAccountDto>> GetMyLinkedAccountsAsync(
            int userId,
            CancellationToken cancellationToken = default);

        Task<BankAccountDto> LinkAsync(
            LinkBankAccountRequestDto request,
            CancellationToken cancellationToken = default);

        Task UnlinkAsync(
            int bankAccountId,
            int currentUserId,
            CancellationToken cancellationToken = default);
    }
}
