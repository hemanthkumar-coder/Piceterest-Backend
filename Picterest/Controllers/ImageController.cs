using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Picterest.DbModels;
using Picterest.DTO.Images;
using Picterest.HelperModels;
using Picterest.Services.Interface;
using System.Security.Claims;

namespace Picterest.Controllers
{
    [Authorize]
    [ApiController]
    [Route("api/[controller]")]
    public class ImageController:BaseController
    {
        private readonly IImageService _imageService;

        public ImageController(IImageService imageService) 
        { 
            _imageService = imageService;
        }

        [HttpPost("upload")]
        public async Task<IActionResult> UploadImage([FromForm] ImageUploadDTO dto)
        {
            if(!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? string.Empty;

            var ImageUploadServiceResult = await _imageService.UploadImage(dto,userId);

            if(ImageUploadServiceResult == null)
            {
                return new JsonResult(new
                {
                    Success = false,
                    Message = "Something Went Wrong",
                    StatusCode = 500
                });
            }

            if(!ImageUploadServiceResult.IsSuccess)
            {
                return new JsonResult(new
                {
                    Success = false,
                    Message = ImageUploadServiceResult.Error,
                    StatusCode = ImageUploadServiceResult.StatusCode
                });
            }
            var ImageDetails = ImageUploadServiceResult.Result as ImageMetaData;

            return new JsonResult(new
            {
                Message = ImageUploadServiceResult.Message,
                StatusCode = ImageUploadServiceResult.StatusCode,
                Data = ImageDetails
            });
        }

        [AllowAnonymous]
        [HttpGet("get")]
        public async Task<IActionResult> GetImage([FromQuery] string id)
        {
            if(string.IsNullOrWhiteSpace(id))
            {
                return BadRequest(new
                {
                    Message = "Image Id is required",
                    StatusCode = 400
                });
            }



            var ImageServiceResponse = await _imageService.GetImageFile(id);

            if(!ImageServiceResponse.IsSuccess)
            {
                return new JsonResult(new
                {
                    Success = false,
                    Message = ImageServiceResponse.Error,
                    StatusCode = ImageServiceResponse.StatusCode
                });
            }

            var ServiceReponseResult = ImageServiceResponse.Result as GetImageResponse;
            if(ServiceReponseResult == null)
            {
                return new JsonResult(new
                {
                    Success = false,
                    Message = "Something Went Wrong",
                    StatusCode = 500
                });
            }

            // Do not provide a download filename: this keeps the response inline so
            // it can be used directly as the src of an <img> element.
            return File(
                ServiceReponseResult.File!,
                ServiceReponseResult.ContentType!
            );

        }

        [HttpGet("metadata")]
        public async Task<IActionResult> GetImageMetaData([FromQuery] string id)
        {
            if (string.IsNullOrWhiteSpace(id))
            {
                return BadRequest(new
                {
                    Success = false,
                    Message = "Image Id is required",
                    StatusCode = 400
                });
            }

            var ServiceResponse = await _imageService.GetImageMetaData(id,UserId);
            if(ServiceResponse == null)
            {
                return new JsonResult(new
                {
                    Success = false,
                    Message = "Something Went Wrong",
                    StatusCode = 500
                });
            }

            if (!ServiceResponse.IsSuccess)
            {
                return new JsonResult(new
                {
                    Success = false,
                    Message = ServiceResponse.Error,
                    StatusCode = ServiceResponse.StatusCode
                });
            }

            var ImageDetails = ServiceResponse.Result as ImageMetaData;

            return new JsonResult(new
            {
                Success = true,
                Message = "Image Details Fetched Successfully",
                Data = ImageDetails,
                StatusCode = 200
            });
            
        }
        [HttpPost("delete")]
        public async Task<IActionResult> DeleteImage([FromBody] string id)
        {
            if (string.IsNullOrEmpty(id))
            {
                return new JsonResult(new
                {
                    Success = false,
                    Message = "Id is Required",
                    StatusCode = 400
                });
            }

            var ImageDeleteResponse = await _imageService.SoftDelete(id,UserId);

            if(ImageDeleteResponse == null)
            {
                return new JsonResult(new
                {
                    Success = false,
                    Message = "Something Went Wrong",
                    StatusCode = 500
                });
            }

            if (!ImageDeleteResponse.IsSuccess)
            {
                return new JsonResult(new
                {
                    Success = false,
                    Message = ImageDeleteResponse.Error,
                    StatusCode = ImageDeleteResponse.StatusCode
                });
            }

            return new JsonResult(new
            {
                Success = true,
                Message = ImageDeleteResponse.Message,
                StatusCode = ImageDeleteResponse.StatusCode
            });

        }
        [HttpPost("getAll")]
        public async Task<IActionResult> GetAllImages([FromBody] PaginatedRequest request)
        {
            var imagesResponse = await _imageService.GetAllImages(UserId,request);
            if(imagesResponse == null)
            {
                return new JsonResult(new
                {
                    Success = false,
                    Message = "Something Went Wrong",
                    StatusCode = 500
                });
            }

            if (!imagesResponse.IsSuccess)
            {
                return new JsonResult(new
                {
                    Success = false,
                    Message = imagesResponse.Error,
                    StatusCode = imagesResponse.StatusCode
                });
            }
            var ImagesList = imagesResponse.Result as PaginatedResponse<ImageMetaData>;
            return new JsonResult(new
            {
                Success = true,
                Message = imagesResponse.Message,
                Data = ImagesList
            });
        }
        [HttpPost("update")]
        public async Task<IActionResult> UpdateImage([FromForm] ImageUpdateDTO dto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }
            
            var UpdateSerivceResponse = await _imageService.UpdateImage(dto,UserId);

            if (!UpdateSerivceResponse.IsSuccess)
            {
                return new JsonResult(new
                {
                    Success = false,
                    Error = UpdateSerivceResponse.Error,
                    StatusCode = UpdateSerivceResponse.StatusCode
                });
            }

            var ImageDetails = UpdateSerivceResponse.Result as ImageMetaData;
            return new JsonResult(new
            {
                Success = true,
                Message = "Image Uploaded Success fully",
                Data = ImageDetails
            });
        }

