using System;
using System.Numerics;
using System.Threading.Tasks;
using Core.Ethereum;
using Core.Ethereum.Models;
using Lykke.Service.EthereumCore.Client;
using Lykke.Service.EthereumCore.Client.Models;
using System.Collections.Generic;
using System.Linq;
using Core;
using Core.Bitcoin;
using System.Text.RegularExpressions;
using Nethereum.Util;
using Lykke.Service.Assets.Client.Models;

using ErrorResponse = Core.Ethereum.ErrorResponse;
using Lykke.Service.Assets.Client;

namespace LkeServices.Ethereum
{
    public class SrvEthereumHelper : ISrvEthereumHelper
    {
        private readonly IEthereumApi _ethereumApi;
        private readonly CachedTradableAssetsDictionary _tradableAssetsDict;
        private readonly IAssetsService _assetsService;
        private readonly AddressUtil _addressUtil;

        public SrvEthereumHelper(
            IEthereumApi ethereumApi,
            CachedTradableAssetsDictionary tradableAssetsDict,
            IAssetsService assetsService)
        {
            _addressUtil = new AddressUtil();
            _ethereumApi = ethereumApi;
            _tradableAssetsDict = tradableAssetsDict;
            _assetsService = assetsService;
        }

        public async Task<string> GetTokenAddressByAssetIdAsync(string assetId)
        {
            var erc20Token = await _assetsService.Erc20TokenGetBySpecificationAsync(
                new Lykke.Service.Assets.Client.Models.Erc20TokenSpecification(new List<string>() { assetId }));
            string erc20TokenAddress = erc20Token.Items.FirstOrDefault()?.Address;

            return erc20TokenAddress;
        }

        public async Task<EthereumResponse<AdapBalanceResponseResponse>> GetBalanceOnAdapterAsync(Asset asset, string address)
        {
            var response = await _ethereumApi.ApiCoinAdapterBalanceByCoinAdapterAddressByUserAddressGetAsync(asset.AssetAddress, address);

            if (response is ApiException error)
            {
                return new EthereumResponse<AdapBalanceResponseResponse>
                {
                    Error = new ErrorResponse { Code = error.Error.Code, Message = error.Error.Message }
                };
            }

            BalanceModel res = response as BalanceModel;
            if (res != null)
            {
                return new EthereumResponse<AdapBalanceResponseResponse>
                {
                    Result = new AdapBalanceResponseResponse
                    {
                        Balance = EthServiceHelpers.ConvertFromContract(res.Amount, asset.MultiplierPower, asset.Accuracy),
                        CoinAdaperAddress = asset.AssetAddress,
                        UserAddress = address
                    }
                };
            }

            throw new Exception("Unknown response");
        }

        public bool IsValidAddress(string address)
        {
            if (!new Regex("^(0x)?[0-9a-f]{40}$", RegexOptions.IgnoreCase).IsMatch(address))
            {
                // check if it has the basic requirements of an address
                return false;
            }
            else if (new Regex("^(0x)?[0-9a-f]{40}$").IsMatch(address) ||
                new Regex("^(0x)?[0-9A-F]{40}$").IsMatch(address))
            {
                // If it's all small caps or all all caps, return true
                return true;
            }
            else
            {
                // Check each case
                return _addressUtil.IsChecksumAddress(address);
            };
        }
        #region GetByTransactionHash

        public async Task<EthereumResponse<EthereumTransactionHistorical>> GetTransactionDetailsAsync(string transactionHash)
        {
            var response = await _ethereumApi.ApiTransactionsTxHashByTransactionHashPostAsync(transactionHash);

            var error = response as ApiException;
            if (error != null)
            {
                return new EthereumResponse<EthereumTransactionHistorical>
                {
                    Error = new ErrorResponse { Code = error.Error.Code, Message = error.Error.Message }
                };
            }

            var transaction = response as TransactionResponse;
            if (transaction != null)
            {
                return new EthereumResponse<EthereumTransactionHistorical>()
                {
                    Result = MapTransactionResponseToModel(transaction)
                };
            }

            throw new Exception("Unknown response");
        }

