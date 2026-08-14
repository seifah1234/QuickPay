using System;
using System.Collections.Generic;
using System.Text;

namespace QuickPay.BLL.Services.Abstraction
{
    public interface IRealtimeNotifier
    {
        Task PushToUserAsync(int userId, string type, string message);
    }
}
