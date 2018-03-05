using Common;
using Common.Log;
using Core.MarginTrading;
using Core.MarginTrading.Models;
using Core.MarginTrading.Repositories;
using LkeServices.Generated.MarginApi;
using LkeServices.Generated.MarginApi.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace LkeServices.MarginTrading
{
    public class MarginDataServiceSettings
    {
        public Uri BaseUri { get; set; }
        public string ApiKey { get; set; }
        public string FrontendUrl { get; set; }
    }

    public class MarginDataService : IMarginDataService
    {
        private readonly MarginDataServiceSettings _settings;
        private readonly IMaintenanceInfoRepository _maintenanceInfoRepository;
        private readonly bool _isDemo;
        private readonly ILog _log;

        public MarginDataService(MarginDataServiceSettings settings, IMaintenanceInfoRepository maintenanceInfoRepository, bool isDemo, ILog log)
        {
            _settings = settings;
            _maintenanceInfoRepository = maintenanceInfoRepository;
            _isDemo = isDemo;
            _log = log;
        }

        private MarginTradingApi Api => new MarginTradingApi(_settings.BaseUri);
        
        public async Task<string> InitAccounts(string clientId, string tradingConditions)
        {
            var request = new InitAccountsRequest { ClientId = clientId, TradingConditionsId = tradingConditions };
            var result = await Api.ApiBackofficeMarginTradingAccountsInitPostWithHttpMessagesAsync(_settings.ApiKey,
                request);

            return result.Body.Message;
        }
        
        #region Trading conditions

      
        public async Task AddOrEditTradingConditionAsync(TradingConditionRecord model)
        {
            // Replaced Api.ApiBackofficeTradingConditionsAddPostAsync
            await Api.AddOrReplaceTradingConditionAsync(_settings.ApiKey, model.ConvertToDomain());
        }

        #endregion
        
        #region Account groups

        public async Task<InitAccountGroupResponse> InitAccountGroup(string tradingConditionId, string baseAssetId)
        {
            var request = new InitAccountGroupRequest
            {
                BaseAssetId = baseAssetId,
                TradingConditionId = tradingConditionId
            };
            var res = await Api.InitAccountGroupAsync(_settings.ApiKey, request);
            return (res.ConvertToDto());
        }
               
        public async Task AddOrEditAccountGroupAsync(AccountGroupRecord model)
        {
            // Replaced Api.ApiBackofficeAccountGroupsAddPostAsync
            await Api.AddOrReplaceAccountGroupAsync(_settings.ApiKey, model.ConvertToDomain());
        }

        #endregion
        
        #region Account assets
        
        public async Task AssignInstrumentsAsync(string tradingConditionId, string baseAssetId, string[] instruments)
        {
            // Replaced Api.ApiBackofficeAccountAssetsAssignInstrumentsPostAsync
            await Api.AssignInstrumentsAsync(_settings.ApiKey, new AssignInstrumentsRequest
            {
                TradingConditionId = tradingConditionId,
                BaseAssetId = baseAssetId,
                Instruments = instruments
            });
        }

        public async Task AddOrEditAccountAssetAsync(AccountAssetRecord model)
        {
            // replaced Api.ApiBackofficeAccountAssetsAddPostAsync(_settings.ApiKey, model.ConvertToDomain());
            await Api.AddOrReplaceAccountAssetAsync(_settings.ApiKey, model.ConvertToDomain());
        }


        #endregion
        
        #region Accounts
             
        public async Task DeleteAccountAsync(string clientId, string accountId)
        {
            await Api.ApiBackofficeMarginTradingAccountsDeleteByClientIdByAccountIdPostAsync(clientId, accountId, _settings.ApiKey);
        }

        public async Task AddAccountAsync(AccountRecord model)
        {
            await Api.ApiBackofficeMarginTradingAccountsAddPostAsync(_settings.ApiKey, model.ConvertToDomain());
        }

        public async Task<bool> DepositToAccount(string clientId, string accountId, double amount, MarginPaymentType paymentType)
        {
            var request = new AccountDepositWithdrawRequest
            {
                AccountId = accountId,
                ClientId = clientId,
                Amount = amount,
                PaymentType = paymentType.ConvertToDomain()
            };
            var result = await Api.ApiBackofficeMarginTradingAccountsDepositPostWithHttpMessagesAsync(_settings.ApiKey,
                request);

            return result.Body == true;
        }

        public async Task<bool> WithdrawFromAccount(string clientId, string accountId, double amount, MarginPaymentType paymentType)
        {
            var request = new AccountDepositWithdrawRequest
            {
                AccountId = accountId,
                ClientId = clientId,
                Amount = amount,
                PaymentType = paymentType.ConvertToDomain()
            };
            var result = await Api.ApiBackofficeMarginTradingAccountsWithdrawPostWithHttpMessagesAsync(_settings.ApiKey,
                request);

            return result.Body == true;
        }

        public async Task<bool> ResetAccount(string clientId, string accountId)
        {
            var request = new AccounResetRequest
            {
                AccountId = accountId,
                ClientId = clientId
            };
            var result = await Api.ApiBackofficeMarginTradingAccountsResetPostWithHttpMessagesAsync(_settings.ApiKey,
                request);

            return result.Body == true;
        }

        #endregion
        
        #region Setups

        public Task SetEnabled(string clientId, bool enabled)
        {
            return Api.ApiBackofficeSettingsEnabledByClientIdPostWithHttpMessagesAsync(clientId,
                _settings.ApiKey,
                enabled);
        }

        public async Task<bool> GetEnabled(string clientId)
        {
            var result = await Api.ApiBackofficeSettingsEnabledByClientIdGetWithHttpMessagesAsync(clientId,
                _settings.ApiKey);

            return result.Body == true;
        }

        #endregion
        
        #region MatchingEngineRoutes
       
        public async Task AddMatchingEngineRoute(NewMatchingEngineRouteRecord route)
        {
            await Api.ApiBackofficeRoutesPostWithHttpMessagesAsync(_settings.ApiKey, route.ConverToDomain());
        }
        public async Task EditMatchingEngineRoute(string id, NewMatchingEngineRouteRecord route)
        {            
            await Api.ApiBackofficeRoutesByIdPutWithHttpMessagesAsync(id, _settings.ApiKey, route.ConverToDomain());
        }
        public async Task DeleteMatchingEngineRoute(string id)
        {
            await Api.ApiBackofficeRoutesByIdDeleteAsync(id, _settings.ApiKey);
        }
        
        #endregion

        public async Task<MarginTradingAccountBackendRecord> GetAccountDetailsAsync(string clientId, string accountId)
        {
            var result = await Api.ApiAccountprofileAccountByClientIdByAccountIdGetAsync(clientId, accountId);
            return result.ConvertToDto();
        }

        #region Account Management
        public async Task<AccountsMarginLevelResult> GetAccountsManagementMarginLevels(double threshold)
        {
            return (await Api.ApiAccountsManagementMarginLevelsByThresholdGetAsync(threshold, _settings.ApiKey)).ConvertToDto();
        }
        public async Task<IEnumerable<AccountsCloseAccountPositionsResult>> AccountsManagementClosePositions(IList<string> accountIds, bool ignoreMarginLevel)
        {
            var request = new CloseAccountPositionsRequest
            {
                AccountIds = accountIds,
                IgnoreMarginLevel = ignoreMarginLevel
            };
            var res = await Api.ApiAccountsManagementClosePositionsPostAsync(_settings.ApiKey, request);
            return res.Results.Select(x => x.ConvertToDto());
        }

        #endregion

        #region Service

        public async Task<bool> BackendMaintenanceGet()
        {
            var res = await Api.ApiServiceMaintenanceGetAsync(_settings.ApiKey);
            if (res.Message != null)
                throw new Exception(res.Message);
            return res.Result;

        }
        private async Task<bool> BackendMaintenanceSet(bool enable)
        {
            var res = await Api.ApiServiceMaintenancePostAsync(_settings.ApiKey, enable);
            if (res.Message != null)
                throw new Exception(res.Message);
            
            return res.Result;
        }


        public async Task EnableMaintenanceMode(string userId, string reason)
        {
            /*
                When enable: frontend first and after it backend.
                If backend returned an error it is ok, because server may be already stopped.

                > FrontEnd via Storage Record
                    (Demo/Live = rowkey)
                > Backend via API 
                    - GET /api/service/maintenance
                    - POST /api/service/maintenance

                1. User clicks control to put Live or Demo to Maintenance mode.
                2. Some spinning stuff appears (or literally anything able to represent "wait,please", even "wait,please" text).
                3. BO updates MaintenanceInfo table,
                4. BO calls selected Backend: POST /api/service/maintenance.
                5. BO starts calling Frontend: GET /api/isAlive 10 times per second.
                6. As soon as BO receives 10 sequential responses in which selected backend's version contains "Maintenance", throw away the spinning stuff & show success.
             */

            // FrontEnd
            var maintenanceModeRecord = await _maintenanceInfoRepository.GetMaintenanceInfo(_isDemo);
            if (!maintenanceModeRecord.IsEnabled)
            {
                var newstatus = new MaintenanceInfo
                {
                    IsEnabled = true,
                    ChangedDate = DateTime.UtcNow,
                    ChangedBy = userId,
                    ChangedReason = reason
                };
                await _maintenanceInfoRepository.SetMaintenanceInfo(newstatus, _isDemo);
            }
            // Backend
            try { var backendInMaintenanceMode = BackendMaintenanceSet(true); }
            catch (Exception ex)
            {
                // Log error but continue
                await _log.WriteErrorAsync("EnableMaintenanceMode", null, ex);
            }

            // Ping Frontend IsAlive
            var maintenaceResponceCount = 0;
            var timeout = DateTime.UtcNow.AddSeconds(120);
            do
            {
                if (DateTime.UtcNow > timeout)
                    throw new Exception("Frontend IsAlive operation timeout");

                var isAlive = await GetFrontEndIsAlive();
                var inMaintenance = _isDemo
                    ? isAlive.DemoVersion.ToLowerInvariant().Contains("maintenance")
                    : isAlive.LiveVersion.ToLowerInvariant().Contains("maintenance");

                if (inMaintenance)
                    maintenaceResponceCount++;
                else
                    maintenaceResponceCount = 0;

                System.Threading.Thread.Sleep(100);
            } while (maintenaceResponceCount <= 10);
        }

        public async Task DisableMaintenanceMode(string userId, string reason)
        {
            /*
                When disable: backend first and after frontend only if backend returned ok 
                (to make sure that backend works when we disable maintenance on frontend).
             */

            // Backend
            var backendMaintenanceModeDisabled = await BackendMaintenanceSet(false);
            if (backendMaintenanceModeDisabled == false)
            {
                // Frontend
                var newstatus = new MaintenanceInfo
                {
                    IsEnabled = false,
                    ChangedDate = DateTime.UtcNow,
                    ChangedBy = userId,
                    ChangedReason = reason
                };
                await _maintenanceInfoRepository.SetMaintenanceInfo(newstatus, _isDemo);
            }
        }

        public async Task<MaintenaceModeStatus> GetMaintenanceModeStatus()
        {            
            var backendInMaintenance = await BackendMaintenanceGet();
            var frontendInMaintenance = (await _maintenanceInfoRepository.GetMaintenanceInfo(_isDemo)).IsEnabled;

            IsAliveResponse backEndIsAlive = null;
            if (!backendInMaintenance)
            {
                try { backEndIsAlive = await Api.ApiIsAliveGetAsync(); }
                catch { backEndIsAlive = null; }
            }

            FrontendIsAlive frontendIsAlive = null;
            try { frontendIsAlive = await GetFrontEndIsAlive(); }
            catch { frontendIsAlive = null; }

            var res = new MaintenaceModeStatus
            {
                IsBackendMaintenaceModeEnabled = backendInMaintenance,
                IsFrontendMaintenaceModeEnabled = frontendInMaintenance,
                BackendIsAlive = backEndIsAlive == null 
                    ? null
                    : new BackendIsAlive
                    {
                        Env = backEndIsAlive.Env,
                        MatchingEngineAlive = backEndIsAlive.MatchingEngineAlive,
                        ServerTime = backEndIsAlive.ServerTime,
                        TradingEngineAlive = backEndIsAlive.TradingEngineAlive,
                        Version = backEndIsAlive.Version
                    },
                FrontendIsAlive = frontendIsAlive,
            };
            return res;
        }
        #endregion

        private async Task<FrontendIsAlive> GetFrontEndIsAlive()
        {
            var version = (await new Http.HttpRequestClient().GetRequest(_settings.FrontendUrl + "IsAlive"))
                        .DeserializeJson<FrontendIsAlive>();
            return version;
        }
    }

   
}
