using System.ComponentModel.DataAnnotations;
using TMS_SharedLibrary.Models;

namespace TMS_API.Models.DTO
{
    public class ToyDTO : IValidatableObject
    {
        public int ToyId { get; set; }

        [Required(ErrorMessage = "Toy name is required.")]
        [StringLength(255, MinimumLength = 2, ErrorMessage = "Toy name must be between 2 and 255 characters.")]
        public string Name { get; set; } = null!;

        [StringLength(2000, ErrorMessage = "Description cannot exceed 2000 characters.")]
        public string? Description { get; set; }

        [StringLength(2000, ErrorMessage = "Additional information cannot exceed 2000 characters.")]
        public string? AdditionalInformation { get; set; }

        [StringLength(100, ErrorMessage = "Category cannot exceed 100 characters.")]
        public string? Category { get; set; }

        [StringLength(100, ErrorMessage = "Material cannot exceed 100 characters.")]
        public string? Material { get; set; }

        [Required(ErrorMessage = "Location code is required.")]
        [StringLength(10, MinimumLength = 2, ErrorMessage = "Location code must be between 2 and 10 characters.")]
        [RegularExpression(@"^[A-Za-z0-9\-]+$", ErrorMessage = "Location code can only contain letters, numbers, and hyphens.")]
        public string LocationCode { get; set; }

        [StringLength(255, ErrorMessage = "Image path cannot exceed 255 characters.")]
        public string? ImagePath { get; set; }

        public bool IsAvailable { get; set; }

        public bool IsActive { get; set; }

        public int? ManagedBy { get; set; }
        
        public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        {
            var model = new Toy
            {
                Name = Name,
                Description = Description,
                AdditionalInformation = AdditionalInformation,
                Category = Category,
                Material = Material,
                LocationCode = LocationCode,
                ImagePath = ImagePath
            };

            foreach (var result in model.Validate(validationContext))
            {
                yield return result;
            }
        }
    }
}