        public async Task<EthereumResponse<IEnumerable<EthereumInternalMessageHistorical>>> GetInternalMessagesForTransactionAsync(string transactionHash)
        {
            var response = await _ethereumApi.ApiInternalMessagesTxHashByTransactionHashPostAsync(transactionHash);

            var error = response as ApiException;
            if (error != null)
            {
                return new EthereumResponse<IEnumerable<EthereumInternalMessageHistorical>>
                {
                    Error = new ErrorResponse { Code = error.Error.Code, Message = error.Error.Message }
                };
            }

            var res = response as FilteredInternalMessagessResponse;
            if (res != null)
            {
                List<EthereumInternalMessageHistorical> messages = new List<EthereumInternalMessageHistorical>(res.Messages?.Count ?? 0);
                foreach (var message in res.Messages)
                {
                    messages.Add(MapInternalMessageResponseToModel(message));
                }
                return new EthereumResponse<IEnumerable<EthereumInternalMessageHistorical>>
                {
                    Result = messages
                };
            }

            throw new Exception("Unknown response");
        }

        #endregion

        /// <summary>
        /// Get erc20 transaction history for address with pagination.
        /// </summary>
        public async Task<EthereumResponse<IEnumerable<EthereumAddressHistory>>> GetTokenAddressHistoryAsync(GetEthereumTokenTransactionHistory getEthereumTransactionHistory)
        {
            var response = await _ethereumApi.ApiTransactionsErcHistoryPostAsync(new TokenAddressTransactions()
            {
                TokenAddress = getEthereumTransactionHistory.TokenAddress,
                Address = getEthereumTransactionHistory.Address,
                Count = getEthereumTransactionHistory.Count,
                Start = getEthereumTransactionHistory.Start,
            });

            var error = response as ApiException;
            if (error != null)
            {
                return new EthereumResponse<IEnumerable<EthereumAddressHistory>>
                {
                    Error = new ErrorResponse { Code = error.Error.Code, Message = error.Error.Message }
                };
            }

            var res = response as FilteredTokenAddressHistoryResponse;
            if (res != null)
            {
                List<EthereumAddressHistory> history = new List<EthereumAddressHistory>(res.History?.Count ?? 0);
                foreach (var message in res.History)
                {
                    history.Add(MapEthereumAddressHistory(message));
                }
                return new EthereumResponse<IEnumerable<EthereumAddressHistory>>
                {
                    Result = history
                };
            }

            throw new Exception("Unknown response");
        }

        private static EthereumAddressHistory MapEthereumAddressHistory(TokenAddressHistoryResponse message)
        {
            return new EthereumAddressHistory()
            {
                BlockNumber = (ulong)message.BlockNumber,
                BlockTimestamp = (uint)message.BlockTimestamp,
                BlockTimeUtc = message.BlockTimeUtc.Value,
                From = message.FromProperty,
                HasError = message.HasError.Value,
                MessageIndex = message.MessageIndex.Value,
                To = message.To,
                TransactionHash = message.TransactionHash,
                TransactionIndex = message.TransactionIndexInBlock.Value,
                Value = BigInteger.Parse(message.TokenTransfered),
                GasPrice = BigInteger.Parse(message.GasPrice ?? "0"),
                GasUsed = BigInteger.Parse(message.GasUsed ?? "0"),
                ContractAddress = message.ContractAddress.ToLower()
            };
        }

