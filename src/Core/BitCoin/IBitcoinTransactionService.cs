﻿using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Core.BitCoin
{
    public interface IBitcoinTransactionService
    {
        Task<T> GetTransactionContext<T>(string transactionId) where T : BaseContextData;

        Task SetTransactionContext<T>(string transactionId, T context) where T : BaseContextData;

        Task SetStringTransactionContext(string transactionId, string context);
    }
}
