using Amazon.S3.Model;
using Picterest.DTO.FileStorage;
using Picterest.Enums;
using Picterest.Repositories.Interface;
using Picterest.Services.Interface;

namespace Picterest.Services.Implementation
{
    public class CleanUpStorageService : ICleanUpStorageService
    {
        private readonly ILogger<CleanUpStorageService> _logger;
        private readonly IConfiguration _configuration;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IFileStorageService _fileStorageService;
        private readonly IFileService _fileService;
        public CleanUpStorageService(ILogger<CleanUpStorageService> logger,IConfiguration configuration,IUnitOfWork unitOfWork,IFileStorageService fileStorageService,IFileService fileService) 
        {
            _logger = logger;
            _configuration = configuration;
            _unitOfWork = unitOfWork;
            _fileStorageService = fileStorageService;
            _fileService = fileService;

        }
        public async Task ProcessImageStorageCleanUp()
        {
            var timeInConfiguration = _configuration["RestorationTime"] ?? throw new ArgumentNullException("Restoration Time is not Specified");

            if (!int.TryParse(timeInConfiguration, out var holdTime))
            {
                _logger.LogError(
                    "Invalid value '{Value}' configured for RestorationTime.",
                    timeInConfiguration);

                throw new InvalidOperationException(
                    $"Configuration value 'RestorationTime' must be a valid integer. Current value: '{timeInConfiguration}'.");
            }

            try
            {
                var filesToDeleteInStorage = _unitOfWork.Images.GetSoftDeletedImageFileDataList();

                if(filesToDeleteInStorage.Count() <= 0)
                {
                    return;
                }

                var BucketName = StorageTypes.Images;

                var deleteObjectsRequest = new DeleteObjectsRequest
                {
                    BucketName = BucketName,
                    Objects = filesToDeleteInStorage.Select(i => new KeyVersion
                    {
                        Key = i.ObjectKey
                    }).ToList()
                };

                var response = await _fileStorageService.DeleteObjectsAsync(deleteObjectsRequest);

                if (response == null || !response.IsSuccess || response.Result == null)
                {
                    _logger.LogError("Something went wrong while deleting s3 objects");
                    return;
                }

                var deleteObjectsResponse = (DeletedObjectsResponse)response.Result;

                var SuccessKeys = deleteObjectsResponse.DeletedObjects != null ? deleteObjectsResponse.DeletedObjects.Select(i => i.Key).ToList():new List<string>();
                var FailureKeysObject = deleteObjectsResponse.DeleteErrors != null ? deleteObjectsResponse.DeleteErrors.Select(e => new FailureKeyObject
                {
                    Key = e.Key,
                    Error = e.Message
                }).ToList() : new List<FailureKeyObject>();

                await _fileService.ChangeStatusOfSuccessCleanUpFiles(SuccessKeys);
                await _fileService.ChangeStatusOfFailureCleanUpFiles(FailureKeysObject);
            }
            catch (Exception ex)
            {

                _logger.LogError(ex, "Something Went Wrong");

            }
        }
    }
}
