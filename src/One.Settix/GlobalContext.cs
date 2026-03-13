using System;
using System.Collections.Generic;
using System.Text;

namespace One.Settix
{
    public class GlobalContext : ISettixContext
    {
        public GlobalContext(string applicationName, string cluster = null, string machine = null)
        {
            ApplicationName = applicationName;
            Cluster = cluster ?? EnvVar.GetCluster();
            Machine = machine ?? EnvVar.GetMachine() ?? Box.Machine.NotSpecified;
        }

        public string ApplicationName { get; private set; }
        public string Cluster { get; private set; }
        public string Machine { get; private set; }

        public override string ToString()
        {
            return $"Settix global context: Cluster: {Cluster} | Machine: {Machine} | Application: {ApplicationName}";
        }
    }
}
