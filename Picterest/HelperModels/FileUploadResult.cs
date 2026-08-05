namespace Picterest.HelperModels
{
    public class FileUploadResult
    {
        public string ObjectKey { get; set; } = string.Empty;

        public string Bucket { get; set; } = string.Empty;

        public string ETag { get; set; } = string.Empty;
    }
}
