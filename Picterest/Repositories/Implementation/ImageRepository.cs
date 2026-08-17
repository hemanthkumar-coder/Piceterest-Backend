using Microsoft.EntityFrameworkCore;
using Picterest.Context;
using Picterest.DbModels;
using Picterest.DTO.Images;
using Picterest.HelperModels;
using Picterest.Repositories.Interface;

namespace Picterest.Repositories.Implementation
{
    public class ImageRepository : GenericRepository<Image, ImageDbContext>, IImageRepository
    {
        public ImageRepository(ImageDbContext context) : base(context)
        {
        }
        public async Task<Image?> GetImageByName(string name)
        {
            return await _context.Images.FirstOrDefaultAsync(i => i.Name == name);
        }

        public async Task<IEnumerable<Image>> GetImagesBySearchString(string searchString)
        {
            return await _context.Images
                .Where(i => i.Name.Contains(searchString) || (i.Description != null && i.Description.Contains(searchString)))
                .ToListAsync();
        }

        public override async Task<Image?> GetByPkAsync(Guid id)
        {
            return await _context.Images.Include(i=>i.Files).FirstOrDefaultAsync(i => i.Id == id && !i.IsDeleted);
        }

        public async Task<List<Image>> GetAllImages(
    Guid userId,
    PaginatedRequest request)
        {
            var query = _context.Images
                .Where(i =>
                    !i.IsDeleted &&
                    i.UserId == userId)
                .Include(i => i.Files)
                .AsNoTracking();

            if (!string.IsNullOrWhiteSpace(request.search))
            {
                var search = request.search.Trim();

                query = query.Where(i =>
                    EF.Functions.ILike(
                        i.Name,
                        $"%{search}%")
                    ||
                    (i.Description != null &&
                     EF.Functions.ILike(
                         i.Description,
                         $"%{search}%")));
            }

            query = query
                .OrderByDescending(i => i.CreatedAt)
                .ThenByDescending(i => i.Id);

            return await query
                .Skip((request.PageNumber - 1) * request.PageSize)
                .Take(request.PageSize)
                .ToListAsync();
        }

        public IEnumerable<Image>? GetDeletedImagesDetails()
        {
            return _context.Images.Where(i => i.IsDeleted).Include(i=>i.Files).AsNoTracking().ToList();
        }

        public IEnumerable<FileIdAndObjectKey> GetSoftDeletedImageFileDataList()
        {
            return _context.Images.Where(i =>
                    i.IsDeleted &&
                    i.Files.cleanupStatus == Enums.CleanupStatus.PendingStorageDeletion
                    ).Select(item =>
                        new FileIdAndObjectKey
                        {
                            FileId = item.FileId,
                            ObjectKey = item.Files.ObjectKey
                        }
                    ).ToList();
        }

        public IEnumerable<Image>? GetRestorableImagesList(Guid userId)
        {
            return _context.Images.Where(i => i.IsDeleted && i.Files.cleanupStatus == Enums.CleanupStatus.PendingStorageDeletion && i.UserId == userId).Include(i => i.Files).AsNoTracking().ToList() ?? new List<Image>();
        }

        public Image? GetRestorableImage(Guid id)
        {
            return _context.Images.Include(i=>i.Files).FirstOrDefault(i => i.Id == id && i.IsDeleted && i.Files.cleanupStatus == Enums.CleanupStatus.PendingStorageDeletion);
        }

        public async Task RestoreImages(IEnumerable<Guid> imageIds,Guid userId)
        {
            var restorableImages = await _context.Images.Include(i => i.Files)
                .Where(i => imageIds.Contains(i.Id) && i.IsDeleted && i.Files.cleanupStatus == Enums.CleanupStatus.PendingStorageDeletion && i.UserId == userId)
                .ToListAsync();

            foreach (var image in restorableImages)
            {
                image.IsDeleted = false;
                image.Files.cleanupStatus = Enums.CleanupStatus.Active;
                image.RestoredAt = DateTime.UtcNow;
            }

            await _context.SaveChangesAsync();

        }

        public Task<Image?> GetImageFileByImageIdAndUserId(Guid id, Guid userId)
        {
            return _context.Images.Include(i => i.Files).FirstOrDefaultAsync(i => i.Id == id && !i.IsDeleted && i.UserId == userId);
        }

        public async Task<int> GetAllImagesCount(Guid userId)
        {
            var query = _context.Images.Where(i => !i.IsDeleted && i.UserId == userId);
            return await query.CountAsync();
        }
    }
}
