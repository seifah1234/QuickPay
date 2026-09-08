using Microsoft.EntityFrameworkCore;
using QuickPay.BLL.DTOs.SmartSplit;
using QuickPay.BLL.Services.Interfaces;
using QuickPay.BLL.Services.SplitStrategies;
using QuickPay.DAL.Entities;
using QuickPay.DAL.Enums;
using QuickPay.DAL.UnitOfWork;

namespace QuickPay.BLL.Services.Implementation
{
    public class SmartSplitService : ISmartSplitService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ISplitStrategyFactory _splitStrategyFactory;
        private readonly INotificationService _notificationService;

        public SmartSplitService(
            IUnitOfWork unitOfWork,
            ISplitStrategyFactory splitStrategyFactory,
            INotificationService notificationService)
        {
            _unitOfWork = unitOfWork;
            _splitStrategyFactory = splitStrategyFactory;
            _notificationService = notificationService;
        }

        public async Task<IEnumerable<SplitGroupDto>> GetMySplitsAsync(
            int userId,
            CancellationToken cancellationToken = default)
        {
            var splitGroups = await _unitOfWork.SplitGroups
                .GetForUserAsync(userId, cancellationToken);

            return splitGroups.Select(ToDto);
        }

        public async Task<SplitGroupDto> GetDetailsAsync(
            int splitGroupId,
            int currentUserId,
            CancellationToken cancellationToken = default)
        {
            var splitGroup = await GetAuthorizedSplitGroupAsync(
                splitGroupId, currentUserId, cancellationToken);

            return ToDto(splitGroup);
        }

        public async Task<SplitGroupDto> CreateSplitAsync(
            CreateSplitGroupDto request,
            CancellationToken cancellationToken = default)
        {
            if (request.TotalAmount <= 0)
            {
                throw new ArgumentException(
                    "Total amount must be greater than zero.");
            }

            if (!Enum.TryParse<SplitType>(request.SplitType, out var splitType))
            {
                throw new ArgumentException(
                    $"Unknown split type: {request.SplitType}.");
            }

            var isAuthorized = await _unitOfWork.FinancialAccounts
                .IsUserAuthorizedForAccountAsync(
                    request.SettlementWalletId,
                    request.CurrentUserId,
                    cancellationToken);

            if (!isAuthorized)
            {
                throw new UnauthorizedAccessException(
                    "You are not authorized to settle into this wallet.");
            }

            if (request.Participants.Count == 0)
            {
                throw new ArgumentException(
                    "Add at least one participant to split with.");
            }

            // Resolve every identifier to an actual user up front, so a
            // typo fails the whole request instead of silently dropping
            // a participant.
            var resolvedParticipants = new List<(User User, decimal? Amount, decimal? Percentage)>();

            foreach (var participantInput in request.Participants)
            {
                if (string.IsNullOrWhiteSpace(participantInput.Identifier))
                {
                    throw new ArgumentException(
                        "Every participant needs a username, phone number, or email.");
                }

                var user = await _unitOfWork.Users.GetByIdentifierAsync(
                    participantInput.Identifier.Trim(), cancellationToken);

                if (user is null)
                {
                    throw new KeyNotFoundException(
                        $"No user found for \"{participantInput.Identifier}\".");
                }

                if (user.Id == request.CurrentUserId)
                {
                    throw new ArgumentException(
                        "You can't add yourself as a participant - you're the one collecting the money.");
                }

                resolvedParticipants.Add((user, participantInput.Amount, participantInput.Percentage));
            }

            if (resolvedParticipants.Select(p => p.User.Id).Distinct().Count() != resolvedParticipants.Count)
            {
                throw new ArgumentException("Each participant can only appear once.");
            }

            var strategy = _splitStrategyFactory.Resolve(splitType);

            var shareInputs = resolvedParticipants
                .Select(p => new SplitShareInput(p.User.Id, p.Amount, p.Percentage))
                .ToList();

            // Throws ArgumentException with a specific reason if the
            // amounts/percentages given don't add up correctly.
            var shares = strategy.Calculate(request.TotalAmount, shareInputs);

            var payment = new Payment
            {
                UserId = request.CurrentUserId,
                Amount = request.TotalAmount,
                Currency = "EGP",
                Status = PaymentStatus.Pending,
                SettlementWalletId = request.SettlementWalletId
            };

            var splitGroup = new SplitGroup
            {
                Payment = payment,
                Status = SplitGroupStatus.Pending,
                DueDate = request.DueDate,
                SplitType = splitType
            };

            foreach (var share in shares)
            {
                splitGroup.Participants.Add(new SplitParticipant
                {
                    UserId = share.UserId,
                    Amount = share.Amount,
                    Percentage = share.Percentage,
                    Status = SplitParticipantStatus.Pending
                });
            }

            await _unitOfWork.Payments.AddAsync(payment, cancellationToken);
            await _unitOfWork.SplitGroups.AddAsync(splitGroup, cancellationToken);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            foreach (var share in shares)
            {
                await _notificationService.NotifyAsync(
                    share.UserId,
                    type: "SplitRequested",
                    message: $"You've been asked to pay {share.Amount:N2} EGP" +
                        (string.IsNullOrWhiteSpace(request.Description) ? "." : $" for {request.Description}."),
                    cancellationToken);
            }

            return await GetDetailsAsync(
                splitGroup.Id, request.CurrentUserId, cancellationToken);
        }

