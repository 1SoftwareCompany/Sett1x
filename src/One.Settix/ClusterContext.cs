namespace One.Settix
{
    public class ClusterContext : ISettixContext
    {
        public ClusterContext(string applicationName = null, string cluster = null)
        {
            this.ApplicationName = applicationName ?? EnvVar.GetApplication();
            this.Cluster = cluster ?? EnvVar.GetCluster();
            this.Machine = Box.Machine.NotSpecified;
        }

        public string ApplicationName { get; private set; }

        public string Cluster { get; private set; }

        public string Machine { get; private set; }

        public override string ToString()
        {
            return $"Settix context: Cluster: {Cluster} | Machine: {Machine} | Application: {ApplicationName}";
        }
    }
}
