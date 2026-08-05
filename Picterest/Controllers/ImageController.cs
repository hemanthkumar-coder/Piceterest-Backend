using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Picterest.DbModels;
using Picterest.DTO.Images;
using Picterest.Services.Interface;

namespace Picterest.Controllers
{
    [Authorize]
    [ApiController]
    [Route("api/[controller]")]
    public class ImageController:ControllerBase
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
            
            var ImageUploadServiceResult = await _imageService.UploadImage(dto);

            if(ImageUploadServiceResult == null)
            {
                return new JsonResult(new
                {
                    Message = "Something Went Wrong",
                    StatusCode = 500
                });
            }

            if(!ImageUploadServiceResult.IsSuccess)
            {
                return new JsonResult(new
                {
                    Message = "Failed to Upload Image",
                    StatusCode = 400
                });
            }
            var ImageDetails = ImageUploadServiceResult.Result as ImageMetaData;

            return new JsonResult(new
            {
                Message = "Image Uploaded Successfully",
                StatusCode = 200,
                ImageDetails
            });
        }

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
                    Message = ImageServiceResponse.Error,
                    StatusCode = ImageServiceResponse.StatusCode
                });
            }

            var ServiceReponseResult = ImageServiceResponse.Result as GetImageResponse;
            if(ServiceReponseResult == null)
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

            var ServiceResponse = await _imageService.GetImageMetaData(id);
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
                ImageDetails,
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

            var ImageDeleteResponse = await _imageService.SoftDelete(id);

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
        [HttpGet("getAll")]
        public async Task<IActionResult> GetAllImages()
        {
            var imagesResponse = await _imageService.GetAllImages();
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
            var ImagesList = imagesResponse.Result as List<ImageMetaData>;
            return new JsonResult(new
            {
                Success = true,
                Message = imagesResponse.Message,
                ImagesList
            });
        }
        [HttpPost("update")]
        public async Task<IActionResult> UpdateImage([FromForm] ImageUpdateDTO dto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }
            
            var UpdateSerivceResponse = await _imageService.UpdateImage(dto);

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
                ImageDetails
            });
        }

        [HttpGet("getAllRestorable")]
        public async Task<IActionResult> GetAllRestorableImages()
        {
            var restorableImagesResponse = await _imageService.GetRestorableImagesList();
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
                RestorableImagesList
            });
        }
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
            var restoreResponse = await _imageService.RestoreImages(imageIds);
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