        [HttpGet("getAllRestorable")]
        public async Task<IActionResult> GetAllRestorableImages()
        {
            var restorableImagesResponse = await _imageService.GetRestorableImagesList(UserId);
            if(restorableImagesResponse == null)
            {
                return new JsonResult(new
                {
                    Success = false,
                    Message = "Something Went Wrong",
                    StatusCode = 500
                });
            }

            if (!restorableImagesResponse.IsSuccess)
            {
                return new JsonResult(new
                {
                    Success = false,
                    Message = restorableImagesResponse.Error,
                    StatusCode = restorableImagesResponse.StatusCode
                });
            }

            var RestorableImagesList = restorableImagesResponse.Result as List<ImageMetaData>;
            return new JsonResult(new
            {
                Success = true,
                Message = restorableImagesResponse.Message,
                Data = RestorableImagesList
            });
        }
        [AllowAnonymous]
        [HttpGet("getRestorableImage")]
        public async Task<IActionResult> GetRestorableImage([FromQuery] string id)
        {
            if (string.IsNullOrEmpty(id))
            {
                return new JsonResult(new
                {
                    Success = false,
                    Message = "Invalid Image ID",
                    StatusCode = 400
                });
            }

            var restorableImageResponse = await _imageService.GetRestorableImageFile(id);
            if (restorableImageResponse == null)
            {
                return new JsonResult(new
                {
                    Success = false,
                    Message = "Something Went Wrong",
                    StatusCode = 500
                });
            }

            if (!restorableImageResponse.IsSuccess)
            {
                return new JsonResult(new
                {
                    Success = false,
                    Message = restorableImageResponse.Error,
                    StatusCode = restorableImageResponse.StatusCode
                });
            }

            var ServiceReponseResult = restorableImageResponse.Result as GetImageResponse;
            if (ServiceReponseResult == null)
            {
                return new JsonResult(new
                {
                    Message = "Something Went Wrong",
                    StatusCode = 500
                });
            }

            // Do not provide a download filename: this keeps the response inline so
            // it can be used directly as the src of an <img> element.
            return File(
                ServiceReponseResult.File!,
                ServiceReponseResult.ContentType!
            );
        }
        [HttpPost("restore")]
        public async Task<IActionResult> RestoreImages([FromBody] List<string> imageIds)
        {
            if(imageIds == null || imageIds.Count == 0)
            {
                return new JsonResult(new
                {
                    Success = false,
                    Message = "No Image IDs provided for restoration.",
                    StatusCode = 400
                });
            }
            var restoreResponse = await _imageService.RestoreImages(imageIds,UserId);
            if (restoreResponse == null)
            {
                return new JsonResult(new
                {
                    Success = false,
                    Message = "Something Went Wrong",
                    StatusCode = 500
                });
            }
            if (!restoreResponse.IsSuccess)
            {
                return new JsonResult(new
                {
                    Success = false,
                    Message = restoreResponse.Error,
                    StatusCode = restoreResponse.StatusCode
                });
            }
            return new JsonResult(new
            {
                Success = true,
                Message = restoreResponse.Message,
                StatusCode = restoreResponse.StatusCode
            });
        }
    }
}
