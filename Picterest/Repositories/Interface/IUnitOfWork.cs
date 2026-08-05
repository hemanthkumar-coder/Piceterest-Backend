using Microsoft.EntityFrameworkCore.Storage;

namespace Picterest.Repositories.Interface
{
    public interface IUnitOfWork: IDisposable
    {
        IImageRepository Images { get; }
        IFileRepository Files { get; }
        IUserRepository Users { get; }
        Task<int> SaveChangesAsync();
        int SaveChanges();
        void EnableDetectChanges();
        void DisableDetectChanges();

        Task<IDbContextTransaction> BeginTransactionAsync(CancellationToken cancellationToken = default);
        Task CommitTransactionAsync(CancellationToken cancellationToken = default);
        Task RevertTransactionAsync(CancellationToken cancellationToken = default);
    }
}
