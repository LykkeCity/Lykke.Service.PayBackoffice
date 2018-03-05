using System;
using System.Threading.Tasks;
using Core.BitCoin;
using Core.Exchange;
using Core.Offchain.Models;

namespace Core.Offchain
{
    public interface IOffchainService
    {
        Task<OffchainResult> CreateTransfer(string clientId, string assetPair, string asset, decimal amount, string prevTempPrivateKey);

        Task<OffchainResult> CreateLimitTransfer(string clientId, string assetPair, string asset, decimal amount, decimal price, string prevTempPrivateKey);

        Task<OffchainResult> TransferRequest(string clientId, string transferId, string prevTempPrivateKey);

        Task<OffchainResult> CreateChannel(IWalletCredentials credentials, IOffchainTransfer offchainTransfer, bool required);

        Task<OffchainResult> CreateDirectTransfer(Guid id, string clientId, string asset, decimal amount, string prevTempPrivateKey);

        Task<OffchainResult> CreateCashout(string clientId, string destination, string asset, decimal amount);

        Task<OffchainResult> CreateOffchainCashout(string clientId, string asset, decimal amount, string prevTempPrivateKey);

        Task<OffchainResult> CreateHubCommitment(string clientId, string transferId, string signedChannel);

        Task<OffchainResult> Finalize(string clientId, string transferId, string clientRevokePubKey, string clientRevokeEncryptedPrivateKey, string signedCommitment);

        Task<OffchainResultOrder> GetResultOrderFromTransfer(string transferId);

        Task<bool> CancelLimitOrder(string clientId, string orderId);

        Task CheckOrderPrice(string assetPair, OrderAction action, decimal price);
    }
}
