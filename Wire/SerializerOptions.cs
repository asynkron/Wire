namespace Wire
{
    public class SerializerOptions
    {
        public readonly bool VersionTolerance;

        public SerializerOptions(bool versionTolerance = false)
        {
            VersionTolerance = versionTolerance;
        }
    }
}