        /// <summary>
        /// Get transaction history for address with pagination.
        /// </summary>
        public async Task<EthereumResponse<IEnumerable<EthereumAddressHistory>>> GetAddressHistoryAsync(GetEthereumTransactionHistory getEthereumTransactionHistory)
        {
            var response = await _ethereumApi.ApiTransactionsHistoryPostAsync(new AddressTransactions()
            {
                Address = getEthereumTransactionHistory.Address,
                Count = getEthereumTransactionHistory.Count,
                Start = getEthereumTransactionHistory.Start,
            });

            var error = response as ApiException;
            if (error != null)
            {
                return new EthereumResponse<IEnumerable<EthereumAddressHistory>>
                {
                    Error = new ErrorResponse { Code = error.Error.Code, Message = error.Error.Message }
                };
            }

            var res = response as FilteredAddressHistoryResponse;
            if (res != null)
            {
                List<EthereumAddressHistory> history = new List<EthereumAddressHistory>(res.History?.Count ?? 0);
                foreach (var message in res.History)
                {
                    history.Add(new EthereumAddressHistory()
                    {
                        BlockNumber = (ulong)message.BlockNumber,
                        BlockTimestamp = (uint)message.BlockTimestamp,
                        BlockTimeUtc = message.BlockTimeUtc.Value,
                        From = message.FromProperty,
                        HasError = message.HasError.Value,
                        MessageIndex = message.MessageIndex.Value,
                        To = message.To,
                        TransactionHash = message.TransactionHash,
                        TransactionIndex = message.TransactionIndexInBlock.Value,
                        Value = BigInteger.Parse(message.Value),
                        GasPrice = BigInteger.Parse(message.GasPrice ?? "0"),
                        GasUsed = BigInteger.Parse(message.GasUsed ?? "0")
                    });
                }
                return new EthereumResponse<IEnumerable<EthereumAddressHistory>>
                {
                    Result = history
                };
            }

            throw new Exception("Unknown response");
        }

        /// <summary>
        /// Get transaction history for address with pagination.
        /// </summary>
        public async Task<EthereumResponse<IEnumerable<EthereumInternalMessageHistorical>>> GetInternalMessageHistoryAsync(GetEthereumTransactionHistory getEthereumTransactionHistory)
        {
            var response = await _ethereumApi.ApiInternalMessagesPostAsync(new AddressTransactions()
            {
                Address = getEthereumTransactionHistory.Address,
                Count = getEthereumTransactionHistory.Count,
                Start = getEthereumTransactionHistory.Start,
            });

            var error = response as ApiException;
            if (error != null)
            {
                return new EthereumResponse<IEnumerable<EthereumInternalMessageHistorical>>
                {
                    Error = new ErrorResponse { Code = error.Error.Code, Message = error.Error.Message }
                };
            }

            var res = response as FilteredInternalMessagessResponse;
            if (res != null)
            {
                List<EthereumInternalMessageHistorical> messages = new List<EthereumInternalMessageHistorical>(res.Messages?.Count ?? 0);
                foreach (var message in res.Messages)
                {
                    messages.Add(MapInternalMessageResponseToModel(message));
                }
                return new EthereumResponse<IEnumerable<EthereumInternalMessageHistorical>>
                {
                    Result = messages
                };
            }

            throw new Exception("Unknown response");
        }

        /// <summary>
        /// Get transaction history for address with pagination.
        /// </summary>
        public async Task<EthereumResponse<IEnumerable<EthereumTransactionHistorical>>> GetTransactionHistoryAsync(GetEthereumTransactionHistory getEthereumTransactionHistory)
        {
            var response = await _ethereumApi.ApiTransactionsPostAsync(new AddressTransactions()
            {
                Address = getEthereumTransactionHistory.Address,
                Count = getEthereumTransactionHistory.Count,
                Start = getEthereumTransactionHistory.Start,
            });

            var error = response as ApiException;
            if (error != null)
            {
                return new EthereumResponse<IEnumerable<EthereumTransactionHistorical>>
                {
                    Error = new ErrorResponse { Code = error.Error.Code, Message = error.Error.Message }
                };
            }

            var res = response as FilteredTransactionsResponse;
            if (res != null)
            {
                List<EthereumTransactionHistorical> transactions = new List<EthereumTransactionHistorical>(res.Transactions?.Count ?? 0);
                foreach (var transaction in res.Transactions)
                {
                    transactions.Add(MapTransactionResponseToModel(transaction));
                }
                return new EthereumResponse<IEnumerable<EthereumTransactionHistorical>>
                {
                    Result = transactions
                };
            }

            throw new Exception("Unknown response");
        }

        /// <summary>
        /// Get rexommended gas price.
        /// </summary>
        public async Task<EthereumResponse<BigInteger>> GetRecommendedGasPriceAsync()
        {
            var response = await _ethereumApi.ApiRpcGetNetworkGasPriceGetAsync();

            var error = response as ApiException;
            if (error != null)
            {
                return new EthereumResponse<BigInteger>
                {
                    Error = new ErrorResponse { Code = error.Error.Code, Message = error.Error.Message }
                };
            }

            var res = response as BalanceModel;
            if (res != null)
            {
                return new EthereumResponse<BigInteger>
                {
                    Result = BigInteger.Parse(res.Amount)
                };
            }

            throw new Exception("Unknown response");
        }

