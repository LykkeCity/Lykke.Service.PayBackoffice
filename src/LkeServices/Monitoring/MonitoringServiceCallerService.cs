using Core.Monitoring;
using Lykke.MonitoringServiceApiCaller;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace LkeServices.Monitoring
{
    public class MonitoringServiceCallerService : IMonitoringServiceCallerService
    {
        private MonitoringServiceFacade _monitoringServiceFacade;

        public MonitoringServiceCallerService(MonitoringServiceFacade monitoringServiceFacade)
        {
            _monitoringServiceFacade = monitoringServiceFacade;
        }

        public async Task<IEnumerable<IMonitoringRecordExtended>> GetAllAsync()
        {
            var allServices = await _monitoringServiceFacade.GetAll();
            var orderedByName = allServices?.OrderBy(x => x.ServiceName);
            List<IMonitoringRecordExtended> records = new List<IMonitoringRecordExtended>(allServices?.Count() ?? 0);
            foreach (var service in orderedByName)
            {
                records.Add(new MonitoringRecordExtended()
                {
                    DateTime = service.LastPing ?? DateTime.MinValue,
                    ServiceName = service.ServiceName,
                    Version = service.Version,
                    SkipUntil = service.SkipUntil,
                    Url = service.Url
                });
            }

            return records;
        }

        public async Task<IMonitoringRecordExtended> GetServiceAsync(string serviceName)
        {
            var record = await _monitoringServiceFacade.GetService(serviceName);
            return new MonitoringRecordExtended()
            {
                DateTime = record.LastPing ?? DateTime.MinValue,
                ServiceName = record.ServiceName,
                Version = record.Version,
                Url = record.Url,
                SkipUntil = record.SkipUntil,
            };
        }

        public async Task MuteServiceAsync(string serviceName, int minutes)
        {
            await _monitoringServiceFacade.Mute(new Lykke.MonitoringServiceApiCaller.Models.MonitoringObjectMuteModel()
            {
                Minutes = minutes,
                ServiceName = serviceName
            });
        }

        public async Task UnMuteServiceAsync(string serviceName)
        {
            await _monitoringServiceFacade.Unmute(new Lykke.MonitoringServiceApiCaller.Models.MonitoringObjectUnmuteModel()
            {
                ServiceName = serviceName
            });
        }

        public async Task RemoveUrlFromMonitoring(string serviceName)
        {
            await _monitoringServiceFacade.RemoveService(serviceName);
        }

        public async Task AddUrlToMonitoringAsync(string serviceName, string url)
        {
            await _monitoringServiceFacade.MonitorUrl(new Lykke.MonitoringServiceApiCaller.Models.UrlMonitoringObjectModel()
            {
                ServiceName = serviceName,
                Url = url
            });
        }
    }
}