        public async Task<SplitGroupDto> PayShareAsync(
            PaySplitShareDto request,
            CancellationToken cancellationToken = default)
        {
            var isAuthorized = await _unitOfWork.FinancialAccounts
                .IsUserAuthorizedForAccountAsync(
                    request.FromWalletId,
                    request.CurrentUserId,
                    cancellationToken);

            if (!isAuthorized)
            {
                throw new UnauthorizedAccessException(
                    "You are not authorized to pay from this wallet.");
            }

            var participant = await _unitOfWork.SplitParticipants.GetAsync(
                request.SplitGroupId, request.CurrentUserId, cancellationToken);

            if (participant is null)
            {
                throw new KeyNotFoundException(
                    "You are not a participant in this split.");
            }

            if (participant.Status == SplitParticipantStatus.Paid)
            {
                throw new InvalidOperationException(
                    "You've already paid your share.");
            }

            if (participant.SplitGroup.Status == SplitGroupStatus.Cancelled)
            {
                throw new InvalidOperationException(
                    "This split was cancelled.");
            }

            await _unitOfWork.BeginTransactionAsync(cancellationToken);

            try
            {
                var fromWallet = await _unitOfWork.FinancialAccounts
                    .GetByIdAsync(request.FromWalletId, cancellationToken);

                var settlementWallet = await _unitOfWork.FinancialAccounts
                    .GetByIdAsync(
                        participant.SplitGroup.Payment.SettlementWalletId,
                        cancellationToken);

                if (fromWallet is null || settlementWallet is null)
                {
                    throw new KeyNotFoundException("Wallet was not found.");
                }

                if (!fromWallet.IsActive)
                {
                    throw new InvalidOperationException("Your wallet is inactive.");
                }

                if (fromWallet.Balance < participant.Amount)
                {
                    throw new InvalidOperationException("Insufficient balance.");
                }

                fromWallet.Balance -= participant.Amount;
                settlementWallet.Balance += participant.Amount;

                var ledgerTransaction = new Transaction
                {
                    FromAccountId = fromWallet.Id,
                    ToAccountId = settlementWallet.Id,
                    Amount = participant.Amount,
                    Type = TransactionType.SplitPayment,
                    Status = TransactionStatus.Completed,
                    PaymentId = participant.SplitGroup.PaymentId,
                    CreatedAt = DateTime.UtcNow
                };

                await _unitOfWork.Transactions.AddAsync(ledgerTransaction);

                participant.Status = SplitParticipantStatus.Paid;
                participant.PaidAt = DateTime.UtcNow;

                await UpdateSplitGroupStatusAsync(
                    participant.SplitGroupId, cancellationToken);

                try
                {
                    await _unitOfWork.SaveChangesAsync(cancellationToken);
                }
                catch (DbUpdateConcurrencyException)
                {
                    throw new InvalidOperationException(
                        "One of the wallets was updated at the same time by another operation. Please try again.");
                }

                await _unitOfWork.CommitTransactionAsync(cancellationToken);
            }
            catch
            {
                await _unitOfWork.RollbackTransactionAsync(cancellationToken);
                throw;
            }

            await _notificationService.NotifyAsync(
                participant.SplitGroup.Payment.UserId,
                type: "SplitShareReceived",
                message: $"{participant.Amount:N2} EGP was paid towards your split.",
                cancellationToken);

            return await GetDetailsAsync(
                request.SplitGroupId, request.CurrentUserId, cancellationToken);
        }

