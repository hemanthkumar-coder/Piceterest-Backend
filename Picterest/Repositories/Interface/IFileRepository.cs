namespace Picterest.Repositories.Interface
{
    public interface IFileRepository: IGenericRepository<DbModels.File>
    {
        DbModels.File? GetFileByObjectKey(string objectKey);
        IEnumerable<DbModels.File> GetCleanUpPendingFiles(List<string> keys);
        IEnumerable<Guid> GetFileIdsWhichAreCleanedFromStorage();
        Task DeleteFilesWithIdsAsync(IEnumerable<Guid> fileIds);
        
    }
}
