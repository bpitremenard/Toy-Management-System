using System.ComponentModel.DataAnnotations;

namespace TMS_SharedLibrary.Models;

public partial class Toy : IValidatableObject
{
    private static readonly HashSet<string> AllowedCategories = new(StringComparer.OrdinalIgnoreCase)
    {
        "Cognitive",
        "Social and Emotional (Dramatic Play)",
        "Sensory",
        "Physical"
    };

    private static readonly HashSet<string> AllowedMaterials = new(StringComparer.OrdinalIgnoreCase)
    {
        "Wood",
        "Plastic",
        "Plush",
        "Metal",
        "Rubber"
    };

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

    [Required(ErrorMessage = "Location is required.")]
    [StringLength(10, MinimumLength = 2, ErrorMessage = "Location code must be between 2 and 10 characters.")]
    [RegularExpression(@"^[A-Za-z0-9\-]+$", ErrorMessage = "Location code can only contain letters, numbers, and hyphens.")]
    public string LocationCode { get; set; }

    [StringLength(255, ErrorMessage = "Image path cannot exceed 255 characters.")]
    public string? ImagePath { get; set; }

    public bool IsAvailable { get; set; }

    public bool IsActive { get; set; }

    public int? ManagedBy { get; set; }

    public virtual Teacher? ManagedByNavigation { get; set; }

    public virtual ICollection<ToyLoan> ToyLoans { get; set; } = new List<ToyLoan>();

    public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
    {
        if (string.IsNullOrWhiteSpace(Name))
        {
            yield return new ValidationResult("Toy name cannot be blank.", new[] { nameof(Name) });
        }

        if (string.IsNullOrWhiteSpace(LocationCode))
        {
            yield return new ValidationResult("Location code cannot be blank.", new[] { nameof(LocationCode) });
        }

        if (!string.IsNullOrWhiteSpace(Category) && !AllowedCategories.Contains(Category))
        {
            yield return new ValidationResult("Category must be one of the supported development goals.", new[] { nameof(Category) });
        }

        if (!string.IsNullOrWhiteSpace(Material) && !AllowedMaterials.Contains(Material))
        {
            yield return new ValidationResult("Material must be one of the supported toy materials.", new[] { nameof(Material) });
        }
    }
}
