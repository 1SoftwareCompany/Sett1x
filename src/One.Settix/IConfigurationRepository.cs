﻿using System.Collections.Generic;
using System.Threading.Tasks;

namespace One.Settix
{
    public interface IConfigurationRepository
    {
        Task<string> GetAsync(string key);
        Task SetAsync(string key, string value);
        Task DeleteAsync(string key);
        Task<bool> ExistsAsync(string key);
        IEnumerable<DeployedSetting> GetAll(ISettixContext context);
    }
}
