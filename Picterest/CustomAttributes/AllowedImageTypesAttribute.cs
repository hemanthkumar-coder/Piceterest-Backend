using System.ComponentModel.DataAnnotations;

namespace Picterest.CustomAttributes
{
    public class AllowedImageTypesAttribute:ValidationAttribute
    {
        private readonly string[] _extensions;
        private readonly string[] _mimeTypes;
        public AllowedImageTypesAttribute(string[] extensions, string[] mimeTypes)
        {
            _extensions = extensions;
            _mimeTypes = mimeTypes;
        }

        protected override ValidationResult? IsValid(object? value, ValidationContext validationContext)
        {
            if (value is not IFormFile file)
                return ValidationResult.Success;

            var extension = Path.GetExtension(file.FileName);

            if (!_extensions.Contains(extension, StringComparer.OrdinalIgnoreCase))
            {
                return new ValidationResult($"Allowed Extension: {string.Join(", ", _extensions)}");
            }

            if (!_mimeTypes.Contains(file.ContentType, StringComparer.OrdinalIgnoreCase))
            {
                return new ValidationResult("Invalid Content Type");
            }

            return ValidationResult.Success;
        }
    }
}
