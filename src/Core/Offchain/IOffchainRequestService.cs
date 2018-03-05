﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Core.Offchain
{
    public interface IOffchainRequestService
    {
        Task CreateOffchainRequest(string transactionId, string clientId, string assetId, decimal amount, string orderId, OffchainTransferType type);
        Task NotifyUser(string clientId);
        Task CreateOffchainRequestAndNotify(string transactionId, string clientId, string assetId, decimal amount, string orderId,OffchainTransferType type);

        Task CreateHubCashoutRequests(string clientId, decimal bitcoinAmount = 0, decimal lkkAmount = 0);
    }
}
