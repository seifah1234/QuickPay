using QuickPay.BLL.DTOs;
using QuickPay.BLL.DTOs.SharedWallet;
using QuickPay.BLL.Services.Interfaces;
using QuickPay.DAL.Entities;
using QuickPay.DAL.Enums;
using QuickPay.DAL.UnitOfWork;
using SharedWalletEntity = QuickPay.DAL.Entities.SharedWallet;

namespace QuickPay.BLL.Services.Implementation
{
    public class SharedWalletService : ISharedWalletService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly INotificationService _notificationService;

        public SharedWalletService(
            IUnitOfWork unitOfWork,
            INotificationService notificationService)
        {
            _unitOfWork = unitOfWork;
            _notificationService = notificationService;
        }

        public async Task<IEnumerable<SharedWalletDto>> GetMySharedWalletsAsync(
            int userId,
            CancellationToken cancellationToken = default)
        {
            var sharedWallets = await _unitOfWork.SharedWallets
                .GetByMemberUserIdAsync(userId, cancellationToken);

            return sharedWallets.Select(ToDto);
        }

        public async Task<SharedWalletDto> GetDetailsAsync(
            int sharedWalletId,
            int currentUserId,
            CancellationToken cancellationToken = default)
        {
            var sharedWallet = await GetMemberWalletOrThrow(
                sharedWalletId, currentUserId, cancellationToken);

            return ToDto(sharedWallet);
        }

        public async Task<IEnumerable<TransactionHistoryDto>> GetHistoryAsync(
            int sharedWalletId,
            int currentUserId,
            CancellationToken cancellationToken = default)
        {
            // Membership check only - every member can see every operation
            // (per spec), not just their own.
            await GetMemberWalletOrThrow(
                sharedWalletId, currentUserId, cancellationToken);

            var transactions = await _unitOfWork.Transactions
                .GetByAccountIdAsync(sharedWalletId, cancellationToken);

            return transactions.Select(t => new TransactionHistoryDto
            {
                Id = t.Id,
                Type = t.Type.ToString(),
                Amount = t.Amount,
                Status = t.Status.ToString(),
                CreatedAt = t.CreatedAt,
                Direction = t.FromAccountId == sharedWalletId
                    ? "Outgoing"
                    : "Incoming"
            });
        }

        public async Task<SharedWalletDto> CreateAsync(
            CreateSharedWalletDto request,
            CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(request.Name))
            {
                throw new ArgumentException(
                    "Shared wallet name is required.");
            }

            var sharedWallet = new SharedWalletEntity
            {
                Name = request.Name.Trim(),
                Balance = 0,
                Currency = "EGP",
                IsActive = true
            };

            // Creator becomes the first Admin - there is no separate
            // "owner" column (see the RemoveOwnerColumn migration);
            // ownership is just membership with the Admin role.
            sharedWallet.Members.Add(new SharedWalletMember
            {
                UserId = request.CurrentUserId,
                Role = SharedWalletRole.Admin,
                JoinedAt = DateTime.UtcNow
            });

            await _unitOfWork.SharedWallets.AddAsync(
                sharedWallet, cancellationToken);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            await _notificationService.NotifyAsync(
    request.CurrentUserId,
    type: "SharedWalletCreated",
    message: $"You created a new shared wallet \"{sharedWallet.Name}\".",
    cancellationToken);