        /// <summary>
        /// Create transaction for signing.
        /// </summary>
        public async Task<EthereumResponse<EstimationResponse>> EstimateTransactionAsync(BroadcastEthereumTransactionModel ethereumTransaction)
        {
            var response = await _ethereumApi.ApiPrivateWalletEstimateTransactionPostAsync(new PrivateWalletEthSignedTransaction()
            {
                FromAddress = ethereumTransaction.FromAddress,
                SignedTransactionHex = ethereumTransaction.TransactionHexRawSigned,
            });

            var error = response as ApiException;
            if (error != null)
            {
                return new EthereumResponse<EstimationResponse>
                {
                    Error = new ErrorResponse { Code = error.Error.Code, Message = error.Error.Message }
                };
            }

            var res = response as EstimatedGasModel;
            if (res != null)
            {
                return new EthereumResponse<EstimationResponse>
                {
                    Result = new EstimationResponse
                    {
                        GasAmount = res.EstimatedGas,
                        IsAllowed = res.IsAllowed.Value

                    }
                };
            }

            throw new Exception("Unknown response");
        }

        /// <summary>
        /// Create transaction for signing.
        /// </summary>
        public async Task<EthereumResponse<GetTransactionHashModel>> BroadcastTransactionAsync(BroadcastEthereumTransactionModel ethereumTransaction)
        {
            var response = await _ethereumApi.ApiPrivateWalletSubmitTransactionPostAsync(new PrivateWalletEthSignedTransaction()
            {
                FromAddress = ethereumTransaction.FromAddress,
                SignedTransactionHex = ethereumTransaction.TransactionHexRawSigned,
            });

            var error = response as ApiException;
            if (error != null)
            {
                return new EthereumResponse<GetTransactionHashModel>
                {
                    Error = new ErrorResponse { Code = error.Error.Code, Message = error.Error.Message }
                };
            }

            var res = response as HashResponse;
            if (res != null)
            {
                return new EthereumResponse<GetTransactionHashModel>
                {
                    Result = new GetTransactionHashModel
                    {
                        TransactionHash = res.HashHex
                    }
                };
            }

            throw new Exception("Unknown response");
        }

        /// <summary>
        /// Create transaction for signing.
        /// </summary>
        public async Task<EthereumResponse<GetEthereumTransactionModel>> CreateTransactionAsync(EthereumTransactionModel ethereumTransaction)
        {
            var ethAsset = await GetEthAsset();
            var asset = ethereumTransaction.Asset;
            object response = null;

            //Generate Transaction for ETH transfer
            if (ethAsset.Id == asset.Id)
            {
                response = await _ethereumApi.ApiPrivateWalletGetTransactionPostAsync(new PrivateWalletEthTransaction()
                {
                    FromAddress = ethereumTransaction.FromAddress,
                    GasAmount = ethereumTransaction.GasAmount.ToString(),
                    GasPrice = ethereumTransaction.GasPrice.ToString(),
                    ToAddress = ethereumTransaction.ToAddress.ToString(),
                    Value = EthServiceHelpers.ConvertToContract((decimal)ethereumTransaction.Value, asset.MultiplierPower, asset.Accuracy), //Ethereum to wei
                });
            }
            else
            {
                string tokenAddress = await GetTokenAddressByAssetIdAsync(asset.Id);
                if (string.IsNullOrEmpty(tokenAddress) || !IsValidAddress(tokenAddress))
                {
                    throw new Exception("Chosen asset is not erc20 compatible");
                }
                //Generate Transaction for ERC20 transfer
                response = await _ethereumApi.ApiErc20WalletGetTransactionPostAsync(new PrivateWalletErc20Transaction()
                {
                    TokenAddress = tokenAddress,
                    TokenAmount = EthServiceHelpers.ConvertToContract((decimal)ethereumTransaction.Value, asset.MultiplierPower, asset.Accuracy),
                    FromAddress = ethereumTransaction.FromAddress,
                    GasAmount = ethereumTransaction.GasAmount.ToString(),
                    GasPrice = ethereumTransaction.GasPrice.ToString(),
                    ToAddress = ethereumTransaction.ToAddress.ToString(),
                    Value = "0",//Do not send eth for that type of transactions
                });
            }

            var error = response as ApiException;
            if (error != null)
            {
                return new EthereumResponse<GetEthereumTransactionModel>
                {
                    Error = new ErrorResponse { Code = error.Error.Code, Message = error.Error.Message }
                };
            }

            var res = response as EthTransactionRaw;
            if (res != null)
            {
                return new EthereumResponse<GetEthereumTransactionModel>
                {
                    Result = new GetEthereumTransactionModel
                    {
                        FromAddress = res.FromAddress,
                        TransactionHexRaw = res.TransactionHex
                    }
                };
            }

            throw new Exception("Unknown response");
        }

