using Amazon.S3;
using Picterest.DbModels;
using Picterest.DTO.Images;
using Picterest.HelperModels;
using Picterest.Repositories.Interface;
using Picterest.Services.Interface;
using Picterest.Utils;
using static System.Net.Mime.MediaTypeNames;

namespace Picterest.Services.Implementation
{
    public class ImageService : IImageService
    {
        private readonly IFileStorageService _fileStorageService;
        private readonly IUnitOfWork _unitOfWork;
        private readonly ILogger<ImageService> _logger;
        private readonly IConfiguration _configuration;

        public ImageService(IFileStorageService fileStorageService,IUnitOfWork unitOfWork, ILogger<ImageService> logger,IConfiguration configuration)
        {
            _fileStorageService = fileStorageService;
            _unitOfWork = unitOfWork;
            _logger = logger;
            _configuration = configuration;
        }

        public async Task<ServiceResult> GetAllImages(string userId)
        {
            try
            {

                if (!Guid.TryParse(userId, out var UID))
                {
                    return new ServiceResult
                    {
                        IsSuccess = false,
                        Error = "Please Login to Access Images",
                        StatusCode = 401
                    };
                }

                var images = _unitOfWork.Images.GetAllImages(UID);
                if (images == null)
                {
                    _logger.LogError("Images are null for Get All Images");
                    return new ServiceResult
                    {
                        IsSuccess = false,
                        Error = "Something Went Wrong",
                        StatusCode = 500
                    };
                }
                if(images.Count() == 0)
                {
                    return new ServiceResult
                    {
                        IsSuccess = true,
                        Message = "No Images Found",
                        Result = new List<ImageMetaData>()
                    };
                }
                var ImagesList = images.Select(i => new ImageMetaData
                {
                    ImageId = i.Id,
                    ImageName = i.Name,
                    ImageDescription = i.Description ?? string.Empty,
                    ImageSize = FileSizeHelper.FormatFileSize(i.Files.Size),
                    UploadedAt = i.CreatedAt,
                    ImageUrl = BuildImageApiUrl(i.Id)
                }).ToList();

                return new ServiceResult
                {
                    IsSuccess = true,
                    Message = "Successfully Retrieved All Images",
                    Result = ImagesList
                };

            }
            catch (Exception ex)
            {

                _logger.LogError(ex, "Error Retrieving Images");
                return new ServiceResult
                {
                    IsSuccess = false,
                    Error = "Something Went Wrong",
                    StatusCode = 500
                };
            }
        }

        public async Task<ServiceResult> GetDeletedImagesDetails()
        {
            try
            {
                var deletedImages = _unitOfWork.Images.GetDeletedImagesDetails();

                if (deletedImages == null || deletedImages.Count() == 0)
                {
                    return new ServiceResult
                    {
                        IsSuccess = true,
                        Message = "No Deleted Images Present",
                        Result = new List<BatchDeleteRequestItem>()
                    };
                }

                var FilesToDelete = deletedImages.Select(i => new BatchDeleteRequestItem
                {
                    FileId = i.Files.Id,
                    BucketName = i.Files.Bucket,
                    ObjectKey = i.Files.ObjectKey,
                }).ToList();

                return new ServiceResult
                {
                    IsSuccess = true,
                    Message = "Retrieved Deleted Images File Details",
                    Result = FilesToDelete
                };
            }
            catch (Exception ex)
            {

                _logger.LogError(ex, "Error Retrieving List of Deleted Images File Details");
                return new ServiceResult
                {
                    IsSuccess = false,
                    Error = "Something Went Wrong",
                    StatusCode = 500
                };
            }
        }

