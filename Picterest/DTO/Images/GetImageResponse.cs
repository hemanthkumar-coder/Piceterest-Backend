namespace Picterest.DTO.Images
{
    public class GetImageResponse
    {
        public Stream File { get; set; } = null!;
        public string ImageName { get; set; } = string.Empty;
        public string ContentType { get; set; } = string.Empty;
        public Guid ImageId { get; set; } = Guid.Empty;
        public long FileSize { get; set; } = 0;

    }
}
