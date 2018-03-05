using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Core.CashOperations
{
    public interface ICashoutPaymentDate
    {
        DateTime PaymentDate { get; }
        string RequestId { get; }
    }

    public interface ICashoutPaymentDateRepository
    {
        Task<ICashoutPaymentDate> GetAsync(string requestId);
        Task AddAsync(string requestId, DateTime paymentDate);
    }
}