        public async Task<ServiceResult> GetImageFile(string id)
        {
            if (string.IsNullOrWhiteSpace(id))
            {
                return new ServiceResult
                {
                    IsSuccess = false,
                    Error = "Image ID is null or empty",
                    StatusCode = 400
                };
            }
            try
            {
                
                if (!Guid.TryParse(id, out var imageId))
                {
                    return new ServiceResult
                    {
                        IsSuccess = false,
                        Error = "Image ID is not a valid GUID",
                        StatusCode = 400
                    };
                }

                var image = await _unitOfWork.Images.GetByPkAsync(imageId);

                if (image == null)
                {
                    return new ServiceResult
                    {
                        IsSuccess = false,
                        Error = "Image not found",
                        StatusCode = 404
                    };
                }

                var fileId = image.FileId;

                var file = image.Files;

                if (file == null)
                {
                    return new ServiceResult
                    {
                        IsSuccess = false,
                        Error = "File not found for the image",
                        StatusCode = 404
                    };
                }

                var GetObjectResponse = await _fileStorageService.GetObjectAsync(file.Bucket, file.ObjectKey);
                if (GetObjectResponse == null)
                {
                    return new ServiceResult
                    {
                        IsSuccess = false,
                        Error = "Error retrieving the image from storage",
                        StatusCode = 500
                    };
                }

                if (!GetObjectResponse.IsSuccess)
                {
                    return new ServiceResult
                    {
                        IsSuccess = false,
                        Error = GetObjectResponse.Error ?? "Error retrieving the image from storage",
                        StatusCode = GetObjectResponse.StatusCode
                    };
                }

                GetImageResponse response = new GetImageResponse
                {
                    File = GetObjectResponse.Result as Stream ?? throw new ArgumentNullException(nameof(GetObjectResponse.Result)),
                    ImageName = image.Name,
                    ContentType = file.ContentType,
                    ImageId = image.Id,
                    FileSize = file.Size
                };

                return new ServiceResult
                {
                    IsSuccess = true,
                    Result = response,
                    Message = "Image retrieved successfully"
                };
            }
            catch (Exception ex)
            {

                _logger.LogError(ex, "Error Retrieving Image with Id {imageId}", id);
                return new ServiceResult
                {
                    IsSuccess = false,
                    Error = "Something Went Wrong",
                    StatusCode = 500
                };
            }


        }

        public async Task<ServiceResult> GetImageMetaData(string id, string userId)
        {
            if(string.IsNullOrWhiteSpace(id))
            {
                return new ServiceResult
                {
                    IsSuccess = false,
                    Error = "Image ID is null or empty",
                    StatusCode = 400
                };
            }

            try
            {
                
                if (!Guid.TryParse(id, out var imageId))
                {
                    return new ServiceResult
                    {
                        IsSuccess = false,
                        Error = "Given Image Id is not a valid Guid",
                        StatusCode = 400
                    };
                }
                if (!Guid.TryParse(userId, out var UID))
                {
                    return new ServiceResult
                    {
                        IsSuccess = false,
                        Error = "You dont have access to this resource",
                        StatusCode = 401
                    };
                }

                var image = await _unitOfWork.Images.GetImageFileByImageIdAndUserId(imageId, UID);

                if (image == null)
                {
                    return new ServiceResult
                    {
                        IsSuccess = false,
                        Error = "Image not Found",
                        StatusCode = 404
                    };
                }

                var file = image.Files;
                if (file == null)
                {
                    return new ServiceResult
                    {
                        IsSuccess = false,
                        Error = "File not Found",
                        StatusCode = 404
                    };
                }

                var ImageResponse = new ImageMetaData
                {
                    ImageId = imageId,
                    ImageName = image.Name,
                    ImageDescription = image.Description ?? string.Empty,
                    ImageSize = FileSizeHelper.FormatFileSize(file.Size),
                    UploadedAt = image.CreatedAt,
                    ImageUrl = BuildImageApiUrl(imageId)
                };

                return new ServiceResult
                {
                    IsSuccess = true,
                    Message = "Image Details Fetched Successfully",
                    StatusCode = 200,
                    Result = ImageResponse
                };
            }
            catch (Exception ex)
            {

                _logger.LogError(ex, "Error Occured while Retrieving Image with id {ImageId}", id);
                return new ServiceResult
                {
                    IsSuccess = false,
                    StatusCode = 500,
                    Error = "Something Went Wrong"
                };
            }

        }