            return await GetDetailsAsync(
                sharedWallet.Id, request.CurrentUserId, cancellationToken);
        }

        public async Task<SharedWalletDto> RenameAsync(
            RenameSharedWalletDto request,
            CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(request.NewName))
            {
                throw new ArgumentException(
                    "Shared wallet name is required.");
            }

            await GetAdminMemberOrThrow(
                request.SharedWalletId, request.CurrentUserId, cancellationToken);

            var sharedWallet = await _unitOfWork.SharedWallets
                .GetByIdAsync(request.SharedWalletId, cancellationToken);

            if (sharedWallet is null)
            {
                throw new KeyNotFoundException("Shared wallet was not found.");
            }

            sharedWallet.Name = request.NewName.Trim();
            sharedWallet.UpdatedAt = DateTime.UtcNow;

            await _unitOfWork.SaveChangesAsync(cancellationToken);


            var walletWithMembers = await _unitOfWork.SharedWallets
    .GetWithMembersAsync(sharedWallet.Id, cancellationToken);

            if (walletWithMembers is not null)
            {
                foreach (var member in walletWithMembers.Members)
                {
                    await _notificationService.NotifyAsync(
                        member.UserId,
                        type: "SharedWalletRenamed",
                        message: $"Shared wallet was renamed to \"{sharedWallet.Name}\".",
                        cancellationToken);
                }
            }

            return await GetDetailsAsync(
                sharedWallet.Id, request.CurrentUserId, cancellationToken);
        }

        public async Task<SharedWalletDto> AddMemberAsync(
            AddSharedWalletMemberDto request,
            CancellationToken cancellationToken = default)
        {
            await GetAdminMemberOrThrow(
                request.SharedWalletId, request.CurrentUserId, cancellationToken);

            if (string.IsNullOrWhiteSpace(request.Identifier))
            {
                throw new ArgumentException(
                    "Enter a username, phone number, or email to add.");
            }

            var userToAdd = await _unitOfWork.Users.GetByIdentifierAsync(
                request.Identifier.Trim(), cancellationToken);

            if (userToAdd is null)
            {
                throw new KeyNotFoundException(
                    "No user found with that username, phone number, or email.");
            }

            var existingMembership = await _unitOfWork.SharedWalletMembers
                .GetAsync(
                    request.SharedWalletId, userToAdd.Id, cancellationToken);

            if (existingMembership is not null)
            {
                throw new InvalidOperationException(
                    "That person is already a member of this shared wallet.");
            }

            var walletBeforeAdd = await _unitOfWork.SharedWallets
    .GetWithMembersAsync(request.SharedWalletId, cancellationToken);

            var existingMemberIds = walletBeforeAdd?.Members
                .Select(m => m.UserId)
                .ToList() ?? new List<int>();

            await _unitOfWork.SharedWalletMembers.AddAsync(
                new SharedWalletMember
                {
                    SharedWalletId = request.SharedWalletId,
                    UserId = userToAdd.Id,
                    Role = SharedWalletRole.Member,
                    JoinedAt = DateTime.UtcNow
                },
                cancellationToken);

            await _unitOfWork.SaveChangesAsync(cancellationToken);

            await _notificationService.NotifyAsync(
                userToAdd.Id,
                type: "SharedWalletMemberAdded",
                message: "You were added to a shared wallet.",
                cancellationToken);

            var walletName = walletBeforeAdd?.Name ?? "a shared wallet";

            foreach (var existingMemberId in existingMemberIds)
            {
                await _notificationService.NotifyAsync(
                    existingMemberId,
                    type: "SharedWalletMemberJoined",
                    message: $"{userToAdd.UserName} joined shared wallet \"{walletName}\".",
                    cancellationToken);
            }

            return await GetDetailsAsync(
                request.SharedWalletId, request.CurrentUserId, cancellationToken);
        }

        public async Task RemoveMemberAsync(
            RemoveSharedWalletMemberDto request,
            CancellationToken cancellationToken = default)
        {
            await GetAdminMemberOrThrow(
                request.SharedWalletId, request.CurrentUserId, cancellationToken);

            var membershipToRemove = await _unitOfWork.SharedWalletMembers
                .GetAsync(
                    request.SharedWalletId,
                    request.MemberUserId,
                    cancellationToken);

            if (membershipToRemove is null)
            {
                throw new KeyNotFoundException(
                    "That person is not a member of this shared wallet.");
            }

            if (membershipToRemove.Role == SharedWalletRole.Admin)
            {
                var adminCount = await _unitOfWork.SharedWalletMembers
                    .CountAdminsAsync(request.SharedWalletId, cancellationToken);

                if (adminCount <= 1)
                {
                    throw new InvalidOperationException(
                        "Cannot remove the last admin. Promote another member first.");
                }
            }

            var walletBeforeRemove = await _unitOfWork.SharedWallets
    .GetWithMembersAsync(request.SharedWalletId, cancellationToken);

            var removedUser = await _unitOfWork.Users.GetByIdAsync(
                request.MemberUserId, cancellationToken);

            var remainingMemberIds = walletBeforeRemove?.Members
                .Where(m => m.UserId != request.MemberUserId)
                .Select(m => m.UserId)
                .ToList() ?? new List<int>();

            _unitOfWork.SharedWalletMembers.Remove(membershipToRemove);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            await _notificationService.NotifyAsync(
                request.MemberUserId,
                type: "SharedWalletMemberRemoved",
                message: "You were removed from a shared wallet.",
                cancellationToken);

            var walletName = walletBeforeRemove?.Name ?? "a shared wallet";
            var removedUserName = removedUser?.UserName ?? "A member";

            foreach (var remainingMemberId in remainingMemberIds)
            {
                await _notificationService.NotifyAsync(
                    remainingMemberId,
                    type: "SharedWalletMemberLeft",
                    message: $"{removedUserName} left shared wallet \"{walletName}\".",
                    cancellationToken);
            }
        }

        public async Task CloseAsync(
            CloseSharedWalletDto request,
            CancellationToken cancellationToken = default)
        {
            await GetAdminMemberOrThrow(
                request.SharedWalletId, request.CurrentUserId, cancellationToken);

            var sharedWallet = await _unitOfWork.SharedWallets
                .GetByIdAsync(request.SharedWalletId, cancellationToken);

            if (sharedWallet is null)
            {
                throw new KeyNotFoundException("Shared wallet was not found.");
            }

            if (sharedWallet.Balance != 0)
            {
                throw new InvalidOperationException(
                    "Cannot close a shared wallet with a remaining balance. Transfer the funds out first.");
            }

            // Soft close, same as personal wallets: other modules still
            // reference this account by Id through past transactions.
            sharedWallet.IsActive = false;
            sharedWallet.UpdatedAt = DateTime.UtcNow;

            await _unitOfWork.SaveChangesAsync(cancellationToken);

            var walletWithMembers = await _unitOfWork.SharedWallets
                .GetWithMembersAsync(sharedWallet.Id, cancellationToken);

            if (walletWithMembers is not null)
            {
                foreach (var member in walletWithMembers.Members)
                {
                    await _notificationService.NotifyAsync(
                        member.UserId,
                        type: "SharedWalletClosed",
                        message: $"Shared wallet \"{sharedWallet.Name}\" has been closed.",
                        cancellationToken);
                }
            }
        }

        private async Task<SharedWalletEntity> GetMemberWalletOrThrow(
            int sharedWalletId,
            int currentUserId,
            CancellationToken cancellationToken)
        {
            var sharedWallet = await _unitOfWork.SharedWallets
                .GetWithMembersAsync(sharedWalletId, cancellationToken);

            if (sharedWallet is null)
            {
                throw new KeyNotFoundException("Shared wallet was not found.");
            }

            var isMember = sharedWallet.Members
                .Any(m => m.UserId == currentUserId);

            if (!isMember)
            {
                throw new UnauthorizedAccessException(
                    "You are not a member of this shared wallet.");
            }

            return sharedWallet;
        }

        private async Task GetAdminMemberOrThrow(
            int sharedWalletId,
            int currentUserId,
            CancellationToken cancellationToken)
        {
            var membership = await _unitOfWork.SharedWalletMembers
                .GetAsync(sharedWalletId, currentUserId, cancellationToken);

            if (membership is null)
            {
                throw new UnauthorizedAccessException(
                    "You are not a member of this shared wallet.");
            }

            if (membership.Role != SharedWalletRole.Admin)
            {
                throw new UnauthorizedAccessException(
                    "Only shared wallet admins can do this.");
            }
        }

        private static SharedWalletDto ToDto(SharedWalletEntity sharedWallet)
        {
            return new SharedWalletDto
            {
                Id = sharedWallet.Id,
                Name = sharedWallet.Name,
                Balance = sharedWallet.Balance,
                Currency = sharedWallet.Currency,
                IsActive = sharedWallet.IsActive,
                Members = sharedWallet.Members.Select(m => new SharedWalletMemberDto
                {
                    UserId = m.UserId,
                    UserName = m.User.UserName,
                    Role = m.Role.ToString(),
                    JoinedAt = m.JoinedAt
                })
            };
        }
    }
}
