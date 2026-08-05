using Amazon.S3.Model;

namespace Picterest.DTO.FileStorage
{
    public class DeletedObjectsResponse
    {
        public List<DeletedObject> DeletedObjects { get; set; } = new List<DeletedObject>();
        public List<DeleteError> DeleteErrors { get; set; } = new List<DeleteError>();
    }
}