        public async Task<ServiceResult> SoftDelete(string id,string userId)
        {
            if (string.IsNullOrEmpty(id))
            {
                return new ServiceResult
                {
                    IsSuccess = false,
                    Error = "Id Cannot be null or Empty",
                    StatusCode = 400
                };
            }
            try
            {
                
                if (!Guid.TryParse(id, out var ImageId))
                {
                    return new ServiceResult
                    {
                        IsSuccess = false,
                        Error = "please provide valid Id",
                        StatusCode = 400
                    };
                }
                if (!Guid.TryParse(userId, out var UID))
                {
                    return new ServiceResult
                    {
                        IsSuccess = false,
                        Error = "please provide valid Id",
                        StatusCode = 400
                    };
                }

                var image = await _unitOfWork.Images.GetImageFileByImageIdAndUserId(ImageId,UID);

                if (image == null)
                {
                    return new ServiceResult
                    {
                        IsSuccess = false,
                        Error = "Image Not Found",
                        StatusCode = 404
                    };
                }

                image.IsDeleted = true;
                image.DeletedAt = DateTime.UtcNow;
                MarkForDeletion(image);
                await _unitOfWork.SaveChangesAsync();

                return new ServiceResult
                {
                    IsSuccess = true,
                    Message = "Image Deleted Successfully",
                    StatusCode = 200
                };
            }
            catch (Exception ex)
            {

                _logger.LogError(ex, "Error Occured Deleting image with id {ImageId}", id);
                return new ServiceResult
                {
                    IsSuccess = true,
                    Error = "Something Went Wrong",
                    StatusCode = 500
                };
            }
        }

        private static void MarkForDeletion(DbModels.Image image)
        {
            image.IsDeleted = true;

            if (image.Files != null)
            {
                image.Files.cleanupStatus = Enums.CleanupStatus.PendingStorageDeletion;
            }
        }

        public async Task<ServiceResult> UpdateImage(ImageUpdateDTO dto,string userId)
        {
            
            if (!Guid.TryParse(dto.ImageId, out var ImageId))
            {
                return new ServiceResult
                {
                    IsSuccess = false,
                    Error = "Please Provide Valid Id",
                    StatusCode = 400
                };
            }
            if (!Guid.TryParse(userId, out var UID))
            {
                return new ServiceResult
                {
                    IsSuccess = false,
                    Error = "You Dont Have Access to Update Image",
                    StatusCode = 401
                };
            }
            //Check Whether Image with GivenId is present or not
            var image = await _unitOfWork.Images.GetImageFileByImageIdAndUserId(ImageId, UID);
            if(image == null)
            {
                return new ServiceResult
                {
                    IsSuccess = false,
                    Error = "Image Not Found",
                    StatusCode = 404
                };
            }

            var file = image.Files;

            var oldObjectKey = file.ObjectKey;
            var oldBucketName = file.Bucket;

            string FileName = dto.Name ?? file.FileName;


            FileUploadResult? fileUploadResult = null;
            
            try
            {
                
                if(dto.File != null)
                {

                    FileName = dto.Name == null ? Path.GetFileNameWithoutExtension(dto.File.FileName) : dto.Name;

                    var fileUpdateResponse = await _fileStorageService.UploadAsync(dto.File, FileName);

                    
                    if (!fileUpdateResponse.IsSuccess)
                    {
                        return new ServiceResult
                        {
                            IsSuccess = false,
                            Error = fileUpdateResponse.Error,
                            StatusCode = fileUpdateResponse.StatusCode,
                        };
                    }
                    fileUploadResult = fileUpdateResponse.Result as FileUploadResult;
                    if(fileUploadResult == null)
                    {
                        _logger.LogError("File Update Response Result is null");
                        return new ServiceResult
                        {
                            IsSuccess = false,
                            Error = "Something Went Wrong",
                            StatusCode = 500
                        };
                    }

                    

                    file.Size = dto.File.Length;
                    file.FileName = FileName;
                    file.ContentType = dto.File.ContentType;
                    file.UpdatedAt = DateTime.UtcNow;
                    file.ObjectKey = fileUploadResult.ObjectKey;

                }

                if (!string.IsNullOrWhiteSpace(dto.Name))
                {
                    image.Name = dto.Name.Trim();
                }
                if (!string.IsNullOrWhiteSpace(dto.Description))
                {
                    image.Description = dto.Description.Trim();
                }
                image.UpdatedAt = DateTime.UtcNow;
                using var transaction = await _unitOfWork.BeginTransactionAsync();
                await _unitOfWork.SaveChangesAsync();
                await _unitOfWork.CommitTransactionAsync();


                if(fileUploadResult != null)
                {
                    try
                    {
                        await _fileStorageService.DeleteAsync(oldObjectKey, oldBucketName);
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex,
                            "Failed to delete old object {ObjectKey}",
                            oldObjectKey);
                    }
                }

                var ImageDetails = new ImageMetaData
                {
                    ImageId = image.Id,
                    ImageName = image.Name,
                    ImageDescription = image.Description ?? string.Empty,
                    ImageSize = FileSizeHelper.FormatFileSize(file.Size),
                    UploadedAt = image.CreatedAt,
                    ImageUrl = BuildImageApiUrl(image.Id)
                };

                return new ServiceResult
                {
                    IsSuccess = true,
                    Message = "Image Updated Successfully",
                    Result = ImageDetails
                };

            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error Updating Image");

                await _unitOfWork.RevertTransactionAsync();

                //Delete the File which is Uploaded to the Storage if the Database Transaction Fails
                if (fileUploadResult != null)
                {
                    try
                    {
                        await _fileStorageService.DeleteAsync(
                            fileUploadResult.ObjectKey,
                            fileUploadResult.Bucket);
                    }
                    catch (Exception cleanupEx)
                    {
                        _logger.LogError(cleanupEx,
                            "Failed to delete uploaded object {ObjectKey}",
                            fileUploadResult.ObjectKey);
                    }
                }

                return new ServiceResult
                {
                    IsSuccess = false,
                    Error = "Something Went Wrong",
                    StatusCode = 500
                };
            }

            
        }

