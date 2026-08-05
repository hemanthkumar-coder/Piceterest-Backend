using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Picterest.DTO.FileStorage;
using Picterest.HelperModels;
using Picterest.Repositories.Interface;
using Picterest.Services.Interface;

namespace Picterest.Services.Implementation
{
    public class FileService : IFileService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ILogger<FileService> _logger;
        public FileService(IUnitOfWork unitOfWork,ILogger<FileService> logger)
        {
            _unitOfWork = unitOfWork;
            _logger = logger;
        }
        public async Task<ServiceResult> CreateFile(DbModels.File file)
        {
            try
            {
                if (file == null)
                {
                    return new ServiceResult
                    {
                        IsSuccess = false,
                        Error = "File cannot be null",
                        Result = null
                    };
                }

                await _unitOfWork.Files.AddAsync(file);
                await _unitOfWork.SaveChangesAsync();
                return new ServiceResult
                {
                    IsSuccess = true,
                    Error = null,
                    Result = file
                };
            }
            catch (Exception ex)
            {

                return new ServiceResult
                {
                    IsSuccess = false,
                    Error = $"An error occurred while creating the file: {ex.Message}",
                    Result = null
                };
            }
        }

        public async Task<ServiceResult> ChangeStatusOfSuccessCleanUpFiles(List<string> keys)
        {
            if(keys == null || keys.Count() == 0)
            {
                return new ServiceResult
                {
                    IsSuccess = false,
                    Error = "No Keys Found",
                    StatusCode = 400
                };
            }

            try
            {
                var cleanUpPendingFiles = _unitOfWork.Files.GetCleanUpPendingFiles(keys);
                if (!cleanUpPendingFiles.Any())
                {
                    return new ServiceResult
                    {
                        IsSuccess = false,
                        Error = "Something Went Wrong While Fetching cleanup pending files",
                        StatusCode = 400
                    };
                }

                foreach (var item in cleanUpPendingFiles)
                {
                    item.LastDeleteError = null;
                    item.StorageDeletedAt = DateTime.UtcNow;
                    item.cleanupStatus = Enums.CleanupStatus.StorageDeleted;
                }
                await _unitOfWork.BeginTransactionAsync();
                await _unitOfWork.SaveChangesAsync();
                await _unitOfWork.CommitTransactionAsync();
                return new ServiceResult
                {
                    IsSuccess = true,
                    Message = "Successfully Deleted Clean Up Files"
                };
            }
            catch (Exception ex)
            {
                await _unitOfWork.RevertTransactionAsync();
                _logger.LogError(ex, "Something Went Wrong");
                return new ServiceResult
                {
                    IsSuccess = false,
                    Error = "Something Went Wrong"
                };
            }
        }

        public async Task<ServiceResult> ChangeStatusOfFailureCleanUpFiles(List<FailureKeyObject> obj)
        {
            try
            {
                var failureKeys = obj.Select(o => o.Key).ToList();

                var cleanUpPendingFiles = _unitOfWork.Files.GetCleanUpPendingFiles(failureKeys);

                if (!cleanUpPendingFiles.Any())
                {
                    return new ServiceResult
                    {
                        IsSuccess = false,
                        Error = "Something Went Wrong While Fetching cleanup pending files",
                        StatusCode = 400
                    };
                }

                foreach (var item in cleanUpPendingFiles)
                {
                    item.LastDeleteError = obj.Where(o => o.Key == item.ObjectKey).Select(o => o.Error).ToString();
                    item.DeleteAttempts++;
                }
                await _unitOfWork.BeginTransactionAsync();
                await _unitOfWork.SaveChangesAsync();
                await _unitOfWork.CommitTransactionAsync();
                return new ServiceResult
                {
                    IsSuccess = true,
                    Message = "Successfully Updated Status of Failed Files"
                };
            }
            catch (Exception ex)
            {

                _logger.LogError(ex, "Something Went Wrong");
                return new ServiceResult
                {
                    IsSuccess = false,
                    Error = "Something Went Wrong",
                    StatusCode = 500
                };
            }

        }

        public async Task<ServiceResult> DeleteStorageCleanedFilesFromDb()
        {
            var fileIds = _unitOfWork.Files.GetFileIdsWhichAreCleanedFromStorage();

            if (!fileIds.Any())
            {
                _logger.LogInformation("No Files to Delete From DB");

                return new ServiceResult
                {
                    IsSuccess = true,
                    Message = "No Files to Delete From DB"
                };
            }

            try
            {
                await _unitOfWork.BeginTransactionAsync();

                await _unitOfWork.Files.DeleteFilesWithIdsAsync(fileIds);

                await _unitOfWork.CommitTransactionAsync();

                return new ServiceResult
                {
                    IsSuccess = true,
                    Message = "Files From DB Deleted Successfully"
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error Occured While Deleting Files From DB");

                return new ServiceResult
                {
                    IsSuccess = false,
                    Error = "Something Went Wrong",
                    StatusCode = 500
                };
            }
        }
    }
}
