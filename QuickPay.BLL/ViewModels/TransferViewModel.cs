using QuickPay.BLL.DTOs;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace QuickPay.BLL.ViewModels
{
    public class TransferViewModel
    {
        [Range(1, int.MaxValue,
            ErrorMessage = "Please select a source account.")]
        public int FromAccountId { get; set; }

        [Range(1, int.MaxValue,
            ErrorMessage = "Please search for and select a recipient.")]
        public int ToAccountId { get; set; }

        public string? ToAccountDisplay { get; set; }

        [Range(0.01, double.MaxValue,
            ErrorMessage = "Amount must be greater than zero.")]
        public decimal Amount { get; set; }

        public IEnumerable<AccountViewModel> Accounts { get; set; }
            = Enumerable.Empty<AccountViewModel>();
    }
}
