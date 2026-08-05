using Picterest.CustomAttributes;
using System.ComponentModel.DataAnnotations;

namespace Picterest.DTO.Images
{
    public class ImageUploadDTO
    {
        [StringLength(100,ErrorMessage="Name Must Contain Maximum of 100 Characters")]
        public string? Name { get; set; }
        [StringLength(500, ErrorMessage = "Description cannot exceed 500 characters.")]
        public string? Description { get; set; }
        [Required(ErrorMessage = "Image File is required.")]
        [AllowedImageTypes(
            new[] { ".jpg", ".jpeg", ".png", ".webp" },
            new[] { "image/jpeg", "image/png", "image/webp" })]
        public required IFormFile File { get; set; }
    }
}
