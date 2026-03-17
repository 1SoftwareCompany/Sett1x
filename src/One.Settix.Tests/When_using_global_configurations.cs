using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Machine.Specifications;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Primitives;

namespace One.Settix.Tests
{
    public class When_combining_global_with_application_configurations
    {
        Establish context = () =>
        {
            cfgRepo = new GlobalAwareConfigurationRepository(new[]
            {
                new DeployedSetting(new Key(GlobalApp, Cluster, Box.Machine.NotSpecified, "global_cluster"), "global_cluster_value"),
                new DeployedSetting(new Key(GlobalApp, Cluster, Box.Machine.NotSpecified, "global_machine"), "global_machine_cluster_default"),
                new DeployedSetting(new Key(GlobalApp, Cluster, Machine, "global_machine"), "global_machine_value"),
                new DeployedSetting(new Key(App, Cluster, Box.Machine.NotSpecified, "app_cluster"), "app_cluster_value"),
                new DeployedSetting(new Key(App, Cluster, Box.Machine.NotSpecified, "app_machine"), "app_machine_cluster_default"),
                new DeployedSetting(new Key(App, Cluster, Machine, "app_machine"), "app_machine_value")
            });

            appContext = new ApplicationContext(App, Cluster, Machine);
            globalContext = new GlobalContext(GlobalApp, Cluster, Machine);
            settix = new Settix(appContext, cfgRepo);
        };

        Because of = () => allKeys = settix.GetAll(appContext, globalContext).ToList();

        It should_include_all_global_and_application_keys = () => allKeys.Count.ShouldEqual(4);
        It should_include_global_cluster_setting = () => allKeys.Single(x => x.Key.SettingKey == "global_cluster").Value.ShouldEqual("global_cluster_value");
        It should_include_global_machine_setting = () => allKeys.Single(x => x.Key.SettingKey == "global_machine").Value.ShouldEqual("global_machine_value");
        It should_include_application_cluster_setting = () => allKeys.Single(x => x.Key.SettingKey == "app_cluster").Value.ShouldEqual("app_cluster_value");
        It should_include_application_machine_setting = () => allKeys.Single(x => x.Key.SettingKey == "app_machine").Value.ShouldEqual("app_machine_value");

        const string App = "app";
        const string GlobalApp = "global-app";
        const string Cluster = "cluster";
        const string Machine = "m1";

        static IConfigurationRepository cfgRepo;
        static Settix settix;
        static ApplicationContext appContext;
        static GlobalContext globalContext;
        static List<DeployedSetting> allKeys;
    }

    public class When_global_and_application_have_conflicting_keys
    {
        Establish context = () =>
        {
            cfgRepo = new GlobalAwareConfigurationRepository(new[]
            {
                new DeployedSetting(new Key(GlobalApp, Cluster, Box.Machine.NotSpecified, "sharedKey"), "global_value"),
                new DeployedSetting(new Key(App, Cluster, Box.Machine.NotSpecified, "SHAREDKEY"), "app_value")
            });

            appContext = new ApplicationContext(App, Cluster, Machine);
            globalContext = new GlobalContext(GlobalApp, Cluster, Machine);
            settix = new Settix(appContext, cfgRepo);
        };

        Because of = () => exception = Catch.Exception(() => settix.GetAll(appContext, globalContext).ToList());

        It should_throw_invalid_operation_exception = () => exception.ShouldBeOfExactType<InvalidOperationException>();
        It should_report_conflicting_key = () => exception.Message.ShouldContain("sharedKey");

        const string App = "app";
        const string GlobalApp = "global-app";
        const string Cluster = "cluster";
        const string Machine = "m1";

        static IConfigurationRepository cfgRepo;
        static Settix settix;
        static ApplicationContext appContext;
        static GlobalContext globalContext;
        static Exception exception;
    }

