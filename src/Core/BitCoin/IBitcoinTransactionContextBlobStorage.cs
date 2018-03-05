using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Core.BitCoin
{
    public interface IBitcoinTransactionContextBlobStorage
    {
        Task<string> Get(string transactionId);
        Task Set(string transactionId, string context);
    }
}
