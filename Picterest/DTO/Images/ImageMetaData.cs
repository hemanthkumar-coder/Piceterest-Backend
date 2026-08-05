namespace Picterest.DTO.Images
{
    public class ImageMetaData
    {
        public Guid ImageId { get; set; } = Guid.Empty;
        public string ImageName { get; set; } = string.Empty;
        public string ImageDescription { get; set; } = string.Empty;
        public string ImageSize { get; set; } = string.Empty;
        public string? ImageUrl { get; set; }
        public DateTime UploadedAt { get; set; }

    }
}
