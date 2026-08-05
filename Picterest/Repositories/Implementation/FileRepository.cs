using Microsoft.EntityFrameworkCore;
using Picterest.Context;
using Picterest.Enums;
using Picterest.Repositories.Interface;

namespace Picterest.Repositories.Implementation
{
    public class FileRepository : GenericRepository<DbModels.File, ImageDbContext>, IFileRepository
    {
        public FileRepository(ImageDbContext context) : base(context)
        {
        }

        public async Task DeleteFilesWithIdsAsync(IEnumerable<Guid> fileIds)
        {
            await _context.Files.Where(f => fileIds.Contains(f.Id)).ExecuteDeleteAsync();
        }

        public IEnumerable<DbModels.File> GetCleanUpPendingFiles(List<string> keys)
        {
            return _context.Files.Where(f => keys.Contains(f.ObjectKey) && f.cleanupStatus == CleanupStatus.PendingStorageDeletion).ToList();
        }

        public DbModels.File? GetFileByObjectKey(string objectKey)
        {
            return _context.Files.FirstOrDefault(f => f.ObjectKey == objectKey);
        }

        public IEnumerable<Guid> GetFileIdsWhichAreCleanedFromStorage()
        {
            return _context.Files.Where(f => f.cleanupStatus == CleanupStatus.StorageDeleted).Select(f => f.Id).ToList();
        }
    }
}
