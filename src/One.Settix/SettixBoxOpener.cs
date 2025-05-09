﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
//using System.Text.Json.Serialization;
//using System.Text.Json; // Check if deserialization of dictionaries is fixed => https://github.com/dotnet/runtime/issues/30524
// December 2021 => These from Microsoft are soooo...
using One.Settix.Box;
using Newtonsoft.Json;

namespace One.Settix
{
    public class SettixBoxOpener
    {
        // Check if deserialization of dictionaries is fixed => https://github.com/dotnet/runtime/issues/30524
        //static JsonSerializerOptions jsonSerializerOptions = new JsonSerializerOptions()
        //{
        //    PropertyNameCaseInsensitive = true,
        //    ReadCommentHandling = JsonCommentHandling.Skip,
        //    AllowTrailingCommas = true
        //};

        One.Settix.Box.Box box;

        public SettixBoxOpener(Box.Box box)
        {
            this.box = new Box.Box(box);
        }

        public Configuration Open(SettixOptions options)
        {
            options = options ?? SettixOptions.Defaults;

            foreach (var reference in box.References)
            {
                var refJarFile = reference.Values.First();
                var referenceJar = JsonConvert.DeserializeObject<Jar>(File.ReadAllText(refJarFile));
                var referenceBox = Box.Box.Mistranslate(referenceJar);

                box.Merge(referenceBox);
            }

            if (string.IsNullOrEmpty(options.ClusterName) && string.IsNullOrEmpty(options.MachineName))
                throw new ArgumentNullException("clusterName", "When getting configuraion for a machine the clusterName is required");

            Dictionary<string, object> confDefaults = box.Defaults.AsDictionary();
            Dictionary<string, object> confCluster = GetClusterConfiguration(options.ClusterName);
            Dictionary<string, object> confMachine = GetMachineConfiguration(options.MachineName);
            List<string> dynamics = box.Dynamics;

            Dictionary<string, object> namanizedDefaltConfigs = NamenizeConfiguration(confDefaults, options.ClusterName, Machine.NotSpecified);
            Dictionary<string, object> namanizedClusterConfigs = NamenizeConfiguration(confCluster, options.ClusterName, Machine.NotSpecified);
            Dictionary<string, object> namanizedMachineConfigs = NamenizeConfiguration(confMachine, options.ClusterName, options.MachineName);
            List<string> namanizedDynamics = NamenizeConfiguration(dynamics, options.ClusterName, Machine.NotSpecified);

            return new Configuration(box.Name, Merge(namanizedDefaltConfigs, Merge(namanizedMachineConfigs, namanizedClusterConfigs)), namanizedDynamics);
        }

        Dictionary<string, object> GetMachineConfiguration(string machineName)
        {
            Machine machine = null;
            Dictionary<string, object> confMachine = new Dictionary<string, object>();
            if (TryFindMachine(machineName, out machine))
                confMachine = machine.AsDictionary();

            return confMachine;
        }

        Dictionary<string, object> GetClusterConfiguration(string clusterName)
        {
            Cluster cluster = null;
            Dictionary<string, object> confCluster = new Dictionary<string, object>();
            if (TryFindCluster(clusterName, out cluster))
                confCluster = cluster.AsDictionary();

            return confCluster;
        }

        Dictionary<string, object> NamenizeConfiguration(Dictionary<string, object> settings, string clusterName, string machineName)
        {
            return settings.ToDictionary(x => NameBuilder.GetSettingName(box.Name, clusterName, machineName, x.Key), y => y.Value);
        }

        List<string> NamenizeConfiguration(List<string> dynamicSettings, string clusterName, string machineName)
        {
            return dynamicSettings.Select(x => NameBuilder.GetSettingName(box.Name, clusterName, machineName, x)).ToList();
        }

        bool TryFindCluster(string clusterName, out Cluster cluster)
        {
            cluster = box.Clusters.Where(x => x.Name.Equals(clusterName, StringComparison.OrdinalIgnoreCase)).SingleOrDefault();
            return cluster != null;
        }

        bool TryFindMachine(string machineName, out Machine machine)
        {
            machine = box.Machines.Where(x => x.Name.Equals(machineName, StringComparison.OrdinalIgnoreCase)).SingleOrDefault();
            return machine != null;
        }

        Dictionary<T1, T2> Merge<T1, T2>(Dictionary<T1, T2> first, Dictionary<T1, T2> second)
        {
            if (first == null) throw new ArgumentNullException(nameof(first));
            if (second == null) throw new ArgumentNullException(nameof(second));

            var merged = new Dictionary<T1, T2>();
            first.ToList().ForEach(kv => merged[kv.Key] = kv.Value);
            second.ToList().ForEach(kv => merged[kv.Key] = kv.Value);

            return merged;
        }
    }
}
