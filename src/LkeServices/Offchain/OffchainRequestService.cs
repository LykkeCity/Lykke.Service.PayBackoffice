﻿using System;
using System.Threading.Tasks;
using Core;
using Core.Notifications;
using Core.Offchain;
using Lykke.Service.ClientAccount.Client;

namespace LkeServices.Offchain
{
    public class OffchainRequestService : IOffchainRequestService
    {
        private readonly IOffchainRequestRepository _offchainRequestRepository;
        private readonly IOffchainTransferRepository _offchainTransferRepository;
        private readonly IClientAccountClient _clientAccountService;
        private readonly IAppNotifications _appNotifications;

        public OffchainRequestService(IOffchainRequestRepository offchainRequestRepository,
            IOffchainTransferRepository offchainTransferRepository,
            IClientAccountClient clientAccountService, IAppNotifications appNotifications)
        {
            _offchainRequestRepository = offchainRequestRepository;
            _offchainTransferRepository = offchainTransferRepository;
            _clientAccountService = clientAccountService;
            _appNotifications = appNotifications;
        }

        public async Task CreateOffchainRequest(string transactionId, string clientId, string assetId, decimal amount, string orderId, OffchainTransferType type)
        {
            var transfer = await _offchainTransferRepository.CreateTransfer(transactionId, clientId, assetId, amount, type, null, orderId);

            await _offchainRequestRepository.CreateRequest(transfer.Id, clientId, assetId, RequestType.RequestTransfer, type);
        }

        public async Task NotifyUser(string clientId)
        {
            var pushSettings = await _clientAccountService.GetPushNotificationAsync(clientId);
            if (pushSettings.Enabled)
            {
                var clientAcc = await _clientAccountService.GetByIdAsync(clientId);

                await _appNotifications.SendDataNotificationToAllDevicesAsync(new[] { clientAcc.NotificationsId }, NotificationType.OffchainRequest, "Wallet");
            }
        }

        public async Task CreateOffchainRequestAndNotify(string transactionId, string clientId, string assetId, decimal amount,
            string orderId, OffchainTransferType type)
        {
            await CreateOffchainRequest(transactionId, clientId, assetId, amount, orderId, type);
            await NotifyUser(clientId);
        }

        public async Task CreateHubCashoutRequests(string clientId, decimal bitcoinAmount = 0, decimal lkkAmount = 0)
        {
            if (bitcoinAmount > 0)
                await CreateOffchainRequest(Guid.NewGuid().ToString(), clientId, LykkeConstants.BitcoinAssetId, bitcoinAmount, null, OffchainTransferType.HubCashout);

            if (lkkAmount > 0)
                await CreateOffchainRequest(Guid.NewGuid().ToString(), clientId, LykkeConstants.LykkeAssetId, lkkAmount, null, OffchainTransferType.HubCashout);

            await NotifyUser(clientId);
        }
    }
}