        /// <summary>
        /// Get Eth balance on Address only. Balance for assets would be implemented later.
        /// TODO: Extend to work with erc20 balances
        /// </summary>
        /// <param name="address"></param>
        /// <returns></returns>
        public async Task<EthereumResponse<GetBalanceModel>> GetWalletBalance(string address, IEnumerable<Asset> assets)
        {
            var response = await _ethereumApi.ApiRpcGetBalanceByAddressGetAsync(address);
            var ethAsset = await GetEthAsset();

            if (response is ApiException error)
            {
                return new EthereumResponse<GetBalanceModel>
                {
                    Error = new ErrorResponse { Code = error.Error.Code, Message = error.Error.Message }
                };
            }

            var erc20Asset = new Dictionary<string, Asset>();
            var tokens = await _assetsService.Erc20TokenGetBySpecificationAsync(new Lykke.Service.Assets.Client.Models.Erc20TokenSpecification()
            {
                Ids = assets?.Where(x => x.Blockchain == Blockchain.Ethereum).Select(x => x.Id).ToList()
            });
            //TODO: Only erc20 Assets
            var tokenAddresses = assets.Join(tokens?.Items, x => x.Id, y => y.AssetId, (asset, token) =>
            {
                string assetAddress = token?.Address?.ToLower();
                if (!string.IsNullOrEmpty(assetAddress))
                    erc20Asset[assetAddress] = asset;

                return assetAddress;
            })?.Where(x => x != null).ToList();

            var tokenResponse = await _ethereumApi.ApiErc20BalancePostAsync(new GetErcBalance(address, tokenAddresses));
            error = tokenResponse as ApiException;
            if (error != null)
            {
                return new EthereumResponse<GetBalanceModel>
                {
                    Error = new ErrorResponse { Code = error.Error.Code, Message = error.Error.Message }
                };
            }

            var tokenBalances = tokenResponse as AddressTokenBalanceContainerResponse;
            List<BalanceRecord> balances = new List<BalanceRecord>();

            if (tokenBalances != null)
            {
                balances = tokenBalances.Balances.Select(x =>
                {
                    string ercAddress = x.Erc20TokenAddress.ToLower();
                    Asset asset = null;

                    erc20Asset.TryGetValue(ercAddress, out asset);

                    if (asset == null)
                        return null;

                    return new BalanceRecord()
                    {
                        AssetId = asset.Id,
                        Balance = (double)EthServiceHelpers.ConvertFromContract(x.Balance, asset.MultiplierPower, asset.Accuracy)
                    };
                })?.Where(x => x != null).ToList();
            }

            var res = response as BalanceModel;
            if (res != null)
            {
                var ethBalance = new BalanceRecord()
                {
                    AssetId = ethAsset.Id,
                    Balance = (double)EthServiceHelpers.ConvertFromContract(res.Amount, ethAsset.MultiplierPower, ethAsset.Accuracy)
                };
                balances.Add(ethBalance);
            }

            return new EthereumResponse<GetBalanceModel>
            {
                Result = new GetBalanceModel
                {
                    Address = address,
                    Balances = balances,
                }
            };


            //throw new Exception("Unknown response");
        }

