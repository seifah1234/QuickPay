namespace QuickPay.BLL.Services.Interfaces
{
    public interface IRealtimeNotifier
    {
        Task PushToUserAsync(
            int userId,
            string type,
            string message,
            CancellationToken cancellationToken = default);
    }
}
