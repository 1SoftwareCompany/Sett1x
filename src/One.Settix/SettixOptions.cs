using System;

namespace One.Settix
{
    public class SettixOptions
    {
        public SettixOptions() { }

        public SettixOptions(string clusterName, string machineName)
        {
            ClusterName = clusterName;
            MachineName = machineName;
        }

        public string ClusterName { get; set; }

        public string MachineName { get; set; }

        public static SettixOptions Defaults = new SettixOptions() { ClusterName = "local", MachineName = Environment.MachineName };
    }
}
