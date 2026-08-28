using QuickPay.DAL.Enums;

namespace QuickPay.BLL.Services.SplitStrategies
{
    /// <summary>One share of a split, before it's tied to a resolved user.</summary>
    public record SplitShareInput(int UserId, decimal? Amount, decimal? Percentage);

    public record SplitShareResult(int UserId, decimal Amount, decimal? Percentage);

    public interface ISplitStrategy
    {
        SplitType SplitType { get; }

        /// <summary>
        /// Turns a total amount + raw participant input into concrete
        /// per-person amounts. Throws ArgumentException if the input is
        /// invalid for this split type (e.g. percentages don't add to 100).
        /// The returned amounts always sum exactly to totalAmount (any
        /// rounding remainder is distributed cent-by-cent).
        /// </summary>
        IReadOnlyList<SplitShareResult> Calculate(
            decimal totalAmount,
            IReadOnlyList<SplitShareInput> participants);
    }
}
