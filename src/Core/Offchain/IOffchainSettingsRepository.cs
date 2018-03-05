using System;
using System.Collections.Generic;
using System.Drawing.Text;
using System.Text;
using System.Threading.Tasks;

namespace Core.Offchain
{
    public interface IOffchainSettingsRepository
    {
        Task<T> Get<T>(string key);
        Task<T> Get<T>(string key, T defaultValue);
        Task Set<T>(string key, T value);
    }
}
