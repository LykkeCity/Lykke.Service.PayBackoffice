using Core.CashOperations.PaymentSystems;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Core.CashOperations
{
    public interface ICashoutProcessedReportRow
    {
        string ClientId { get; }
        string RequestId { get; }
        string Currency { get; }
        DateTime DateOfRequest { get; }
        string TransactionId { get; }
        double Amount { get; }
        DateTime? DateOfPayment { get; }
        Swift SwiftData { get; }
    }

    public interface ICashoutsProcessedReportRepository
    {
        Task AddAsync(ICashoutProcessedReportRow row);
        Task<List<ICashoutProcessedReportRow>> GetDataAsync();
    }
}
