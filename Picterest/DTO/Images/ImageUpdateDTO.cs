using Picterest.CustomAttributes;
using System.ComponentModel.DataAnnotations;

namespace Picterest.DTO.Images
{
    public class ImageUpdateDTO
    {
        [Required(ErrorMessage = "ImageId is Required")]
        public string ImageId { get; set; } = string.Empty;

        [AllowedImageTypes(
            new[] { ".jpg", ".jpeg", ".png", ".webp" },
            new[] { "image/jpeg", "image/png", "image/webp" })]
        public IFormFile? File { get; set; }

        [StringLength(100, ErrorMessage = "Name Must Contain Maximum of 100 Characters")]
        public string? Name { get; set; }
        [StringLength(500, ErrorMessage = "Description cannot exceed 500 characters.")]
        public string? Description { get; set; }
    }
}
