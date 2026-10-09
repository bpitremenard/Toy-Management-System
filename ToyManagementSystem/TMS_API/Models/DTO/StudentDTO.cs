using System.ComponentModel.DataAnnotations;

namespace TMS_API.Models.DTO
{
    public class StudentDTO
    {
        public int StudentId { get; set; }

        [StringLength(255, ErrorMessage = "Placement cannot exceed 255 characters.")]
        public string? Placement { get; set; }

        [Range(1, 3, ErrorMessage = "Year must be between 1 and 8.")]
        public int? Year { get; set; }

        [StringLength(255, ErrorMessage = "OID cannot exceed 255 characters.")]
        public string? OID { get; set; }

        public List<string>? BorrowedToysHistory { get; set; }
        public List<string?> CurrentBorrowedToys { get; set; }

    }
}
