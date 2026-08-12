using Amazon.S3;
using Amazon.S3.Model;
using Amazon.S3.Util;
using Microsoft.Extensions.Options;
using Picterest.Configuration;
using Picterest.DTO.FileStorage;
using Picterest.HelperModels;
using Picterest.Services.Interface;
using System.Net;

namespace Picterest.Services.Implementation
{
    public class FileStorageService : IFileStorageService
    {
        private readonly IAmazonS3 _s3Client;
        private readonly SeaweedOptions _seaweedOptions;
        private readonly ILogger<FileStorageService> _logger;
        public FileStorageService(IAmazonS3 s3Client, IOptions<SeaweedOptions>seaweedOptions,ILogger<FileStorageService> logger)
        {
            _s3Client = s3Client;
            _seaweedOptions = seaweedOptions.Value;
            _logger = logger;
        }

        public async Task CreateBucketIfNotExistsAsync(string BucketName, CancellationToken cancellationToken = default)
        {
            if(string.IsNullOrWhiteSpace(BucketName))
            {
                throw new ArgumentException("Bucket name cannot be null or whitespace.", nameof(BucketName));
            }
            bool isBucketExists = await AmazonS3Util.DoesS3BucketExistV2Async(_s3Client, BucketName);

            if (isBucketExists)
                return;

            var request = new PutBucketRequest
            {
                BucketName = BucketName,
            };

            var response = await _s3Client.PutBucketAsync(request, cancellationToken);

            if(response.HttpStatusCode != System.Net.HttpStatusCode.OK)
            {
                throw new Exception($"Failed to create bucket {BucketName}. Status code: {response.HttpStatusCode}");
            }

        }

        public async Task<ServiceResult> DeleteAsync(string objectKey, string BucketName, CancellationToken cancellationToken = default)
        {
            if(string.IsNullOrWhiteSpace(objectKey))
            {
                return new ServiceResult
                {
                    IsSuccess = false,
                    Error = "Object key cannot be null or whitespace."
                };
            }
            if(string.IsNullOrWhiteSpace(BucketName))
            {
                return new ServiceResult
                {
                    IsSuccess = false,
                    Error = "Bucket name cannot be null or whitespace."
                };
            }

            try
            {
                try
                {
                    await _s3Client.GetObjectMetadataAsync(BucketName, objectKey, cancellationToken);
                }
                catch(AmazonS3Exception ex) when (ex.StatusCode == HttpStatusCode.NotFound)
                {
                    return new ServiceResult
                    {
                        IsSuccess = false,
                        Error = $"File is Already Deleted or Not Found."
                    };
                }

                var response = await _s3Client.DeleteObjectAsync(BucketName, objectKey, cancellationToken);
                if (response.HttpStatusCode != HttpStatusCode.NoContent &&
                    response.HttpStatusCode != HttpStatusCode.OK
                   )
                {
                    return new ServiceResult
                    {
                        IsSuccess = false,
                        Error = $"Failed to delete object. Status Code: {response.HttpStatusCode}"
                    };
                }

                return new ServiceResult
                {
                    IsSuccess = true,
                    Message = "File deleted successfully."
                };
            }
            catch(AmazonS3Exception ex)
            {
                return new ServiceResult
                {
                    IsSuccess = false,
                    Error = $"Error deleting the file: {ex.Message}"
                };
            }
            catch (Exception ex)
            {
                return new ServiceResult
                {
                    IsSuccess = false,
                    Error = $"Unexpected error occurred: {ex.Message}"
                };
            }
            
        }

        public async Task<ServiceResult> GetObjectAsync(string BucketName, string objectKey, CancellationToken cancellationToken = default)
        {
            if(string.IsNullOrWhiteSpace(BucketName)|| string.IsNullOrWhiteSpace(objectKey))
            {
                return new ServiceResult
                {
                    IsSuccess = false,
                    Error="BucketName or ObjectKey cannot be null or whitespace.",
                    StatusCode = 400
                };
            }

            try
            {
                var response = await _s3Client.GetObjectAsync(BucketName, objectKey, cancellationToken);

                return new ServiceResult
                {
                    IsSuccess = true,
                    Message = "Retrived File Successfully",
                    Result = response.ResponseStream
                };
            }
            catch (AmazonS3Exception ex)
            {
                _logger.LogError(ex, "Error occurred while accessing the file from S3. Bucket: {BucketName}, ObjectKey: {ObjectKey}", BucketName, objectKey);
                return HandleAmazonS3Exception(ex);
            }
            catch(Exception ex)
            {
                _logger.LogError(ex, "Error occurred while accessing the file from S3. Bucket: {BucketName}, ObjectKey: {ObjectKey}", BucketName, objectKey);
                return new ServiceResult
                {
                    IsSuccess = false,
                    Error = $"Something Went Wrong While Accessing the File",
                    StatusCode = 500
                };
            }
        }

