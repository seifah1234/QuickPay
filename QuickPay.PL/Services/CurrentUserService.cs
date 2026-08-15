using Microsoft.AspNetCore.Http;
using QuickPay.BLL.Services.Interfaces;

namespace QuickPay.PL.Services
{
    public class CurrentUserService : ICurrentUserService
    {
        private const int DefaultStubUserId = 1;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public CurrentUserService(IHttpContextAccessor httpContextAccessor)
        {
            _httpContextAccessor = httpContextAccessor;
        }

        public int GetCurrentUserId()
        {
            var header = _httpContextAccessor.HttpContext?
                .Request.Headers["X-Debug-User-Id"]
                .FirstOrDefault();

            return int.TryParse(header, out var userId)
                ? userId
                : DefaultStubUserId;
        }
    }
}
