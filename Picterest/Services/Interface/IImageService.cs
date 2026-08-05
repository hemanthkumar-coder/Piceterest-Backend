using Picterest.DTO.Images;
using Picterest.HelperModels;

namespace Picterest.Services.Interface
{
    public interface IImageService
    {
        Task<ServiceResult> UploadImage(ImageUploadDTO dto);
        Task<ServiceResult> GetImageFile(string id);
        Task<ServiceResult> GetImageMetaData(string id);
        Task<ServiceResult> SoftDelete(string id);
        Task<ServiceResult> GetAllImages();
        Task<ServiceResult> UpdateImage(ImageUpdateDTO dto);
        Task<ServiceResult> GetDeletedImagesDetails();
        Task<ServiceResult> GetRestorableImagesList();
        Task<ServiceResult> GetRestorableImageFile(string id);
        Task<ServiceResult> RestoreImages(List<string> imageIds);
    }   
}
