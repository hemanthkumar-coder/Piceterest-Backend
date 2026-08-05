using Amazon.S3.Model;
using Picterest.HelperModels;

namespace Picterest.Services.Interface
{
    public interface IFileStorageService
    {
        Task<ServiceResult> UploadAsync(
        IFormFile file,
        string FileName,
        CancellationToken cancellationToken = default);

        Task<ServiceResult> DeleteAsync(
            string objectKey,
            string BucketName,
            CancellationToken cancellationToken = default);

        Task CreateBucketIfNotExistsAsync(string BucketName, CancellationToken cancellationToken = default);
        Task<ServiceResult> GetObjectAsync(string BucketName, string objectKey, CancellationToken cancellationToken = default);
        Task<ServiceResult> UpdateObjectAsync(string BucketName,string ObjectKey,IFormFile File,CancellationToken cancellationToken = default);
        Task<ServiceResult> DeleteObjectsAsync(DeleteObjectsRequest deleteObjectsRequest, CancellationToken cancellationToken = default);
        string GetImageUrl(string bucket, string objectKey);
    }
}
