using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Primitives;
using System;
using System.Collections.Generic;
using System.Linq;

namespace One.Settix
{
    public class SettixConfigurationProvider : ConfigurationProvider
    {
        private readonly Settix settix;

        public SettixConfigurationProvider(ISettixConfigurationSource source)
        {
            this.settix = source.Settix;

            if (source.ReloadOnChange)
            {
                ChangeToken.OnChange(() => source.ReloadWatcher.Watch(), Load);
            }
        }

        public override void Load()
        {
            List<DeployedSetting> newState = settix.GetAll(settix.ApplicationContext).ToList();

            Data = newState.ToDictionary(key => key.Key.SettingKey, value => value.Value, StringComparer.OrdinalIgnoreCase);
            Data.Add(EnvVar.ApplicationKey, settix.ApplicationContext.ApplicationName);
            Data.Add(EnvVar.MachineKey, settix.ApplicationContext.Machine);
            Data.Add(EnvVar.ClusterKey, settix.ApplicationContext.Cluster);

            OnReload();  // Notifies the change to the ConfigurationProvider. This will trigger the reload of the ConfigurationProvider.
        }
    }
}
