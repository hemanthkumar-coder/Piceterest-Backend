using Picterest.Enums;

namespace Picterest.DbModels
{
    public class File
    {
        public Guid Id { get; set; }
        public string FileName { get; set; } = string.Empty;
        public string ObjectKey { get; set; } = string.Empty;
        public string Bucket { get; set; } = string.Empty;
        public string ContentType { get; set; } = string.Empty;
        public long Size { get; set; }
        public CleanupStatus cleanupStatus { get; set; } = CleanupStatus.Active;
        public DateTime? StorageDeletedAt { get; set; }
        public int DeleteAttempts { get; set; }
        public string? LastDeleteError { get; set; }
        public DateTime UploadedAt { get; set; } = DateTime.UtcNow;
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
        public Image Image { get; set; } = null!;

    }
}
