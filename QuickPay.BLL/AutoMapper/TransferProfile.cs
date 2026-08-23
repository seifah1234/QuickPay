using AutoMapper;
using QuickPay.BLL.DTOs;
using QuickPay.BLL.ViewModels;
using QuickPay.DAL.Entities;

namespace QuickPay.BLL.AutoMapper
{
    public class TransferProfile : Profile
    {
        public TransferProfile()
        {
            CreateMap<TransferViewModel, TransferRequestDto>();

            CreateMap<TransferResultDto, TransferResultViewModel>();

            CreateMap<FinancialAccount, AccountDto>();

            CreateMap<AccountDto, AccountViewModel>();
        }
    }
}
