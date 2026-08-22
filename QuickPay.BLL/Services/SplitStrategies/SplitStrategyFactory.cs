using QuickPay.DAL.Enums;

namespace QuickPay.BLL.Services.SplitStrategies
{
    public interface ISplitStrategyFactory
    {
        ISplitStrategy Resolve(SplitType splitType);
    }

    public class SplitStrategyFactory : ISplitStrategyFactory
    {
        private readonly IReadOnlyDictionary<SplitType, ISplitStrategy> _strategies;

        public SplitStrategyFactory(IEnumerable<ISplitStrategy> strategies)
        {
            _strategies = strategies.ToDictionary(s => s.SplitType);
        }

        public ISplitStrategy Resolve(SplitType splitType)
        {
            if (!_strategies.TryGetValue(splitType, out var strategy))
            {
                throw new ArgumentException($"Unsupported split type: {splitType}.");
            }

            return strategy;
        }
    }
}
