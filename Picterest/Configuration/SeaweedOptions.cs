namespace Picterest.Configuration
{
    public class SeaweedOptions
    {
        public const string SectionName = "SeaweedFS";

        public string ServiceUrl { get; set; } = string.Empty;

        public string AccessKey { get; set; } = string.Empty;

        public string SecretKey { get; set; } = string.Empty;
    }
}
