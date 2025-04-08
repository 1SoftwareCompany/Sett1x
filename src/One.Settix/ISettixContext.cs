namespace One.Settix
{
    public interface ISettixContext
    {
        string ApplicationName { get; }

        string Cluster { get; }

        string Machine { get; }
    }
}
