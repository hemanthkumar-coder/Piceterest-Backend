using Picterest.DbModels;
using Picterest.DTO.Images;
using Picterest.HelperModels;

namespace Picterest.Repositories.Interface
{
    public interface IImageRepository: IGenericRepository<Image>
    {
        Task<Image?> GetImageByName(string name);
        Task<IEnumerable<Image>> GetImagesBySearchString(string searchString);
        Task<List<Image>> GetAllImages(Guid userId,PaginatedRequest request);
        IEnumerable<Image>? GetDeletedImagesDetails();
        IEnumerable<FileIdAndObjectKey> GetSoftDeletedImageFileDataList();
        IEnumerable<Image>? GetRestorableImagesList(Guid userId);
        Image? GetRestorableImage(Guid id);
        Task RestoreImages(IEnumerable<Guid> imageIds,Guid userId);
        Task<Image?> GetImageFileByImageIdAndUserId(Guid id, Guid userId);
        Task<int> GetAllImagesCount(Guid userId);

    }
}
