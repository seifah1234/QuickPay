using QuickPay.DAL.Enums;

namespace QuickPay.BLL.Services.SplitStrategies
{
    public class EqualSplitStrategy : ISplitStrategy
    {
        public SplitType SplitType => SplitType.Equal;

        public IReadOnlyList<SplitShareResult> Calculate(
            decimal totalAmount,
            IReadOnlyList<SplitShareInput> participants)
        {
            if (participants.Count == 0)
            {
                throw new ArgumentException(
                    "A split needs at least one participant.");
            }

            // Split into whole cents, then hand out the leftover cents one
            // at a time so the shares always sum exactly to totalAmount.
            var totalCents = (long)Math.Round(totalAmount * 100m, 0, MidpointRounding.AwayFromZero);
            var baseCents = totalCents / participants.Count;
            var remainderCents = totalCents % participants.Count;

            var results = new List<SplitShareResult>(participants.Count);

            for (var i = 0; i < participants.Count; i++)
            {
                var cents = baseCents + (i < remainderCents ? 1 : 0);
                results.Add(new SplitShareResult(
                    participants[i].UserId, cents / 100m, null));
            }

            return results;
        }
    }
}
