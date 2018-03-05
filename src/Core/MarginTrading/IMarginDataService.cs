using Core.MarginTrading.Models;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Core.MarginTrading
{
    public interface IMarginDataService
    {   
        Task AddOrEditTradingConditionAsync(TradingConditionRecord model);

        Task AddOrEditAccountGroupAsync(AccountGroupRecord model);
        Task<InitAccountGroupResponse> InitAccountGroup(string tradingConditionId, string baseAssetId);

        Task AssignInstrumentsAsync(string tradingConditionId, string baseAssetId, string[] instruments);
        Task AddOrEditAccountAssetAsync(AccountAssetRecord model);
        
        Task DeleteAccountAsync(string clientId, string accountId);
        Task AddAccountAsync(AccountRecord model);
        Task<string> InitAccounts(string clientId, string tradingConditions); 

        Task<bool> DepositToAccount(string clientId, string accountId, double amount, MarginPaymentType paymentType);
        Task<bool> WithdrawFromAccount(string clientId, string accountId, double amount, MarginPaymentType paymentType);
        Task<bool> ResetAccount(string clientId, string accountId);
        Task SetEnabled(string clientId, bool enabled);
        Task<bool> GetEnabled(string clientId);
        
        Task EditMatchingEngineRoute(string id, NewMatchingEngineRouteRecord route);
        Task AddMatchingEngineRoute(NewMatchingEngineRouteRecord route);
        Task DeleteMatchingEngineRoute(string id);

        Task<MarginTradingAccountBackendRecord> GetAccountDetailsAsync(string clientId, string accountId);

        Task<AccountsMarginLevelResult> GetAccountsManagementMarginLevels(double threshold);
        Task<IEnumerable<AccountsCloseAccountPositionsResult>> AccountsManagementClosePositions(IList<string> accountIds, bool ignoreMarginLevel);

        Task EnableMaintenanceMode(string userId, string reason);
        Task DisableMaintenanceMode(string userId, string reason);
        Task<MaintenaceModeStatus> GetMaintenanceModeStatus();

        Task<bool> BackendMaintenanceGet();
    }
    public class MaintenaceModeStatus
    {
        public bool IsBackendMaintenaceModeEnabled { get; set; }
        public IBackendIsAlive BackendIsAlive { get; set; }
        public bool IsFrontendMaintenaceModeEnabled { get; set; }
        public IFrontendIsAlive FrontendIsAlive { get; set; }
    }


    public class TradingConditionRecord
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public bool IsDefault { get; set; }
    }

    public class AccountGroupRecord
    {
        public string TradingConditionId { get; set; }
        public string BaseAssetId { get; set; }
        public decimal MarginCall { get; set; }
        public decimal StopOut { get; set; }
        public decimal DepositTransferLimit { get; set; }
        public decimal ProfitWithdrawalLimit { get; set; }        
    }

    public class AccountAssetRecord
    {
        public string TradingConditionId { get; set; }
        public string BaseAssetId { get; set; }
        public string Instrument { get; set; }
        public int LeverageInit { get; set; }
        public int LeverageMaintenance { get; set; }
        public double SwapLong { get; set; }
        public double SwapShort { get; set; }        
        public double CommissionLong { get; set; }
        public double CommissionShort { get; set; }
        public double CommissionLot { get; set; }
        public double DeltaBid { get; set; }
        public double DeltaAsk { get; set; }
        public double DealLimit { get; set; }
        public double PositionLimit { get; set; }
    }
       
    public class AccountRecord
    {
        public string Id { get; set; }
        public string ClientId { get; set; }
        public string TradingConditionId { get; set; }
        public string BaseAssetId { get; set; }
        public double Balance { get; set; }
        public double WithdrawTransferLimit { get; set; }
    }

    public enum MarginPaymentType
    {
        Transfer,
        Swift
    }

    public enum OrderDirection
    {
        Buy,
        Sell
    }
    public enum AssetType
    {
        Base,
        Quote
    }

    public class MatchingEngineRouteRecord
    {
        public string Id { get; set; }
        public int Rank { get; set; }
        public string TradingConditionId { get; set; }
        public string ClientId { get; set; }
        public string Instrument { get; set; }
        public OrderDirection? Type { get; set; }
        public string MatchingEngineId { get; set; }
        public string Asset { get; set; }     
    }
    public class NewMatchingEngineRouteRecord
    {        
        public int Rank { get; set; }
        public string TradingConditionId { get; set; }
        public string ClientId { get; set; }
        public string Instrument { get; set; }
        public OrderDirection? Type { get; set; }
        public string MatchingEngineId { get; set; }
        public string Asset { get; set; }        
    }
    public enum OrderStatus
    {        
        WaitingForExecution,
        Active,
        Closed,
        Rejected,
        Closing
    }
    public enum OrderCloseReason
    {   
        None,
        Close,
        StopLoss,
        TakeProfit,
        StopOut,
        Canceled,
        CanceledBySystem,
        ClosedByBroker
    }
    public enum OrderRejectReason
    {
        None,
        NoLiquidity,
        NotEnoughBalance,
        LeadToStopOut,
        AccountInvalidState,
        InvalidExpectedOpenPrice,
        InvalidVolume,
        InvalidTakeProfit,
        InvalidStoploss,
        InvalidInstrument,
        InvalidAccount,
        TradingConditionError,
        TechnicalError
    }


    public class ClientOrdersBackendRecord
    {        
        public IEnumerable<OrderBackendRecord> Positions { get; set; }
        public IEnumerable<OrderBackendRecord> Orders { get; set; }
    }
    public class OrderBackendRecord
    {        
        public string Id { get; set; }
        public string AccountId { get; set; }
        public string Instrument { get; set; }
        public OrderDirection? Type { get; set; }
        public OrderStatus? Status { get; set; }
        public OrderCloseReason? CloseReason { get; set; }
        public OrderRejectReason? RejectReason { get; set; }
        public string RejectReasonText { get; set; }
        public double? ExpectedOpenPrice { get; set; }
        public double? OpenPrice { get; set; }
        public double? ClosePrice { get; set; }
        public DateTime? OpenDate { get; set; }
        public DateTime? CloseDate { get; set; }
        public double? Volume { get; set; }
        public double? MatchedVolume { get; set; }
        public double? MatchedCloseVolume { get; set; }
        public double? TakeProfit { get; set; }
        public double? StopLoss { get; set; }
        public double? Fpl { get; set; }
        public double? OpenCommission { get; set; }
        public double? CloseCommission { get; set; }
        public double? SwapCommission { get; set; }
    }

    public enum AccountHistoryType
    {
        Deposit,
        Withdraw,
        OrderClosed,
        Reset
    }    
    public class AccountHistoryInfoRecord
    {        
        public IEnumerable<AccountHistoryBackendRecord> Account { get; set; }                
        public IEnumerable<OrderHistoryBackendRecord> PositionsHistory { get; set; }
        public IEnumerable<OrderHistoryBackendRecord> OpenPositions { get; set; }
    }

    public class AccountHistoryBackendRecord
    {
        public string Id { get; set; }
        public DateTime? Date { get; set; }
        public string AccountId { get; set; }
        public string ClientId { get; set; }
        public double? Amount { get; set; }
        public double? Balance { get; set; }
        public double? WithdrawTransferLimit { get; set; }
        public string Comment { get; set; }
        public AccountHistoryType? Type { get; set; }
    }
    public class OrderHistoryBackendRecord
    {   
        public string Id { get; set; }
        public string AccountId { get; set; }
        public string Instrument { get; set; }
        public int? AssetAccuracy { get; set; }
        public OrderDirection? Type { get; set; }
        public OrderStatus? Status { get; set; }
        public OrderCloseReason? CloseReason { get; set; }
        public DateTime? OpenDate { get; set; }
        public DateTime? CloseDate { get; set; }
        public double? OpenPrice { get; set; }
        public double? ClosePrice { get; set; }
        public double? Volume { get; set; }
        public double? TakeProfit { get; set; }
        public double? StopLoss { get; set; }
        public double? TotalPnl { get; set; }
        public double? Pnl { get; set; }
        public double? InterestRateSwap { get; set; }
        public double? OpenCommission { get; set; }
        public double? CloseCommission { get; set; }
    }

    public class MarginTradingAccountBackendRecord
    {
        public string Id { get; set; }
        public string ClientId { get; set; }
        public string TradingConditionId { get; set; }
        public string BaseAssetId { get; set; }
        public double? Balance { get; set; }
        public double? WithdrawTransferLimit { get; set; }
        public double? MarginCall { get; set; }
        public double? StopOut { get; set; }
        public double? TotalCapital { get; set; }
        public double? FreeMargin { get; set; }
        public double? MarginAvailable { get; set; }
        public double? UsedMargin { get; set; }
        public double? MarginInit { get; set; }
        public double? PnL { get; set; }
        public double? OpenPositionsCount { get; set; }
        public double? MarginUsageLevel { get; set; }
        public bool? IsLive { get; set; }

    }


    public class MatchedOrderBackendRecord
    {
        public string OrderId { get; set; }
        public string MarketMakerId { get; set; }
        public double? LimitOrderLeftToMatch { get; set; }
        public double? Volume { get; set; }
        public double? Price { get; set; }
        public DateTime? MatchedDate { get; set; }
    }

    public class AccountsMarginLevelResult
    {       
        public IList<AccountsMarginLevelRecord> Levels { get; set; }
        public DateTime DateTime { get; set; }
    }
    public class AccountsMarginLevelRecord
    {
        public string AccountId { get; set; }
        public string ClientId { get; set; }
        public string TradingConditionId { get; set; }
        public string BaseAssetId { get; set; }
        public double Balance { get; set; }
        public double MarginLevel { get; set; }
        public int OpenedPositionsCount { get; set; }
        public double UsedMargin { get; set; }
        public double TotalBalance { get; set; }
    }
    public class AccountsCloseAccountPositionsResult
    {
        public string AccountId { get; set; }
        public IList<OrderFullRecord> ClosedPositions { get; set; }
        public string ErrorMessage { get; set; }
    }
    public class OrderFullRecord
    {
        public string TradingConditionId { get; set; }
        public double QuoteRate { get; set; }
        public int AssetAccuracy { get; set; }
        public double CommissionLot { get; set; }
        public DateTime? StartClosingDate { get; set; }
        public OrderFillType FillType { get; set; }
        public string Comment { get; set; }
        public double InterestRateSwap { get; set; }
        public double MarginInit { get; set; }
        public double MarginMaintenance { get; set; }
        public double OpenCrossPrice { get; set; }
        public double CloseCrossPrice { get; set; }
        public string Id { get; set; }
        public string ClientId { get; set; }
        public string AccountId { get; set; }
        public string AccountAssetId { get; set; }
        public string Instrument { get; set; }
        public OrderDirection Type { get; set; }
        public OrderStatus Status { get; set; }
        public OrderCloseReason CloseReason { get; set; }
        public OrderRejectReason RejectReason { get; set; }
        public string RejectReasonText { get; set; }
        public double? ExpectedOpenPrice { get; set; }
        public double OpenPrice { get; set; }
        public double ClosePrice { get; set; }
        public DateTime CreateDate { get; set; }
        public DateTime? OpenDate { get; set; }
        public DateTime? CloseDate { get; set; }
        public double Volume { get; set; }
        public double MatchedVolume { get; set; }
        public double MatchedCloseVolume { get; set; }
        public double? TakeProfit { get; set; }
        public double? StopLoss { get; set; }
        public double Fpl { get; set; }
        public double PnL { get; set; }
        public double OpenCommission { get; set; }
        public double CloseCommission { get; set; }
        public double SwapCommission { get; set; }
        public IList<MatchedOrderBackendRecord> MatchedOrders { get; set; }
        public IList<MatchedOrderBackendRecord> MatchedCloseOrders { get; set; }
    }

    public enum OrderFillType
    {
        FillOrKill,
        PartialFill
    }

    public class InitAccountGroupResponse
    {
        public string Message { get; set; }
        public IEnumerable<MarginTradingAccountResponse> Response { get; set; }
    }
    public class MarginTradingAccountResponse
    {
        public string Id { get; set; }
        public string ClientId { get; set; }
        public string TradingConditionId { get; set; }
        public string BaseAssetId { get; set; }
        public double Balance { get; set; }
        public double WithdrawTransferLimit { get; set; }
    }
}
