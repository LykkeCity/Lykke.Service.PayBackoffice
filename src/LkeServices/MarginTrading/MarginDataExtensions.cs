using LkeServices.Generated.MarginApi.Models;
using System;
using System.Linq;

namespace LkeServices.MarginTrading
{
    internal static class MarginDataExtensions
    {
        public static TradingConditionModel ConvertToDomain(this Core.MarginTrading.TradingConditionRecord condition)
        {
            return new TradingConditionModel
            {
                Id = condition.Id,
                Name = condition.Name,
                IsDefault = condition.IsDefault
            };
        }

        public static AccountGroupModel ConvertToDomain(this Core.MarginTrading.AccountGroupRecord group)
        {
            return new AccountGroupModel
            {
                BaseAssetId = group.BaseAssetId,
                MarginCall = Convert.ToDouble(group.MarginCall),
                StopOut = Convert.ToDouble(group.StopOut),
                TradingConditionId = group.TradingConditionId,
                DepositTransferLimit = Convert.ToDouble(group.DepositTransferLimit),
                ProfitWithdrawalLimit = Convert.ToDouble(group.ProfitWithdrawalLimit)
            };
        }

        public static AccountAssetPairModel ConvertToDomain(this Core.MarginTrading.AccountAssetRecord asset)
        {
            return new AccountAssetPairModel
            {
                TradingConditionId = asset.TradingConditionId,
                BaseAssetId = asset.BaseAssetId,
                Instrument = asset.Instrument,
                LeverageInit = asset.LeverageInit,
                LeverageMaintenance = asset.LeverageMaintenance,
                SwapLong = asset.SwapLong,
                SwapShort = asset.SwapShort,
                CommissionLong = asset.CommissionLong,
                CommissionShort = asset.CommissionShort,
                CommissionLot = asset.CommissionLot,
                DeltaBid = asset.DeltaBid,
                DeltaAsk = asset.DeltaAsk,
                PositionLimit = asset.PositionLimit,
                DealLimit = asset.DealLimit
            };
        }

        public static Core.MarginTrading.AccountRecord ConvertToDto(this MarginTradingAccount account)
        {
            return new Core.MarginTrading.AccountRecord
            {

                Id = account.Id,
                ClientId = account.ClientId,
                TradingConditionId = account.TradingConditionId,
                BaseAssetId = account.BaseAssetId,
                Balance = account.Balance,
                WithdrawTransferLimit = account.WithdrawTransferLimit
            };
        }

        public static MarginTradingAccount ConvertToDomain(this Core.MarginTrading.AccountRecord account)
        {
            return new MarginTradingAccount
            {
                Id = account.Id,
                ClientId = account.ClientId,
                TradingConditionId = account.TradingConditionId,
                BaseAssetId = account.BaseAssetId,
                Balance = account.Balance,
                WithdrawTransferLimit = account.WithdrawTransferLimit
            };
        }

        public static PaymentType ConvertToDomain(this Core.MarginTrading.MarginPaymentType paymentType)
        {
            switch (paymentType)
            {
                case Core.MarginTrading.MarginPaymentType.Swift:
                    return PaymentType.Swift;

                case Core.MarginTrading.MarginPaymentType.Transfer:
                    return PaymentType.Transfer;
            }

            throw new ArgumentException($"Unsupported payment type value [{paymentType}]");
        }

        public static Core.MarginTrading.MatchingEngineRouteRecord ConvertToDto(this MatchingEngineRoute route)
        {
            return new Core.MarginTrading.MatchingEngineRouteRecord
            {
                Asset = route.Asset,
                ClientId = route.ClientId,
                Id = route.Id,
                Instrument = route.Instrument,
                MatchingEngineId = route.MatchingEngineId,
                Rank = route.Rank,
                TradingConditionId = route.TradingConditionId,
                Type = route.Type.ConvertToDto()
            };
        }
        public static NewMatchingEngineRouteRequest ConverToDomain(this Core.MarginTrading.NewMatchingEngineRouteRecord route)
        {
            return new NewMatchingEngineRouteRequest()
            {
                Asset = route.Asset,
                ClientId = route.ClientId,
                Instrument = route.Instrument,
                MatchingEngineId = route.MatchingEngineId,
                Rank = route.Rank,
                TradingConditionId = route.TradingConditionId,
                Type = route.Type.ConvertToDomainContract()
            };
        }
        
