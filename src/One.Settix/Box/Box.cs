using System;
using System.Collections.Generic;
using System.Linq;

namespace One.Settix.Box
{
    public class Box
    {
        private readonly List<string> reservedKeys;

        public Box(Jar jar)
        {
            Name = jar.Name;
            References = jar.References;
            Clusters = new List<Cluster>();
            Machines = new List<Machine>();
            Defaults = new Configuration(jar.Defaults);
            Dynamics = jar.Dynamics;
            reservedKeys = new List<string>() { Machine.ClusterKey };
        }

        public Box(Box box)
        {
            Name = box.Name;
            References = new List<Dictionary<string, string>>(box.References.Select(x => new Dictionary<string, string>(x)));
            Clusters = new List<Cluster>(box.Clusters);
            Machines = new List<Machine>(box.Machines);
            Defaults = new Configuration(box.Defaults.AsDictionary());
            Dynamics = new List<string>(box.Dynamics);
            reservedKeys = new List<string>(box.reservedKeys);
        }

        public string Name { get; private set; }

        public Configuration Defaults { get; set; }

        /// <summary>
        /// These settings are dynamic and can be changed at runtime. If a setting exists on the running environment it will NOT be updated. Otherwise it will use the values stored in the box.
        /// </summary>
        /// <remarks>
        /// It is a responsibility of the deployment process to add or NOT add the dynamic settings to the running environment.
        /// </remarks>
        public List<string> Dynamics { get; set; }

        public List<Cluster> Clusters { get; set; }

        public List<Dictionary<string, string>> References { get; set; }

        public void Merge(Box box)
        {
            Defaults = Defaults.Join(box.Defaults);
            Dynamics.AddRange(box.Dynamics);
            Clusters = Clusters.Merge(box.Clusters).ToList();
            Machines = Machines.Merge(box.Machines).ToList();
        }

        public void AddCluster(string name, Dictionary<string, object> settings)
        {
            var cluster = new Cluster(name, settings);
            AddCluster(cluster);
        }

        public void AddCluster(Cluster cluster)
        {
            Guard_SettingMustBeDefinedInDefaults(cluster.AsDictionary());

            if (!Clusters.Contains(cluster))
                Clusters.Add(cluster);
        }

        public List<Machine> Machines { get; set; }

        public void AddMachine(string name, Dictionary<string, object> settings)
        {
            var machine = new Machine(name, settings);
            AddMachine(machine);
        }

        public void AddMachine(Machine machine)
        {
            Guard_SettingMustBeDefinedInDefaults(machine.AsDictionary());
            Guard_MachineClusterConfiguration(machine);

            if (!Machines.Contains(machine))
                Machines.Add(machine);
        }

        private void Guard_SettingMustBeDefinedInDefaults(Dictionary<string, object> settings)
        {
            foreach (var setting in settings)
            {
                Guard_SettingMustBeDefinedInDefaults(setting.Key);
            }
        }

        private void Guard_MachineClusterConfiguration(Machine machine)
        {
            if (machine.ContainsKey(Machine.ClusterKey))
            {
                var clusterName = machine[Machine.ClusterKey];
                var isValid = Clusters.Any(x => x.Name.Equals(clusterName.ToString(), StringComparison.OrdinalIgnoreCase));
                if (isValid == false)
                    throw new ArgumentException(string.Format("Invalid machine configuration. The machine '{0}' is explicitly configured in cluster '{1}' but cluster configuration with that name does not exist.", machine.Name, clusterName));
            }
        }

        private void Guard_SettingMustBeDefinedInDefaults(string settingKey)
        {
            if (reservedKeys.Contains(settingKey))
                return;

            if (!Defaults.ContainsKey(settingKey))
                throw new ArgumentException(String.Format("The setting key '{0}' was not found in the Default settings for application '{1}'. You can override only settings inside the default settings", settingKey, Name));
        }

        public static Box Mistranslate(Jar jar)
        {
            Box box = new Box(jar);

            if (jar.Clusters != null)
            {
                foreach (var cluster in jar.Clusters)
                {
                    box.AddCluster(cluster.Key, cluster.Value);
                }
            }

            if (jar.Machines != null)
            {
                foreach (var machine in jar.Machines)
                {
                    box.AddMachine(machine.Key, machine.Value);
                }
            }

            return box;
        }

        public static Jar Mistranslate(Box box)
        {
            Jar jar = new Jar();

            jar.Name = box.Name;
            jar.References = box.References;

            if (box.Defaults != null)
                jar.Defaults = box.Defaults.AsDictionary();

            if (box.Dynamics != null)
                jar.Dynamics = box.Dynamics;

            if (box.Clusters != null)
            {
                foreach (var cluster in box.Clusters)
                {
                    jar.Clusters.Add(cluster.Name, cluster.AsDictionary());
                }
            }

            if (box.Machines != null)
            {
                foreach (var machine in box.Machines)
                {
                    jar.Machines.Add(machine.Name, machine.AsDictionary());
                }
            }

            return jar;
        }
    }
}
