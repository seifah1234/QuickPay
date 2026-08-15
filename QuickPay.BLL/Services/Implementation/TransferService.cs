using Microsoft.EntityFrameworkCore;
using QuickPay.BLL.DTOs;
using QuickPay.BLL.Services.Interfaces;
using QuickPay.DAL.Entities;
using QuickPay.DAL.Enums;
using QuickPay.DAL.UnitOfWork;
using System;
using System.Collections.Generic;
using System.Text;

namespace QuickPay.BLL.Services.Implementation
{
    public class TransferService : ITransferService
    {
        private readonly IUnitOfWork _unitOfWork;

        public TransferService(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<TransferResultDto> TransferAsync(
            TransferRequestDto request,
            CancellationToken cancellationToken = default)
        {
            if (request.Amount <= 0)
            {
                throw new ArgumentException(
                    "Transfer amount must be greater than zero.");
            }

            if (request.FromAccountId == request.ToAccountId)
            {
                throw new ArgumentException(
                    "Source and destination accounts must be different.");
            }

            // Authorization: the caller must actually own (or be a member
            // of, for shared wallets) the source account. Without this
            // check, any authenticated user could pass any FromAccountId
            // and drain someone else's balance.
            var isAuthorized =
                await _unitOfWork.FinancialAccounts
                    .IsUserAuthorizedForAccountAsync(
                        request.FromAccountId,
                        request.CurrentUserId,
                        cancellationToken);

            if (!isAuthorized)
            {
                throw new UnauthorizedAccessException(
                    "You are not authorized to transfer from this account.");
            }

            await _unitOfWork.BeginTransactionAsync(cancellationToken);

            try
            {
                var fromAccount =
                    await _unitOfWork.FinancialAccounts
                        .GetByIdAsync(request.FromAccountId, cancellationToken);

                var toAccount =
                    await _unitOfWork.FinancialAccounts
                        .GetByIdAsync(request.ToAccountId, cancellationToken);

                if (fromAccount is null)
                {
                    throw new KeyNotFoundException(
                        "Source account was not found.");
                }

                if (toAccount is null)
                {
                    throw new KeyNotFoundException(
                        "Destination account was not found.");
                }

                if (!fromAccount.IsActive)
                {
                    throw new InvalidOperationException(
                        "Source account is inactive.");
                }

                if (!toAccount.IsActive)
                {
                    throw new InvalidOperationException(
                        "Destination account is inactive.");
                }

                if (fromAccount.Balance < request.Amount)
                {
                    throw new InvalidOperationException(
                        "Insufficient balance.");
                }

                fromAccount.Balance -= request.Amount;
                toAccount.Balance += request.Amount;

                var transaction = new Transaction
                {
                    FromAccountId = request.FromAccountId,
                    ToAccountId = request.ToAccountId,
                    Amount = request.Amount,
                    Type = TransactionType.Transfer,
                    Status = TransactionStatus.Completed,
                    CreatedAt = DateTime.UtcNow
                };

                await _unitOfWork.Transactions.AddAsync(transaction);

                try
                {
                    await _unitOfWork.SaveChangesAsync(cancellationToken);
                }
                catch (DbUpdateConcurrencyException)
                {
                    // Someone else updated fromAccount/toAccount's
                    // RowVersion between our read and this write - the
                    // in-memory balances above are stale. Fail the whole
                    // transfer rather than silently overwriting the
                    // other update; the caller can safely retry.
                    throw new InvalidOperationException(
                        "One of the accounts was updated at the same time by another operation. Please try again.");
                }

                await _unitOfWork.CommitTransactionAsync(
                    cancellationToken);

                return new TransferResultDto
                {
                    IsSuccess = true,
                    Message = "Transfer completed successfully.",
                    TransactionId = transaction.Id,
                    Amount = transaction.Amount,
                    FromAccountId = transaction.FromAccountId,
                    ToAccountId = transaction.ToAccountId,
                    Status = transaction.Status.ToString(),
                    CreatedAt = transaction.CreatedAt
                };
            }
            catch
            {
                await _unitOfWork.RollbackTransactionAsync(
                    cancellationToken);

                throw;
            }
        }
    }
}
