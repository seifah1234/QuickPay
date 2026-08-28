using QuickPay.DAL.Enums;

namespace QuickPay.BLL.Services.SplitStrategies
{
    public class CustomAmountSplitStrategy : ISplitStrategy
    {
        public SplitType SplitType => SplitType.CustomAmount;

        public IReadOnlyList<SplitShareResult> Calculate(
            decimal totalAmount,
            IReadOnlyList<SplitShareInput> participants)
        {
            if (participants.Count == 0)
            {
                throw new ArgumentException(
                    "A split needs at least one participant.");
            }

            if (participants.Any(p => p.Amount is null or <= 0))
            {
                throw new ArgumentException(
                    "Every participant needs a positive custom amount.");
            }

            var sum = participants.Sum(p => p.Amount!.Value);

            if (Math.Round(sum, 2) != Math.Round(totalAmount, 2))
            {
                throw new ArgumentException(
                    $"Custom amounts add up to {sum:N2}, but the split total is {totalAmount:N2}.");
            }

            return participants
                .Select(p => new SplitShareResult(p.UserId, p.Amount!.Value, null))
                .ToList();
        }
    }
}
