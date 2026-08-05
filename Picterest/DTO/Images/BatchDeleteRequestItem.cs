namespace Picterest.DTO.Images
{
    public class BatchDeleteRequestItem
    {
        public Guid FileId { get; set; }
        public string BucketName { get; set; } = string.Empty;
        public string ObjectKey { get; set; } = string.Empty;
    }
}
