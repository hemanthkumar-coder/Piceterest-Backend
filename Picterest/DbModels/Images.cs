namespace Picterest.DbModels
{
    public class Image
    {
        public Guid Id { get; set; }
        public required string Name { get; set; }
        public string? Description { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime UpdatedAt { get; set; }
        public DateTime? RestoredAt { get; set; } = null;
        public bool IsDeleted { get; set; } = false;
        public DateTime DeletedAt { get; set; }
        public Guid FileId {  get; set; }
        public File Files { get; set; } = null!;
    }

    
}
