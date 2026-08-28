using QuickPay.BLL.DTOs;

namespace QuickPay.BLL.ViewModels
{
    public class DashboardViewModel
    {
        public IEnumerable<AccountDto> Accounts { get; set; } = new List<AccountDto>();
        public IEnumerable<TransactionHistoryDto> RecentTransactions { get; set; } = new List<TransactionHistoryDto>();
        public decimal TotalBalance { get; set; }
        public int ActiveAccountsCount { get; set; }
        public int TotalAccountsCount { get; set; }
    }
}