        public async Task<ServiceResult> UpdateObjectAsync(string BucketName, string ObjectKey, IFormFile File, CancellationToken cancellationToken = default)
        {
            if(string.IsNullOrWhiteSpace(BucketName) || string.IsNullOrWhiteSpace(ObjectKey))
            {
                _logger.LogError("BucketName or Objectkey is Empty");
                return new ServiceResult
                {
                    IsSuccess = false,
                    Error = "Something Went Wrong",
                    StatusCode = 500
                };
            }

            try
            {
                //Open Stream to Upload File
                using var fileStream = File.OpenReadStream();
                var putObjectRequest = new PutObjectRequest
                {
                    BucketName = BucketName,
                    Key = ObjectKey,
                    InputStream = fileStream,
                    ContentType = File.ContentType
                };
                var response = await _s3Client.PutObjectAsync(putObjectRequest);
                if(response.HttpStatusCode != HttpStatusCode.OK)
                {
                    _logger.LogError("Failed to Upload the File to S3 with ObjectKey {objectKey} : StatusCode {code}",ObjectKey,response.HttpStatusCode);
                    return new ServiceResult
                    {
                        IsSuccess = false,
                        Error = "Something Went Wrong",
                        StatusCode = 500
                    };
                }
                return new ServiceResult
                {
                    IsSuccess = true,
                    Message = "File Uploaded to S3 Successfully",
                    Result = new FileUploadResult
                    {
                        ObjectKey = ObjectKey,
                        Bucket = BucketName,
                        ETag = response.ETag
                    }
                };
            }
            catch(AmazonS3Exception ex)
            {
                _logger.LogError(ex, "Error Updating Image in S3 with BucketName {bucketName} and ObjectKey {objectKey}", BucketName, ObjectKey);
                return HandleAmazonS3Exception(ex);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error Updating Image in S3 with BucketName {bucketName} and ObjectKey {objectKey}", BucketName, ObjectKey);
                return new ServiceResult
                {
                    IsSuccess = false,
                    Error = "Something Went Wrong",
                    StatusCode = 500
                };
            }
        }

        public async Task<ServiceResult> UploadAsync(IFormFile file, string FileName, CancellationToken cancellationToken = default)
        {
            try
            {
                var ObjectKey = $"{DateTime.UtcNow:yyyyMMdd}/{Guid.NewGuid()}";

                using var stream = file.OpenReadStream();

                var BucketName = "images";

                await CreateBucketIfNotExistsAsync(BucketName, cancellationToken);

                var request = new PutObjectRequest
                {
                    BucketName = BucketName,
                    Key = ObjectKey,
                    InputStream = stream,
                    ContentType = file.ContentType
                };

                var response = await _s3Client.PutObjectAsync(request, cancellationToken);
                

                return new ServiceResult
                {
                    IsSuccess = true,
                    Result = new FileUploadResult
                    {
                        ObjectKey = ObjectKey,
                        Bucket = BucketName,
                        ETag = response.ETag
                    }
                };
            }
            catch(AmazonS3Exception ex)
            {
                _logger.LogError(ex, "Error Uploading to S3 Storage");
                return new ServiceResult
                {
                    IsSuccess = false,
                    Error = $"Error Uploading the file {FileName}",
                    StatusCode = 500
                };
            }
            

        }
        public string GetImageUrl(string bucket, string objectKey)
        {
            var request = new GetPreSignedUrlRequest
            {
                BucketName = bucket,
                Key = objectKey,
                Expires = DateTime.UtcNow.AddMinutes(15),
                Verb = HttpVerb.GET
            };

            return _s3Client.GetPreSignedURL(request);
        }

        private ServiceResult HandleAmazonS3Exception(AmazonS3Exception ex)
        {
            switch (ex.StatusCode)
            {
                case HttpStatusCode.NotFound:
                    return new ServiceResult
                    {
                        IsSuccess = false,
                        Error = $"File Not Found.",
                        StatusCode = 404
                    };
                case HttpStatusCode.Unauthorized:
                    return new ServiceResult
                    {
                        IsSuccess = false,
                        Error = $"Unauthorized access to the file.",
                        StatusCode = 401
                    };
                case HttpStatusCode.BadRequest:
                    return new ServiceResult
                    {
                        IsSuccess = false,
                        Error = $"Bad request for the file.",
                        StatusCode = 400
                    };
                case HttpStatusCode.Forbidden:
                    return new ServiceResult
                    {
                        IsSuccess = false,
                        Error = $"Forbidden access to the file.",
                        StatusCode = 403
                    };
                case HttpStatusCode.InternalServerError:
                    return new ServiceResult
                    {
                        IsSuccess = false,
                        Error = $"Internal server error while accessing the file.",
                        StatusCode = 500
                    };
                default:
                    return new ServiceResult
                    {
                        IsSuccess = false,
                        Error = $"Error accessing the file: {ex.Message}",
                        StatusCode = (int)ex.StatusCode
                    };
                }
            }

        public async Task<ServiceResult> DeleteObjectsAsync(DeleteObjectsRequest deleteObjectsRequest, CancellationToken cancellationToken = default)
        {
            if(deleteObjectsRequest == null)
            {
                return new ServiceResult
                {
                    IsSuccess = false,
                    Error = "Delete Objects Request is null",
                    StatusCode = 400
                };
            }

            try
            {
                var response = await _s3Client.DeleteObjectsAsync(deleteObjectsRequest, cancellationToken);

                var deleteObjectResponse = new DeletedObjectsResponse
                {
                    DeletedObjects = response.DeletedObjects,
                    DeleteErrors = response.DeleteErrors,
                };

                return new ServiceResult
                {
                    IsSuccess = true,
                    Message = "Successfully Deleted Objects in S3 Storage",
                    Result = deleteObjectResponse
                };
            }
            catch (Exception ex)
            {

                _logger.LogError(ex, "Error Deleting Objects in s3");
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
