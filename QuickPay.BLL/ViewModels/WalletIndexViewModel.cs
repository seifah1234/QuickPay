using System.ComponentModel.DataAnnotations;

namespace QuickPay.BLL.ViewModels
{
    public class WalletIndexViewModel
    {
        public IEnumerable<WalletViewModel> Wallets { get; set; }
            = Enumerable.Empty<WalletViewModel>();
        
        [Required(ErrorMessage = "Please enter a wallet name.")]
        public string NewWalletName { get; set; } = string.Empty;
        
        [Range(1, int.MaxValue, ErrorMessage = "Please select a wallet.")]
        public int WalletId { get; set; }

        [Range(0.01, double.MaxValue, ErrorMessage = "Amount must be greater than zero.")]
        public decimal Amount { get; set; }
    }
}
