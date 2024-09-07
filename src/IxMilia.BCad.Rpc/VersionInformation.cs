namespace IxMilia.BCad.Rpc
{
    public class VersionInformation
    {
        public string AboutString { get; }

        public string CurrentVersion { get; }

        public string AvailableVersion { get; }

        public VersionInformation(string aboutString, string currentVersion, string availableVersion)
        {
            AboutString = aboutString;
            CurrentVersion = currentVersion;
            AvailableVersion = availableVersion;
        }
    }
}
