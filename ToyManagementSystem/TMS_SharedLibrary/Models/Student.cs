using System.ComponentModel.DataAnnotations;

namespace TMS_SharedLibrary.Models;

public partial class Student : IValidatableObject
{
    public int StudentId { get; set; }

    public int UserId { get; set; }

    [StringLength(255, ErrorMessage = "Placement cannot exceed 255 characters.")]
    public string? Placement { get; set; }

    [Range(1, 3, ErrorMessage = "Year must be between 1 and 8.")]
    public int? Year { get; set; }

    public virtual ICollection<ToyLoan> ToyLoans { get; set; } = new List<ToyLoan>();

    public virtual User User { get; set; } = null!;

    public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
    {
        if (!string.IsNullOrWhiteSpace(Placement) && Placement.Trim().Length < 2)
        {
            yield return new ValidationResult("Placement must be at least 2 characters when provided.", new[] { nameof(Placement) });
        }
    }
}
