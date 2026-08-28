using QuickPay.BLL.DTOs.PaymentGateway;
using QuickPay.BLL.Services.Interfaces;
using QuickPay.DAL.Entities;
using QuickPay.DAL.UnitOfWork;

namespace QuickPay.BLL.Services.Implementation
{
    public class LinkedAccountsService : ILinkedAccountsService
    {
        private readonly IUnitOfWork _unitOfWork;

        public LinkedAccountsService(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<IEnumerable<BankAccountDto>> GetMyLinkedAccountsAsync(
            int userId,
            CancellationToken cancellationToken = default)
        {
            var accounts = await _unitOfWork.BankAccounts
                .GetByUserIdAsync(userId, cancellationToken);

            return accounts.Select(ToDto);
        }

        public async Task<BankAccountDto> LinkAsync(
            LinkBankAccountRequestDto request,
            CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(request.GatewayToken))
            {
                throw new ArgumentException(
                    "No gateway token was provided - card details must be tokenized by the gateway's client-side SDK before this call, never sent to our server directly.");
            }

            var bankAccount = new BankAccount
            {
                UserId = request.CurrentUserId,
                DisplayName = request.DisplayName.Trim(),
                MaskedNumber = request.MaskedNumber,
                GatewayToken = request.GatewayToken,
                IsActive = true
            };

            await _unitOfWork.BankAccounts.AddAsync(
                bankAccount, cancellationToken);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            return ToDto(bankAccount);
        }

        public async Task UnlinkAsync(
            int bankAccountId,
            int currentUserId,
            CancellationToken cancellationToken = default)
        {
            var bankAccount = await _unitOfWork.BankAccounts.GetByIdAsync(
                bankAccountId, cancellationToken);

            if (bankAccount is null || bankAccount.UserId != currentUserId)
            {
                throw new UnauthorizedAccessException(
                    "You are not authorized to remove this account.");
            }

            bankAccount.IsActive = false;
            bankAccount.UpdatedAt = DateTime.UtcNow;

            await _unitOfWork.SaveChangesAsync(cancellationToken);
        }

        private static BankAccountDto ToDto(BankAccount account) => new()
        {
            Id = account.Id,
            DisplayName = account.DisplayName,
            MaskedNumber = account.MaskedNumber
        };
    }
}