        public async Task<EthereumResponse<GetContractModel>> GetContractAsync(Asset asset, string userAddress)
        {
            var response = await _ethereumApi.ApiTransitionCreatePostAsync(new CreateTransitionContractModel
            {
                CoinAdapterAddress = asset.AssetAddress,
                UserAddress = userAddress
            });

            if (response is ApiException error)
            {
                return new EthereumResponse<GetContractModel>
                {
                    Error = new ErrorResponse { Code = error.Error.Code, Message = error.Error.Message }
                };
            }

            var res = response as RegisterResponse;
            if (res != null)
            {
                return new EthereumResponse<GetContractModel>
                {
                    Result = new GetContractModel
                    {
                        Contract = res.Contract
                    }
                };
            }

            throw new Exception("Unknown response");
        }

        public async Task<EthereumResponse<GetContractModel>> GetErc20DepositContractAsync(string userAddress)
        {
            var response = await _ethereumApi.ApiErc20depositsPostAsync(userAddress);

            if (response is ApiException error)
            {
                return new EthereumResponse<GetContractModel>
                {
                    Error = new ErrorResponse { Code = error.Error.Code, Message = error.Error.Message }
                };
            }

            var res = response as RegisterResponse;
            if (res != null)
            {
                return new EthereumResponse<GetContractModel>
                {
                    Result = new GetContractModel
                    {
                        Contract = res.Contract
                    }
                };
            }

            throw new Exception("Unknown response");
        }

        public async Task<EthereumResponse<OperationResponse>> SendTransferAsync(Guid id, string sign, Asset asset, string fromAddress, string toAddress, decimal amount)
        {
            var response = await _ethereumApi.ApiExchangeTransferPostAsync(new TransferModel
            {
                Amount = EthServiceHelpers.ConvertToContract(amount, asset.MultiplierPower, asset.Accuracy),
                CoinAdapterAddress = asset.AssetAddress,
                ToAddress = toAddress,
                FromAddress = fromAddress,
                Id = id,
                Sign = sign
            });

            var error = response as ApiException;
            if (error != null)
            {
                return new EthereumResponse<OperationResponse>
                {
                    Error = new ErrorResponse { Code = error.Error.Code, Message = error.Error.Message }
                };
            }

            var res = response as OperationIdResponse;
            if (res != null)
            {
                return new EthereumResponse<OperationResponse> { Result = new OperationResponse { OperationId = res.OperationId } };
            }

            throw new Exception("Unknown response");
        }

        public async Task<EthereumResponse<EthereumTransaction>> GetNewTxHashAndIdAsync(Asset asset, 
            string fromAddress,
            string toAddress, 
            decimal amount,
            string coinAdapterAddress = null)
        {
            var response = await _ethereumApi.ApiHashCalculateAndGetIdPostAsync(new BaseCoinRequestParametersModel
            {
                Amount = EthServiceHelpers.ConvertToContract(amount, asset.MultiplierPower, asset.Accuracy),
                CoinAdapterAddress = coinAdapterAddress ?? asset.AssetAddress,
                FromAddress = fromAddress,
                ToAddress = toAddress
            });

            var error = response as ApiException;
            if (error != null)
            {
                return new EthereumResponse<EthereumTransaction>
                {
                    Error = new ErrorResponse { Code = error.Error.Code, Message = error.Error.Message }
                };
            }

            var res = response as HashResponseWithId;
            if (res != null)
            {
                return new EthereumResponse<EthereumTransaction>
                {
                    Result = new EthereumTransaction
                    {
                        Hash = res.HashHex,
                        OperationId = res.OperationId.GetValueOrDefault()
                    }
                };
            }

            throw new Exception("Unknown response");
        }

        public async Task<EthereumResponse<EthereumTransaction>> GetNewTxHashAsync(Guid Id, Asset asset, string fromAddress, string toAddress, decimal amount)
        {
            var response = await _ethereumApi.ApiHashCalculatePostAsync(new BaseCoinRequestModel(
                Id, asset.AssetAddress, fromAddress, toAddress,
                EthServiceHelpers.ConvertToContract(amount, asset.MultiplierPower, asset.Accuracy)));

            var error = response as ApiException;
            if (error != null)
            {
                return new EthereumResponse<EthereumTransaction>
                {
                    Error = new ErrorResponse { Code = error.Error.Code, Message = error.Error.Message }
                };
            }

            var res = response as HashResponse;
            if (res != null)
            {
                return new EthereumResponse<EthereumTransaction>
                {
                    Result = new EthereumTransaction
                    {
                        Hash = res.HashHex,
                        OperationId = Id
                    }
                };
            }

            throw new Exception("Unknown response");
        }

