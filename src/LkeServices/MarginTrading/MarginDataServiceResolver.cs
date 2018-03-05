using System;
using Common.Log;
using Core.MarginTrading;
using Core.MarginTrading.Repositories;
using Core.Settings;
using MarginTrading.DataReaderClient;

namespace LkeServices.MarginTrading
{
    public class MarginDataServiceResolver : IMarginDataServiceResolver
    {
        private readonly MarginSettings _settings;        
        private readonly MarginTradingDataReaderApiClientsPair _marginTradingDataReaderHelper;
        private readonly IMaintenanceInfoRepository _maintenanceInfoRepository;
        private readonly ILog _log;

        public MarginDataServiceResolver(MarginSettings settings,            
            MarginTradingDataReaderApiClientsPair marginTradingDataReaderHelper,
            IMaintenanceInfoRepository maintenanceInfoRepository,
            ILog log)
        {
            _settings = settings;            
            _marginTradingDataReaderHelper = marginTradingDataReaderHelper;
            _maintenanceInfoRepository = maintenanceInfoRepository;
            _log = log;
        }

        public IMarginDataService Resolve(bool isDemo)
        {
            var serviceSettings = new MarginDataServiceSettings
            {
                FrontendUrl = _settings.ApiUrl
            };

            if (isDemo)
            {
                serviceSettings.ApiKey = _settings.DemoApiKey;
                serviceSettings.BaseUri = new Uri(_settings.DemoApiRootUrl);
            }
            else
            {
                serviceSettings.ApiKey = _settings.ApiKey;
                serviceSettings.BaseUri = new Uri(_settings.ApiRootUrl);
            }

            return new MarginDataService(serviceSettings, _maintenanceInfoRepository, isDemo, _log);
        }
        public IMarginTradingDataReaderApiClient GetDataReader(bool isDemo)
        {
            if (isDemo)
                return _marginTradingDataReaderHelper.Demo;
            else
                return _marginTradingDataReaderHelper.Live;
        }
    }
}
