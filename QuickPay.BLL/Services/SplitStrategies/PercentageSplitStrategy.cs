using QuickPay.DAL.Enums;

namespace QuickPay.BLL.Services.SplitStrategies
{
    public class PercentageSplitStrategy : ISplitStrategy
    {
        public SplitType SplitType => SplitType.Percentage;

        public IReadOnlyList<SplitShareResult> Calculate(
            decimal totalAmount,
            IReadOnlyList<SplitShareInput> participants)
        {
            if (participants.Count == 0)
            {
                throw new ArgumentException(
                    "A split needs at least one participant.");
            }

            if (participants.Any(p => p.Percentage is null or <= 0))
            {
                throw new ArgumentException(
                    "Every participant needs a positive percentage.");
            }

            var totalPercentage = participants.Sum(p => p.Percentage!.Value);

            if (Math.Round(totalPercentage, 2) != 100m)
            {
                throw new ArgumentException(
                    $"Percentages add up to {totalPercentage:N2}%, they must add up to 100%.");
            }

            var totalCents = (long)Math.Round(totalAmount * 100m, 0, MidpointRounding.AwayFromZero);
            var results = new List<SplitShareResult>(participants.Count);
            long assignedCents = 0;

            for (var i = 0; i < participants.Count; i++)
            {
                long cents;

                if (i == participants.Count - 1)
                {
                    // Last participant absorbs the rounding remainder so
                    // the shares always sum exactly to totalAmount.
                    cents = totalCents - assignedCents;
                }
                else
                {
                    cents = (long)Math.Round(
                        totalCents * (participants[i].Percentage!.Value / 100m),
                        0,
                        MidpointRounding.AwayFromZero);
                    assignedCents += cents;
                }

                results.Add(new SplitShareResult(
                    participants[i].UserId, cents / 100m, participants[i].Percentage));
            }

            return results;
        }
    }
}
