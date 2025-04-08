namespace One.Settix
{
    public interface ISettixFactory
    {
        ISettixContext GetContext();
        IConfigurationRepository GetConfiguration();
    }
}
