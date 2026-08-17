using Picterest.DTO.Images;
using Picterest.HelperModels;

namespace Picterest.Services.Interface
{
    public interface IImageService
    {
        Task<ServiceResult> UploadImage(ImageUploadDTO dto,string userId);
        Task<ServiceResult> GetImageFile(string id);
        Task<ServiceResult> GetImageMetaData(string id,string userId);
        Task<ServiceResult> SoftDelete(string id,string userId);
        Task<ServiceResult> GetAllImages(string userId,PaginatedRequest request);
        Task<ServiceResult> UpdateImage(ImageUpdateDTO dto,string userId);
        Task<ServiceResult> GetDeletedImagesDetails();
        Task<ServiceResult> GetRestorableImagesList(string userId);
        Task<ServiceResult> GetRestorableImageFile(string id);
        Task<ServiceResult> RestoreImages(List<string> imageIds,string userId);
    }   
}
