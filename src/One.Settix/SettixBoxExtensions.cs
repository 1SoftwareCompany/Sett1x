namespace One.Settix
{
    public static class SettixBoxExtensions
    {
        public static One.Settix.Box.Configuration Open(this One.Settix.Box.Box box, SettixOptions options)
        {
            var opener = new SettixBoxOpener(box);
            return opener.Open(options);
        }
    }
}
