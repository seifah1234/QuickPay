using QuickPay.BLL.DTOs;
using QuickPay.DAL.Entities;
using System;
using System.Collections.Generic;
using System.Text;

namespace QuickPay.BLL.Services.Interfaces
{
    public interface ITransferService
    {
        Task<TransferResultDto> TransferAsync(
            TransferRequestDto request,
            CancellationToken cancellationToken = default);
    }
}
