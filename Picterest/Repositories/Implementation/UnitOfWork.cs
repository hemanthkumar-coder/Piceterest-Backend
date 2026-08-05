using Microsoft.EntityFrameworkCore.Storage;
using Picterest.Context;
using Picterest.Repositories.Interface;

namespace Picterest.Repositories.Implementation
{
    public class UnitOfWork : IUnitOfWork
    {
        private IDbContextTransaction? _transaction;

        private IImageRepository? _imageRepository;
        private IFileRepository? _fileRepository;
        private IUserRepository? _userRepository;

        private readonly ImageDbContext _context;
        public IImageRepository Images => _imageRepository ??= new ImageRepository(_context);
        public IFileRepository Files => _fileRepository ??= new FileRepository(_context);

        public IUserRepository Users => _userRepository ??= new UserRepository(_context);

        public UnitOfWork(ImageDbContext context)
        {
            _context = context;
        }

        public void DisableDetectChanges()
        {
            _context.ChangeTracker.AutoDetectChangesEnabled = false;
        }

        public void EnableDetectChanges()
        {
            _context.ChangeTracker.AutoDetectChangesEnabled = true;
        }
        

        public int SaveChanges()
        {
            return _context.SaveChanges();
        }

        public async Task<int> SaveChangesAsync()
        {
            return await _context.SaveChangesAsync();
        }

        public async Task<IDbContextTransaction> BeginTransactionAsync(CancellationToken cancellationToken = default)
        {
            if(_transaction != null)
            {
                return _transaction;
            }

            _transaction = await _context.Database.BeginTransactionAsync(cancellationToken);

            return _transaction;
        }

        public async Task CommitTransactionAsync(CancellationToken cancellationToken = default)
        {
            if(_transaction == null)
            {
                throw new InvalidOperationException("No transaction in progress to commit.");
            }

            try
            {
                await _transaction.CommitAsync(cancellationToken);
            }
            finally
            {
                await _transaction.DisposeAsync();
                _transaction = null;
            }
        }

        public async Task RevertTransactionAsync(CancellationToken cancellationToken = default)
        {
            if(_transaction == null)
            {
                throw new InvalidOperationException("No transaction in progress to revert.");
            }

            try
            {
                await _transaction.RollbackAsync(cancellationToken);
            }
            finally
            {
                await _transaction.DisposeAsync();
                _transaction = null;
            }
        }

        public void Dispose()
        {
            _transaction?.Dispose();
            _context.Dispose();
        }
    }
}