        public async Task SendReminderAsync(
            SendSplitReminderDto request,
            CancellationToken cancellationToken = default)
        {
            var splitGroup = await _unitOfWork.SplitGroups.GetWithDetailsAsync(
                request.SplitGroupId, cancellationToken);

            if (splitGroup is null)
            {
                throw new KeyNotFoundException("Split was not found.");
            }

            if (splitGroup.Payment.UserId != request.CurrentUserId)
            {
                throw new UnauthorizedAccessException(
                    "Only the person who created the split can send reminders.");
            }

            var pendingParticipants = splitGroup.Participants
                .Where(p => p.Status == SplitParticipantStatus.Pending)
                .Where(p => request.ParticipantUserId is null || p.UserId == request.ParticipantUserId)
                .ToList();

            if (request.ParticipantUserId is not null && pendingParticipants.Count == 0)
            {
                throw new InvalidOperationException(
                    "That participant has either already paid or isn't part of this split.");
            }

            foreach (var participant in pendingParticipants)
            {
                await _notificationService.NotifyAsync(
                    participant.UserId,
                    type: "SplitReminder",
                    message: $"Reminder: you still owe {participant.Amount:N2} EGP for a split payment.",
                    cancellationToken);
            }
        }

        public async Task CancelAsync(
            CancelSplitGroupDto request,
            CancellationToken cancellationToken = default)
        {
            var splitGroup = await _unitOfWork.SplitGroups.GetWithDetailsAsync(
                request.SplitGroupId, cancellationToken);

            if (splitGroup is null)
            {
                throw new KeyNotFoundException("Split was not found.");
            }

            if (splitGroup.Payment.UserId != request.CurrentUserId)
            {
                throw new UnauthorizedAccessException(
                    "Only the person who created the split can cancel it.");
            }

            if (splitGroup.Status != SplitGroupStatus.Pending)
            {
                throw new InvalidOperationException(
                    "Can only cancel a split before anyone has paid their share.");
            }

            splitGroup.Status = SplitGroupStatus.Cancelled;
            splitGroup.Payment.Status = PaymentStatus.Cancelled;
            splitGroup.UpdatedAt = DateTime.UtcNow;

            await _unitOfWork.SaveChangesAsync(cancellationToken);

            foreach (var participant in splitGroup.Participants)
            {
                await _notificationService.NotifyAsync(
                    participant.UserId,
                    type: "SplitCancelled",
                    message: "A split you were part of was cancelled.",
                    cancellationToken);
            }
        }

        /// <summary>Recomputes SplitGroup/Payment status from participant statuses. Caller saves.</summary>
        private async Task UpdateSplitGroupStatusAsync(
            int splitGroupId, CancellationToken cancellationToken)
        {
            var splitGroup = await _unitOfWork.SplitGroups.GetWithDetailsAsync(
                splitGroupId, cancellationToken);

            if (splitGroup is null)
            {
                return;
            }

            var allPaid = splitGroup.Participants.All(p => p.Status == SplitParticipantStatus.Paid);
            var anyPaid = splitGroup.Participants.Any(p => p.Status == SplitParticipantStatus.Paid);

            splitGroup.Status = allPaid
                ? SplitGroupStatus.Completed
                : anyPaid
                    ? SplitGroupStatus.PartiallyPaid
                    : SplitGroupStatus.Pending;

            if (allPaid)
            {
                splitGroup.Payment.Status = PaymentStatus.Completed;
            }
        }

        private async Task<SplitGroup> GetAuthorizedSplitGroupAsync(
            int splitGroupId,
            int currentUserId,
            CancellationToken cancellationToken)
        {
            var splitGroup = await _unitOfWork.SplitGroups.GetWithDetailsAsync(
                splitGroupId, cancellationToken);

            if (splitGroup is null)
            {
                throw new KeyNotFoundException("Split was not found.");
            }

            var isInvolved = splitGroup.Payment.UserId == currentUserId ||
                splitGroup.Participants.Any(p => p.UserId == currentUserId);

            if (!isInvolved)
            {
                throw new UnauthorizedAccessException(
                    "You are not part of this split.");
            }

            return splitGroup;
        }

        private static SplitGroupDto ToDto(SplitGroup splitGroup)
        {
            return new SplitGroupDto
            {
                Id = splitGroup.Id,
                InitiatorUserId = splitGroup.Payment.UserId,
                InitiatorUserName = splitGroup.Payment.User.UserName,
                TotalAmount = splitGroup.Payment.Amount,
                Currency = splitGroup.Payment.Currency,
                SplitType = splitGroup.SplitType.ToString(),
                Status = splitGroup.Status.ToString(),
                DueDate = splitGroup.DueDate,
                CreatedAt = splitGroup.CreatedAt,
                Participants = splitGroup.Participants.Select(p => new SplitParticipantDto
                {
                    Id = p.Id,
                    UserId = p.UserId,
                    UserName = p.User.UserName,
                    Amount = p.Amount,
                    Percentage = p.Percentage,
                    Status = p.Status.ToString(),
                    PaidAt = p.PaidAt
                })
            };
        }
    }
}
