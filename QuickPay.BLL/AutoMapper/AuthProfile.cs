using AutoMapper;
using QuickPay.BLL.DTOs.Auth;
using QuickPay.BLL.ViewModels.Auth;

namespace QuickPay.BLL.AutoMapper
{
    public class AuthProfile : Profile
    {
        public AuthProfile()
        {
            CreateMap<RegisterViewModel, RegisterRequestDto>();

            CreateMap<LoginViewModel, LoginRequestDto>();

            CreateMap<VerifyOtpViewModel, VerifyOtpRequestDto>();
        }
    }
}
