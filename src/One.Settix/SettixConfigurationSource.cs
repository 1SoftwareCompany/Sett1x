using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Primitives;
using System;

namespace One.Settix
{
    public abstract class SettixConfigurationSource : ISettixConfigurationSource
    {
        public Settix Settix { get; set; }
        public bool ReloadOnChange { get; set; } = true;
        public TimeSpan ReloadDelay { get; set; } = TimeSpan.FromMinutes(1);
        public IChangeToken ChangeToken { get; set; }
        public Action<SettixConfigurationProvider> ChangeTokenConsumer { get; set; }
        public abstract ISettixWatcher ReloadWatcher { get; set; }

        public abstract IConfigurationProvider Build(IConfigurationBuilder builder);
    }
}
