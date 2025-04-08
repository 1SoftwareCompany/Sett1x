﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Machine.Specifications;

namespace One.Settix.Tests
{
    public class When_using_json_array_as_a_setting_value
    {
        Establish context = () =>
        {
            IConfigurationRepository cfgRepo = new ArrayTestConfigurationRepository();
            appContext = new ApplicationContext("app", "cluster", "m1");
            settix = new Settix(appContext, cfgRepo);
        };

        Because of = () => allKeys = settix.GetAll(appContext).ToList();

        It should_have_all_keys = () => allKeys.Count.ShouldEqual(2);

        It should_have_propper_key1 = () => allKeys.Where(x => x.Key.SettingKey == "key1:0").Single().Value.ShouldEqual("value0");
        It should_have_propper_key2 = () => allKeys.Where(x => x.Key.SettingKey == "key1:1").Single().Value.ShouldEqual("value1");

        static Settix settix;
        static ApplicationContext appContext;
        static List<DeployedSetting> allKeys;

        class ArrayTestConfigurationRepository : IConfigurationRepository
        {
            List<DeployedSetting> keys = new List<DeployedSetting>();

            const string App = "app";
            const string Cluster = "cluster";
            const string Machine = "m1";

            public ArrayTestConfigurationRepository()
            {
                keys.Add(new DeployedSetting(new Key(App, Cluster, Box.Machine.NotSpecified, "key1"), "[\"value0\", \"value1\", \"value2\"]"));
                keys.Add(new DeployedSetting(new Key(App, Cluster, Machine, "key1"), "[\"value0\", \"value1\"]"));
            }

            public Task DeleteAsync(string key) { throw new NotImplementedException(); }

            public Task<bool> ExistsAsync(string key) { throw new NotImplementedException(); }

            public Task<string> GetAsync(string key) { throw new NotImplementedException(); }

            public IEnumerable<DeployedSetting> GetAll(ISettixContext context)
            {
                return keys;
            }

            public Task SetAsync(string key, string value) { throw new NotImplementedException(); }
        }
    }
}
