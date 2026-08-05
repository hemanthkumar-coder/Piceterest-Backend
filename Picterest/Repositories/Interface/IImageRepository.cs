using Picterest.DbModels;
using Picterest.DTO.Images;

namespace Picterest.Repositories.Interface
{
    public interface IImageRepository: IGenericRepository<Image>
    {
        Task<Image?> GetImageByName(string name);
        Task<IEnumerable<Image>> GetImagesBySearchString(string searchString);
        IEnumerable<Image>? GetAllImages();
        IEnumerable<Image>? GetDeletedImagesDetails();
        IEnumerable<FileIdAndObjectKey> GetSoftDeletedImageFileDataList();
        IEnumerable<Image>? GetRestorableImagesList();
        Image? GetRestorableImage(Guid id);
        Task RestoreImages(IEnumerable<Guid> imageIds);

    }
}