        public async Task<ServiceResult> UploadImage(ImageUploadDTO dto,string userId)
        {
            if(dto == null)
            {
                _logger.LogError("Image upload DTO is null");
                return new ServiceResult
                {
                    IsSuccess = false,
                    Error = "Upload DTO is null"
                };
            }

            if (!Guid.TryParse(userId, out Guid UploadedById))
            {
                _logger.LogError("Cannot Parse User Id {id}", userId);
                return new ServiceResult
                {
                    IsSuccess = false,
                    Error = "Invalid User ID",
                    StatusCode = 400
                };
            }

            var FileName = dto.Name !=null ? dto.Name : Path.GetFileNameWithoutExtension(dto.File.FileName);

            FileUploadResult? fileUploadResult = null;
            await using var transaction = await _unitOfWork.BeginTransactionAsync();
            try
            {
                var fileStorageServiceResult = await _fileStorageService.UploadAsync(dto.File, FileName);

                if (fileStorageServiceResult != null && !fileStorageServiceResult.IsSuccess)
                {
                    return new ServiceResult
                    {
                        IsSuccess = false,
                        Error = fileStorageServiceResult.Error,
                        StatusCode = fileStorageServiceResult.StatusCode
                    };
                }

                fileUploadResult = fileStorageServiceResult?.Result as FileUploadResult;

                var fileToStore = new DbModels.File
                {
                    Id = Guid.NewGuid(),
                    FileName = FileName,
                    ObjectKey = fileUploadResult?.ObjectKey ?? throw new ArgumentNullException(nameof(fileUploadResult.ObjectKey)),
                    Bucket = fileUploadResult?.Bucket ?? throw new ArgumentNullException(nameof(fileUploadResult.Bucket)),
                    ContentType = dto.File.ContentType,
                    Size = dto.File.Length
                };

                await _unitOfWork.Files.AddAsync(fileToStore);

                var ImageToStore = new DbModels.Image
                {
                    Id = Guid.NewGuid(),
                    Name = FileName,
                    Description = dto.Description,
                    FileId = fileToStore.Id,
                    UserId = UploadedById
                };

                await _unitOfWork.Images.AddAsync(ImageToStore);

                await _unitOfWork.SaveChangesAsync();

                await _unitOfWork.CommitTransactionAsync();
                var ImageDetails = new ImageMetaData
                {
                    ImageId = ImageToStore.Id,
                    ImageName = ImageToStore.Name,
                    ImageSize = FileSizeHelper.FormatFileSize(fileToStore.Size),
                    ImageDescription = ImageToStore.Description ?? string.Empty,
                    UploadedAt = ImageToStore.CreatedAt,
                    ImageUrl = BuildImageApiUrl(ImageToStore.Id)
                };
                return new ServiceResult
                {
                    IsSuccess = true,
                    Result = ImageDetails
                };
            }
            catch(AmazonS3Exception ex)
            {
                //Revert the Transaction if Any Data is Added to the Database
                await _unitOfWork.RevertTransactionAsync();
                //Delete the File which is Uploaded to the Storage if the Database Transaction Fails
                if (fileUploadResult != null)
                {
                    await _fileStorageService.DeleteAsync(fileUploadResult.ObjectKey, fileUploadResult.Bucket);
                }

                _logger.LogError(ex, "Error Uploading Image to S3 with File Name {FileName}", FileName);

                return new ServiceResult
                {
                    IsSuccess = false,
                    Error = $"Error Uploading Image: {ex.Message}"
                };
            }
            catch (Exception ex)
            {
                //Revert the Transaction if Any Data is Added to the Database
                await _unitOfWork.RevertTransactionAsync();

                //Delete the File which is Uploaded to the Storage if the Database Transaction Fails
                if (fileUploadResult != null)
                {
                    await _fileStorageService.DeleteAsync(fileUploadResult.ObjectKey, fileUploadResult.Bucket);
                }

                _logger.LogError(ex, "Error Uploading Image to S3 with File Name {FileName}", FileName);

                return new ServiceResult
                {
                    IsSuccess = false,
                    Error = "Something Went Wrong"
                };
            }
        }