        public static Core.MarginTrading.OrderDirection? ConvertToDto(this OrderDirection? orderDirection)
        {
            switch (orderDirection)
            {
                case null:
                    return null;
                case OrderDirection.Buy:
                    return Core.MarginTrading.OrderDirection.Buy;
                case OrderDirection.Sell:
                    return Core.MarginTrading.OrderDirection.Sell;
                default:
                    break;
            }

            throw new ArgumentException($"Unsupported order direction type value [{orderDirection}]");
        }
        public static OrderDirection? ConvertToDomain(this Core.MarginTrading.OrderDirection? orderDirection)
        {
            switch (orderDirection)
            {
                case null:
                    return null;
                case Core.MarginTrading.OrderDirection.Buy:
                    return OrderDirection.Buy;
                case Core.MarginTrading.OrderDirection.Sell:
                    return OrderDirection.Sell;
                default:
                    break;
            }

            throw new ArgumentException($"Unsupported order direction type value [{orderDirection}]");
        }
        public static OrderDirectionContract? ConvertToDomainContract(this Core.MarginTrading.OrderDirection? orderDirection)
        {
            switch (orderDirection)
            {
                case null:
                    return null;
                case Core.MarginTrading.OrderDirection.Buy:
                    return OrderDirectionContract.Buy;
                case Core.MarginTrading.OrderDirection.Sell:
                    return OrderDirectionContract.Sell;
                default:
                    break;
            }

            throw new ArgumentException($"Unsupported order direction type value [{orderDirection}]");
        }

        public static Core.MarginTrading.MarginTradingAccountBackendRecord ConvertToDto(this MarginTradingAccountBackendContract source)
        {
            return new Core.MarginTrading.MarginTradingAccountBackendRecord
            {
                Balance = source.Balance,
                BaseAssetId = source.BaseAssetId,
                ClientId = source.ClientId,
                FreeMargin = source.FreeMargin,
                Id = source.Id,
                IsLive = source.IsLive,
                MarginAvailable = source.MarginAvailable,
                MarginCall = source.MarginCall,
                MarginInit = source.MarginInit,
                MarginUsageLevel = source.MarginUsageLevel,
                OpenPositionsCount = source.OpenPositionsCount,
                PnL = source.PnL,
                StopOut = source.StopOut,
                TotalCapital = source.TotalCapital,
                TradingConditionId = source.TradingConditionId,
                UsedMargin = source.UsedMargin,
                WithdrawTransferLimit = source.WithdrawTransferLimit
            };
        }

