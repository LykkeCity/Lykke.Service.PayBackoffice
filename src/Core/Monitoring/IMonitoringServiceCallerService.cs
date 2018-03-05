using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Core.Monitoring
{
    public interface IMonitoringRecordExtended : IMonitoringRecord
    {
        DateTime? SkipUntil { get; set; }
        string Url { get; set; }
    }

    public class MonitoringRecordExtended : IMonitoringRecordExtended
    {
        public DateTime DateTime { get; set; }

        public string ServiceName { get; set; }

        public string Version { get; set; }

        public DateTime? SkipUntil { get; set; }
        
        public string Url { get; set; }
    }



    public interface IMonitoringServiceCallerService
    {
        Task<IEnumerable<IMonitoringRecordExtended>> GetAllAsync();
        Task MuteServiceAsync(string serviceName, int minutes);
        Task UnMuteServiceAsync(string serviceName);
        Task RemoveUrlFromMonitoring(string serviceName);
        Task AddUrlToMonitoringAsync(string serviceName, string url);
        Task<IMonitoringRecordExtended> GetServiceAsync(string serviceName);
    }
}
