using System.Collections.Generic;

namespace One.Settix.Box
{
    public sealed class Cluster : Configuration
    {
        public Cluster(string name, Dictionary<string, object> settings) : base(name, settings) { }
        public Cluster(Configuration configuration) : base(configuration.Name, configuration.AsDictionary()) { }
    }
}