        public async Task<EthereumResponse<OperationResponse>> SendCashOutAsync(Guid id, string sign, Asset asset, string fromAddress, string toAddress, decimal amount)
        {
            var response = await _ethereumApi.ApiExchangeCashoutPostAsync(new CashoutModel
            {
                Amount = EthServiceHelpers.ConvertToContract(amount, asset.MultiplierPower, asset.Accuracy),
                CoinAdapterAddress = asset.AssetAddress,
                ToAddress = toAddress,
                FromAddress = fromAddress,
                Id = id,
                Sign = sign
            });

            var error = response as ApiException;
            if (error != null)
            {
                return new EthereumResponse<OperationResponse>
                {
                    Error = new ErrorResponse { Code = error.Error.Code, Message = error.Error.Message }
                };
            }

            var res = response as OperationIdResponse;
            if (res != null)
            {
                return new EthereumResponse<OperationResponse> { Result = new OperationResponse { OperationId = res.OperationId } };
            }

            throw new Exception("Unknown response");
        }

        public async Task<EthereumResponse<EstimationResponse>> EstimateCashOutAsync(Guid id, string sign, Asset asset, string fromAddress, string toAddress, decimal amount)
        {
            var response = await _ethereumApi.ApiExchangeEstimateCashoutGasPostAsync(new TransferModel
            {
                Amount = EthServiceHelpers.ConvertToContract(amount, asset.MultiplierPower, asset.Accuracy),
                CoinAdapterAddress = asset.AssetAddress,
                ToAddress = toAddress,
                FromAddress = fromAddress,
                Id = id,
                Sign = sign
            });

            var error = response as ApiException;
            if (error != null)
            {
                return new EthereumResponse<EstimationResponse>
                {
                    Error = new ErrorResponse { Code = error.Error.Code, Message = error.Error.Message }
                };
            }

            EstimatedGasModel res = response as EstimatedGasModel;
            if (res != null)
            {
                return new EthereumResponse<EstimationResponse>
                {
                    Result = new EstimationResponse
                    {
                        GasAmount = res.EstimatedGas,
                        IsAllowed = res.IsAllowed.Value
                    }
                };
            }

            throw new Exception("Unknown response");
        }

        public async Task<EthereumResponse<bool>> IsSignValid(Guid id, string sign, Asset asset, string fromAddress, string toAddress, decimal amount)
        {
            var response = await _ethereumApi.ApiExchangeCheckSignPostAsync(new CheckSignModel
            {
                Amount = EthServiceHelpers.ConvertToContract(amount, asset.MultiplierPower, asset.Accuracy),
                CoinAdapterAddress = asset.AssetAddress,
                ToAddress = toAddress,
                FromAddress = fromAddress,
                Id = id,
                Sign = sign
            });

            var error = response as ApiException;
            if (error != null)
            {
                return new EthereumResponse<bool>
                {
                    Error = new ErrorResponse { Code = error.Error.Code, Message = error.Error.Message }
                };
            }

            var res = response as CheckSignResponse;
            if (res != null)
            {
                return new EthereumResponse<bool>
                {
                    Result = res.SignIsCorrect.GetValueOrDefault()

                };
            }

            throw new Exception("Unknown response");
        }

        public async Task<EthereumResponse<OperationResponse>> SendTransferWithChangeAsync(decimal change, string signFrom, Guid id, Asset asset, string fromAddress,
            string toAddress, decimal amount)
        {
            var response = await _ethereumApi.ApiExchangeTransferWithChangePostAsync(new TransferWithChangeModel
            {
                Change = EthServiceHelpers.ConvertToContract(change, asset.MultiplierPower, asset.Accuracy),
                Amount = EthServiceHelpers.ConvertToContract(amount, asset.MultiplierPower, asset.Accuracy),
                SignFrom = signFrom,
                CoinAdapterAddress = asset.AssetAddress,
                ToAddress = toAddress,
                FromAddress = fromAddress,
                Id = id
            });

            var error = response as ApiException;
            if (error != null)
            {
                return new EthereumResponse<OperationResponse>
                {
                    Error = new ErrorResponse { Code = error.Error.Code, Message = error.Error.Message }
                };
            }

            var res = response as OperationIdResponse;
            if (res != null)
            {
                return new EthereumResponse<OperationResponse> { Result = new OperationResponse { OperationId = res.OperationId } };
            }

            throw new Exception("Unknown response");
        }

