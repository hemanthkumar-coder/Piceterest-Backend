using Picterest.DTO.FileStorage;
using Picterest.HelperModels;

namespace Picterest.Services.Interface
{
    public interface IFileService
    {
        Task<ServiceResult> CreateFile(Picterest.DbModels.File file);
        Task<ServiceResult> ChangeStatusOfSuccessCleanUpFiles(List<string> keys);
        Task<ServiceResult> ChangeStatusOfFailureCleanUpFiles(List<FailureKeyObject> obj);
        Task<ServiceResult> DeleteStorageCleanedFilesFromDb();
    }
}
