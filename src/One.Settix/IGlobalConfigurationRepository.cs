using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace One.Settix
{
    public interface IGlobalConfigurationRepository
    {
        Task<string> GetGlobalAsync(string key);
        Task SetGlobalAsync(string key, string value);
        Task DeleteGlobalAsync(string key);
        Task<bool> ExistsGlobalAsync(string key);
        IEnumerable<DeployedSetting> GetAllGlobal();
    }
}