        public async Task<EthereumResponse<bool>> IsSynced(string coinAdapterAddress, string userAddress)
        {
            var response = await _ethereumApi.ApiExchangeCheckPendingTransactionPostAsync(new CheckPendingModel
            {
                UserAddress = userAddress,
                CoinAdapterAddress = coinAdapterAddress
            });

            var error = response as ApiException;
            if (error != null)
            {
                return new EthereumResponse<bool>
                {
                    Error = new ErrorResponse { Code = error.Error.Code, Message = error.Error.Message }
                };
            }

            var res = response as CheckPendingResponse;
            if (res != null)
            {
                return new EthereumResponse<bool>
                {
                    Result = res.IsSynced

                };
            }

            throw new Exception("Unknown response");
        }

        private static EthereumInternalMessageHistorical MapInternalMessageResponseToModel(InternalMessageResponse message)
        {
            return new EthereumInternalMessageHistorical()
            {
                BlockNumber = (ulong)message.BlockNumber.Value,
                Depth = message.Depth.Value,
                FromAddress = message.FromAddress,
                MessageIndex = message.MessageIndex.Value,
                ToAddress = message.ToAddress,
                TransactionHash = message.TransactionHash,
                Type = message.Type,
                Value = message.Value,
                BlockTimestamp = (uint)message.BlockTimestamp,
                BlockTimeUtc = message.BlockTimeUtc.Value
            };
        }

        private static EthereumTransactionHistorical MapTransactionResponseToModel(TransactionResponse transaction)
        {
            return new EthereumTransactionHistorical()
            {
                BlockHash = transaction.BlockHash,
                BlockNumber = transaction.BlockNumber.Value,
                BlockTimestamp = transaction.BlockTimestamp.Value,
                BlockTimeUtc = transaction.BlockTimeUtc.Value,
                ContractAddress = transaction.ContractAddress,
                From = transaction.FromProperty,
                Gas = transaction.Gas,
                GasPrice = transaction.GasPrice,
                GasUsed = transaction.GasUsed,
                Nonce = transaction.Nonce,
                To = transaction.To,
                TransactionHash = transaction.TransactionHash,
                TransactionIndex = transaction.TransactionIndex.Value,
                Value = transaction.Value,
                HasError = transaction.HasError.Value,
                ErcTransfers = transaction.ErcTransfer.Select(x => MapEthereumAddressHistory(x))
            };
        }

        public async Task<Asset> GetEthAsset()
        {
            var dictionary = await _tradableAssetsDict.GetDictionaryAsync();
            var ethAsset = dictionary.Where(x => x.Value?.BlockChainAssetId == LykkeConstants.EthAssetId)?.FirstOrDefault().Value;

            return ethAsset;
        }
    }

    public static class EthServiceHelpers
    {
        public static int CalculateSign(string address, string from, string to)
        {
            int sign = 1;//incoming transaction;
            if (from?.ToLower() == address.ToLower())
            {
                sign = -1;
            }
            else if (from?.ToLower() == to?.ToLower())
            {
                sign = 0;
            }

            return sign;
        }

        public static string ConvertToContract(decimal amount, int multiplier, int accuracy)
        {
            if (accuracy > multiplier)
                throw new ArgumentException("accuracy > multiplier");

            amount *= (decimal)Math.Pow(10, accuracy);
            multiplier -= accuracy;
            var res = (BigInteger)amount * BigInteger.Pow(10, multiplier);

            return res.ToString();
        }

        public static decimal ConvertFromContract(string amount, int multiplier, int accuracy)
        {
            if (accuracy > multiplier)
                throw new ArgumentException("accuracy > multiplier");

            multiplier -= accuracy;

            var val = BigInteger.Parse(amount);
            var res = (decimal)(val / BigInteger.Pow(10, multiplier));
            res /= (decimal)Math.Pow(10, accuracy);

            return res;
        }


    }
}
