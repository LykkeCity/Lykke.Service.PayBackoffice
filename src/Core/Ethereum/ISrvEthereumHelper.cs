using System;
using System.Threading.Tasks;
using Core.Assets;
using Core.Ethereum.Models;
using System.Collections.Generic;
using Core.Bitcoin;
using System.Numerics;
using Lykke.Service.Assets.Client.Models;

namespace Core.Ethereum
{
    public interface ISrvEthereumHelper
    {
        Task<string> GetTokenAddressByAssetIdAsync(string assetId);

        Task<EthereumResponse<EstimationResponse>> EstimateTransactionAsync(BroadcastEthereumTransactionModel ethereumTransaction);

        Task<EthereumResponse<EstimationResponse>> EstimateCashOutAsync(Guid id, string sign, Asset asset, string fromAddress, string toAddress, decimal amount);

        Task<EthereumResponse<EthereumTransactionHistorical>> GetTransactionDetailsAsync(string transactionHash);

        Task<EthereumResponse<IEnumerable<EthereumInternalMessageHistorical>>> GetInternalMessagesForTransactionAsync(string transactionHash);

        Task<EthereumResponse<IEnumerable<EthereumAddressHistory>>> GetTokenAddressHistoryAsync(GetEthereumTokenTransactionHistory getEthereumTransactionHistory);

        Task<EthereumResponse<IEnumerable<EthereumAddressHistory>>> GetAddressHistoryAsync(GetEthereumTransactionHistory getEthereumTransactionHistory);

        Task<EthereumResponse<IEnumerable<EthereumInternalMessageHistorical>>> GetInternalMessageHistoryAsync(GetEthereumTransactionHistory getEthereumTransactionHistory);

        Task<EthereumResponse<IEnumerable<EthereumTransactionHistorical>>> GetTransactionHistoryAsync(GetEthereumTransactionHistory getEthereumTransactionHistory);

        Task<EthereumResponse<BigInteger>> GetRecommendedGasPriceAsync();

        Task<EthereumResponse<GetEthereumTransactionModel>> CreateTransactionAsync(EthereumTransactionModel ethereumTransaction);

        Task<EthereumResponse<GetTransactionHashModel>> BroadcastTransactionAsync(BroadcastEthereumTransactionModel ethereumTransaction);

        Task<EthereumResponse<GetBalanceModel>> GetWalletBalance(string address, IEnumerable<Asset> assets);

        Task<EthereumResponse<GetContractModel>> GetContractAsync(Asset asset, string userAddress);

        Task<EthereumResponse<OperationResponse>> SendTransferAsync(Guid id, string sign, Asset asset, string fromAddress,
            string toAddress, decimal amount);

        Task<EthereumResponse<OperationResponse>> SendCashOutAsync(Guid id, string sign, Asset asset, string fromAddress,
            string toAddress, decimal amount);

        Task<EthereumResponse<bool>> IsSignValid(Guid id, string sign, Asset asset, string fromAddress,
            string toAddress, decimal amount);

        Task<EthereumResponse<EthereumTransaction>> GetNewTxHashAndIdAsync(Asset asset, string fromAddress, string toAddress, decimal amount,
            string coinAdapterAddress = null);

        Task<EthereumResponse<EthereumTransaction>> GetNewTxHashAsync(Guid Id, Asset asset, string fromAddress,
            string toAddress, decimal amount);

        Task<EthereumResponse<OperationResponse>> SendTransferWithChangeAsync(decimal change, string signFrom, Guid id, Asset asset, string fromAddress,
            string toAddress, decimal amount);

        Task<EthereumResponse<bool>> IsSynced(string coinAdapterAddress, string userAddress);

        bool IsValidAddress(string address);
        Task<EthereumResponse<AdapBalanceResponseResponse>> GetBalanceOnAdapterAsync(Asset asset, string address);
        Task<Asset> GetEthAsset();

        Task<EthereumResponse<GetContractModel>> GetErc20DepositContractAsync(string userAddress);
    }

    #region Response Models

    public class EthereumTransaction
    {
        public string Hash { get; set; }
        public Guid OperationId { get; set; }
    }

    #endregion
}
