using System;
using System.Threading.Tasks;

namespace Core.EventLogs
{
    public interface ITempDataRepository
    {
        Task InsertOrReplaceDataAsync(BaseTempData data, string id = null);
        Task<T> RetrieveData<T>(string id = null) where T : BaseTempData;
    }

    public class BaseTempData { }

    public class IcoCoinsBoughtData : BaseTempData
    {
        public double MaxValue { get; set; }
    }

    public class BackupWarningTimeoutData : BaseTempData
    {
        public DateTime LastWarningDt { get; set; }
    }

    public static class Ext
    {
        public static async Task<bool> ShouldShowBackupWarning(this ITempDataRepository repo, string clientId, int timeoutInMinutes)
        {
            var lastWarningDt = (await repo.RetrieveData<BackupWarningTimeoutData>(clientId))?.LastWarningDt;
            if (lastWarningDt == null || DateTime.UtcNow - lastWarningDt > TimeSpan.FromMinutes(timeoutInMinutes))
            {
                await repo.InsertOrReplaceDataAsync(new BackupWarningTimeoutData {LastWarningDt = DateTime.UtcNow},
                    clientId);
                return true;
            }

            return false;
        }
    }
}