    public class When_loading_configuration_provider_with_global_context
    {
        Establish context = () =>
        {
            previousGlobalApp = Environment.GetEnvironmentVariable(EnvVar.GlobalApplicationKey);
            previousCluster = Environment.GetEnvironmentVariable(EnvVar.ClusterKey);
            previousMachine = Environment.GetEnvironmentVariable(EnvVar.MachineKey);

            Environment.SetEnvironmentVariable(EnvVar.GlobalApplicationKey, GlobalApp);
            Environment.SetEnvironmentVariable(EnvVar.ClusterKey, Cluster);
            Environment.SetEnvironmentVariable(EnvVar.MachineKey, Machine);

            cfgRepo = new GlobalAwareConfigurationRepository(new[]
            {
                new DeployedSetting(new Key(GlobalApp, Cluster, Box.Machine.NotSpecified, "global_key"), "global_cluster_default"),
                new DeployedSetting(new Key(GlobalApp, Cluster, Machine, "global_key"), "global_value"),
                new DeployedSetting(new Key(App, Cluster, Box.Machine.NotSpecified, "app_key"), "app_cluster_default"),
                new DeployedSetting(new Key(App, Cluster, Machine, "app_key"), "app_value")
            });

            appContext = new ApplicationContext(App, Cluster, Machine);
            settix = new Settix(appContext, cfgRepo);
            source = new TestSettixConfigurationSource(settix);
            provider = new SettixConfigurationProvider(source);
        };

        Because of = () =>
        {
            provider.Load();
            hasGlobalApplication = provider.TryGet(EnvVar.GlobalApplicationKey, out globalApplicationValue);
            hasGlobalSetting = provider.TryGet("global_key", out globalSettingValue);
            hasApplicationSetting = provider.TryGet("app_key", out appSettingValue);
        };

        It should_include_global_application_metadata = () => hasGlobalApplication.ShouldBeTrue();
        It should_set_global_application_metadata = () => globalApplicationValue.ShouldEqual(GlobalApp);
        It should_include_global_setting = () => hasGlobalSetting.ShouldBeTrue();
        It should_include_application_setting = () => hasApplicationSetting.ShouldBeTrue();
        It should_preserve_global_setting_value = () => globalSettingValue.ShouldEqual("global_value");
        It should_preserve_application_setting_value = () => appSettingValue.ShouldEqual("app_value");

        Cleanup after = () =>
        {
            Environment.SetEnvironmentVariable(EnvVar.GlobalApplicationKey, previousGlobalApp);
            Environment.SetEnvironmentVariable(EnvVar.ClusterKey, previousCluster);
            Environment.SetEnvironmentVariable(EnvVar.MachineKey, previousMachine);
        };

        const string App = "app";
        const string GlobalApp = "global-app";
        const string Cluster = "cluster";
        const string Machine = "m1";

        static IConfigurationRepository cfgRepo;
        static Settix settix;
        static ApplicationContext appContext;
        static SettixConfigurationProvider provider;
        static TestSettixConfigurationSource source;

        static string previousGlobalApp;
        static string previousCluster;
        static string previousMachine;
        static bool hasGlobalApplication;
        static bool hasGlobalSetting;
        static bool hasApplicationSetting;
        static string globalApplicationValue;
        static string globalSettingValue;
        static string appSettingValue;
    }

    class GlobalAwareConfigurationRepository : IConfigurationRepository
    {
        readonly List<DeployedSetting> keys;

        public GlobalAwareConfigurationRepository(IEnumerable<DeployedSetting> keys)
        {
            this.keys = keys.ToList();
        }

        public Task DeleteAsync(string key) => throw new NotImplementedException();
        public Task<bool> ExistsAsync(string key) => throw new NotImplementedException();
        public Task<string> GetAsync(string key) => throw new NotImplementedException();
        public Task SetAsync(string key, string value) => throw new NotImplementedException();
        public IEnumerable<DeployedSetting> GetAll(ISettixContext context) => keys;
    }

    class TestSettixConfigurationSource : SettixConfigurationSource
    {
        public TestSettixConfigurationSource(Settix settix)
        {
            Settix = settix;
            ReloadOnChange = false;
            ReloadWatcher = new NoopSettixWatcher();
        }

        public override ISettixWatcher ReloadWatcher { get; set; }

        public override IConfigurationProvider Build(IConfigurationBuilder builder)
        {
            return new SettixConfigurationProvider(this);
        }
    }

    class NoopSettixWatcher : ISettixWatcher
    {
        public void Dispose() { }

        public IChangeToken Watch()
        {
            return new CancellationChangeToken(new CancellationToken(canceled: true));
        }
    }
}