        private string BuildImageApiUrl(Guid imageId)
        {
            const string imagePath = "/api/image/get?id=";
            var publicApiBaseUrl = _configuration["PublicApiBaseUrl"]?.TrimEnd('/');

            // A relative URL keeps storage details private when the API and frontend
            // are served from the same origin. Configure PublicApiBaseUrl when they
            // are hosted on different origins.
            return string.IsNullOrWhiteSpace(publicApiBaseUrl)
                ? $"{imagePath}{imageId}"
                : $"{publicApiBaseUrl}{imagePath}{imageId}";
        }
        private string BuildRestorableImageApiUrl(Guid imageId)
        {
            const string imagePath = "/api/image/getRestorableImage?id=";
            var publicApiBaseUrl = _configuration["PublicApiBaseUrl"]?.TrimEnd('/');

            // A relative URL keeps storage details private when the API and frontend
            // are served from the same origin. Configure PublicApiBaseUrl when they
            // are hosted on different origins.
            return string.IsNullOrWhiteSpace(publicApiBaseUrl)
                ? $"{imagePath}{imageId}"
                : $"{publicApiBaseUrl}{imagePath}{imageId}";
        }

        public async Task<ServiceResult> GetRestorableImagesList(string userId)
        {
            try
            {
                if (!Guid.TryParse(userId, out var UID))
                {
                    return new ServiceResult
                    {
                        IsSuccess = false,
                        Error = "You Dont Access to this Resource",
                        StatusCode = 401
                    };
                }

                var restorableImages = _unitOfWork.Images.GetRestorableImagesList(UID);

                if(restorableImages == null)
                {
                    _logger.LogError("Restorable Images List is null");
                    return new ServiceResult
                    {
                        IsSuccess = false,
                        Error = "Something Went Wrong",
                        StatusCode = 500
                    };
                }

                if (restorableImages.Count() == 0)
                {
                    return new ServiceResult
                    {
                        IsSuccess = true,
                        Message = "No Restorable Images Present",
                        Result = new List<ImageMetaData>()
                    };
                }

                var RestorableImagesList = restorableImages.Select(i => new ImageMetaData
                {
                    ImageId = i.Id,
                    ImageName = i.Name,
                    ImageDescription = i.Description ?? string.Empty,
                    ImageSize = FileSizeHelper.FormatFileSize(i.Files.Size),
                    UploadedAt = i.CreatedAt,
                    ImageUrl = BuildRestorableImageApiUrl(i.Id)
                }).ToList();

                return new ServiceResult
                {
                    IsSuccess = true,
                    Message = "Successfully Retrieved Restorable Images List",
                    Result = RestorableImagesList
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error Occured While Retrieving Restorable Images List");

                return new ServiceResult
                {
                    IsSuccess = false,
                    Error = "Something Went Wrong",
                    StatusCode = 500
                };
            }
        }

        public async Task<ServiceResult> GetRestorableImageFile(string id)
        {
            if (string.IsNullOrEmpty(id))
            {
                return new ServiceResult
                {
                    IsSuccess = false,
                    Error = "Invalid Image ID"
                };
            }


            if(!Guid.TryParse(id, out Guid imageId))
            {
                return new ServiceResult
                {
                    IsSuccess = false,
                    Error = "Invalid Image ID",
                    StatusCode = 400
                };
            }



            try
            {
                var restorableImage = _unitOfWork.Images.GetRestorableImage(imageId);

                if (restorableImage == null)
                {
                    return new ServiceResult
                    {
                        IsSuccess = false,
                        Error = "Restorable Image not found",
                        StatusCode = 404
                    };
                }

                var file = restorableImage.Files;
                if (file == null)
                {
                    return new ServiceResult
                    {
                        IsSuccess = false,
                        Error = "File not found for the restorable image",
                        StatusCode = 404
                    };
                }

                var GetObjectResponse = await _fileStorageService.GetObjectAsync(file.Bucket, file.ObjectKey);
                if (GetObjectResponse == null)
                {
                    return new ServiceResult
                    {
                        IsSuccess = false,
                        Error = "Error retrieving the image from storage",
                        StatusCode = 500
                    };
                }

                if (!GetObjectResponse.IsSuccess)
                {
                    return new ServiceResult
                    {
                        IsSuccess = false,
                        Error = GetObjectResponse.Error ?? "Error retrieving the image from storage",
                        StatusCode = GetObjectResponse.StatusCode
                    };
                }

                GetImageResponse response = new GetImageResponse
                {
                    File = GetObjectResponse.Result as Stream ?? throw new ArgumentNullException(nameof(GetObjectResponse.Result)),
                    ImageName = restorableImage.Name,
                    ContentType = file.ContentType,
                    ImageId = restorableImage.Id,
                    FileSize = file.Size
                };

                return new ServiceResult
                {
                    IsSuccess = true,
                    Result = response,
                    Message = "Image retrieved successfully"
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error Retrieving Restorable Image");
                return new ServiceResult
                {
                    IsSuccess = false,
                    Error = "Something Went Wrong",
                    StatusCode = 500
                };
            }
        }

        public async Task<ServiceResult> RestoreImages(List<string> imageIds,string userId)
        {
            if(imageIds == null || imageIds.Count == 0)
            {
                return new ServiceResult
                {
                    IsSuccess = false,
                    Error = "Invalid Image IDs",
                    StatusCode = 400
                };
            }

            try
            {
                if (!Guid.TryParse(userId, out Guid UID))
                {
                    return new ServiceResult
                    {
                        IsSuccess = false,
                        Error = "You dont have access to this operation",
                        StatusCode = 400
                    };
                }

                List<Guid> validImageIds = new List<Guid>();
                imageIds.ForEach(id =>
                {
                    if (Guid.TryParse(id, out Guid imageId))
                    {
                        validImageIds.Add(imageId);
                    }
                    else
                    {
                        _logger.LogError($"Invalid image id: {id}");
                        throw new InvalidOperationException("Invalid Image ID provided");
                    }
                });
                await _unitOfWork.BeginTransactionAsync();
                

                await _unitOfWork.Images.RestoreImages(validImageIds,UID);

                await _unitOfWork.CommitTransactionAsync();

                return new ServiceResult
                {
                    IsSuccess = true,
                    Message = "Images Restored Successfully",
                    StatusCode = 200
                };

            }
            catch (Exception ex)
            {
                await _unitOfWork.RevertTransactionAsync();
                _logger.LogError(ex, "Error Occured While Restoring the Specified Images");
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
