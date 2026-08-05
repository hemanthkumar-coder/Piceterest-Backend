namespace Picterest.DTO.Images
{
    public class ImageStorageDeleteRequestItem
    {
        public string BucketName { get; set; } = string.Empty;
        public List<FileIdAndObjectKey>? fileIdAndObjectKeys { get; set; }
    }

    public class FileIdAndObjectKey
    {
        public Guid FileId { get; set; }
        public string ObjectKey { get; set; } = string.Empty;

    }
}