        public static Core.MarginTrading.AccountsMarginLevelResult ConvertToDto(this AccountsMarginLevelResponse source)
        {
            return new Core.MarginTrading.AccountsMarginLevelResult
            {
                DateTime = source.DateTime,
                Levels = source.Levels.Select(x => x.ConvertToDto())
                    .ToList()
            };
        }
        public static Core.MarginTrading.AccountsMarginLevelRecord ConvertToDto(this AccountsMarginLevelContract source)
        {
            return new Core.MarginTrading.AccountsMarginLevelRecord
            {
                AccountId = source.AccountId,
                Balance = source.Balance,
                BaseAssetId = source.BaseAssetId,
                ClientId = source.ClientId,
                MarginLevel = source.MarginLevel,
                OpenedPositionsCount = source.OpenedPositionsCount,
                TradingConditionId = source.TradingConditionId,
                TotalBalance = source.TotalBalance,
                UsedMargin = source.UsedMargin
            };
        }
        public static Core.MarginTrading.AccountsCloseAccountPositionsResult ConvertToDto(this CloseAccountPositionsResult s)
        {
            return new Core.MarginTrading.AccountsCloseAccountPositionsResult
            {
                AccountId = s.AccountId,
                ErrorMessage = s.ErrorMessage,
                ClosedPositions = s.ClosedPositions.Select(x => x.ConvertToDto())
                    .ToList()
            };
        }
        public static Core.MarginTrading.OrderFullRecord ConvertToDto(this OrderFullContract s)
        {
            return new Core.MarginTrading.OrderFullRecord
            {
                AccountAssetId = s.AccountAssetId,
                AccountId = s.AccountId,
                AssetAccuracy = s.AssetAccuracy,
                ClientId = s.ClientId,
                CloseCommission = s.CloseCommission,
                CloseCrossPrice = s.CloseCrossPrice,
                CloseDate = s.CloseDate,
                ClosePrice = s.ClosePrice,
                CloseReason = s.CloseReason.ConvertToDto(),
                Comment = s.Comment,
                CommissionLot = s.CommissionLot,
                CreateDate = s.CreateDate,
                ExpectedOpenPrice = s.ExpectedOpenPrice,
                FillType = s.FillType.ConvertToDto(),
                Fpl = s.Fpl,
                Id = s.Id,
                Instrument = s.Instrument,
                InterestRateSwap = s.InterestRateSwap,
                MarginInit = s.MarginInit,
                MarginMaintenance = s.MarginMaintenance,
                MatchedCloseOrders = s.MatchedCloseOrders.Select(x => x.ConvertToDto())
                    .ToList(),
                MatchedCloseVolume = s.MatchedCloseVolume,
                MatchedOrders = s.MatchedOrders.Select(x => x.ConvertToDto())
                    .ToList(),
                MatchedVolume = s.MatchedVolume,
                OpenCommission = s.OpenCommission,
                OpenCrossPrice = s.OpenCrossPrice,
                OpenDate = s.OpenDate,
                OpenPrice = s.OpenPrice,
                PnL = s.PnL,
                QuoteRate = s.QuoteRate,
                RejectReason = s.RejectReason.ConvertToDto(),
                RejectReasonText = s.RejectReasonText,
                StartClosingDate = s.StartClosingDate,
                Status = s.Status.ConvertToDto(),
                StopLoss = s.StopLoss,
                SwapCommission = s.SwapCommission,
                TakeProfit = s.TakeProfit,
                TradingConditionId = s.TradingConditionId,
                Type = s.Type.ConvertToDto(),
                Volume = s.Volume
            };
        }
        public static Core.MarginTrading.MatchedOrderBackendRecord ConvertToDto(this MatchedOrderBackendContract s)
        {
            return new Core.MarginTrading.MatchedOrderBackendRecord
            {
                LimitOrderLeftToMatch = s.LimitOrderLeftToMatch,
                MarketMakerId = s.MarketMakerId,
                MatchedDate = s.MatchedDate,
                OrderId = s.OrderId,
                Price = s.Price,
                Volume = s.Volume
            };
        }

        public static Core.MarginTrading.OrderCloseReason ConvertToDto(this OrderCloseReasonContract src)
        {
            switch (src)
            {
                case OrderCloseReasonContract.None:
                    return Core.MarginTrading.OrderCloseReason.None;
                case OrderCloseReasonContract.Close:
                    return Core.MarginTrading.OrderCloseReason.Close;
                case OrderCloseReasonContract.StopLoss:
                    return Core.MarginTrading.OrderCloseReason.StopLoss;
                case OrderCloseReasonContract.TakeProfit:
                    return Core.MarginTrading.OrderCloseReason.TakeProfit;
                case OrderCloseReasonContract.StopOut:
                    return Core.MarginTrading.OrderCloseReason.StopOut;
                case OrderCloseReasonContract.Canceled:
                    return Core.MarginTrading.OrderCloseReason.Canceled;
                case OrderCloseReasonContract.CanceledBySystem:
                    return Core.MarginTrading.OrderCloseReason.CanceledBySystem;
                case OrderCloseReasonContract.ClosedByBroker:
                    return Core.MarginTrading.OrderCloseReason.ClosedByBroker;
            }
            throw new ArgumentException($"Unsupported Order Close Reason [{src}]");
        }
        public static Core.MarginTrading.OrderFillType ConvertToDto(this OrderFillTypeContract src)
        {  
            switch (src)
            {
                case OrderFillTypeContract.FillOrKill:
                    return Core.MarginTrading.OrderFillType.FillOrKill;
                case OrderFillTypeContract.PartialFill:
                    return Core.MarginTrading.OrderFillType.PartialFill;
            }
            throw new ArgumentException($"Unsupported Order Fill Type [{src}]");
        }
        public static Core.MarginTrading.OrderRejectReason ConvertToDto(this OrderRejectReasonContract src)
        {
            switch (src)
            {
                case OrderRejectReasonContract.None:
                    return Core.MarginTrading.OrderRejectReason.None;
                case OrderRejectReasonContract.NoLiquidity:
                    return Core.MarginTrading.OrderRejectReason.NoLiquidity;
                case OrderRejectReasonContract.NotEnoughBalance:
                    return Core.MarginTrading.OrderRejectReason.NotEnoughBalance;
                case OrderRejectReasonContract.LeadToStopOut:
                    return Core.MarginTrading.OrderRejectReason.LeadToStopOut;
                case OrderRejectReasonContract.AccountInvalidState:
                    return Core.MarginTrading.OrderRejectReason.AccountInvalidState;
                case OrderRejectReasonContract.InvalidExpectedOpenPrice:
                    return Core.MarginTrading.OrderRejectReason.InvalidExpectedOpenPrice;
                case OrderRejectReasonContract.InvalidVolume:
                    return Core.MarginTrading.OrderRejectReason.InvalidVolume;
                case OrderRejectReasonContract.InvalidTakeProfit:
                    return Core.MarginTrading.OrderRejectReason.InvalidTakeProfit;
                case OrderRejectReasonContract.InvalidStoploss:
                    return Core.MarginTrading.OrderRejectReason.InvalidStoploss;
                case OrderRejectReasonContract.InvalidInstrument:
                    return Core.MarginTrading.OrderRejectReason.InvalidInstrument;
                case OrderRejectReasonContract.InvalidAccount:
                    return Core.MarginTrading.OrderRejectReason.InvalidAccount;
                case OrderRejectReasonContract.TradingConditionError:
                    return Core.MarginTrading.OrderRejectReason.TradingConditionError;
                case OrderRejectReasonContract.TechnicalError:
                    return Core.MarginTrading.OrderRejectReason.TechnicalError;
            }
            throw new ArgumentException($"Unsupported Order Reject Reason [{src}]");
        }
        public static Core.MarginTrading.OrderStatus ConvertToDto(this OrderStatusContract src)
        {
            switch (src)
            {
                case OrderStatusContract.WaitingForExecution:
                    return Core.MarginTrading.OrderStatus.WaitingForExecution;
                case OrderStatusContract.Active:
                    return Core.MarginTrading.OrderStatus.Active;
                case OrderStatusContract.Closed:
                    return Core.MarginTrading.OrderStatus.Closed;
                case OrderStatusContract.Rejected:
                    return Core.MarginTrading.OrderStatus.Rejected;
                case OrderStatusContract.Closing:
                    return Core.MarginTrading.OrderStatus.Closing;
            }
            throw new ArgumentException($"Unsupported Order Status [{src}]");
        }
        public static Core.MarginTrading.OrderDirection ConvertToDto(this OrderDirectionContract src)
        {   
            switch (src)
            {
                case OrderDirectionContract.Buy:
                    return Core.MarginTrading.OrderDirection.Buy;
                case OrderDirectionContract.Sell:
                    return Core.MarginTrading.OrderDirection.Sell;
            }
            throw new ArgumentException($"Unsupported Order Direction [{src}]");
        }
        
        public static Core.MarginTrading.InitAccountGroupResponse ConvertToDto(this MtBackendResponseIEnumerableMarginTradingAccountModel src)
        {
            return new Core.MarginTrading.InitAccountGroupResponse
            {
                Message = src.Message,
                Response = src.Result?.Select(x => x.ConvertToDto())
            };
        }
        public static Core.MarginTrading.MarginTradingAccountResponse ConvertToDto(this MarginTradingAccountModel src)
        {
            return new Core.MarginTrading.MarginTradingAccountResponse
            {
                Id = src.Id,
                TradingConditionId = src.TradingConditionId,
                BaseAssetId = src.BaseAssetId,
                ClientId = src.ClientId,
                Balance = src.Balance,
                WithdrawTransferLimit = src.WithdrawTransferLimit
            };
        }
    }
}
