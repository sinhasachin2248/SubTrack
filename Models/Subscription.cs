
using System.ComponentModel.DataAnnotations;

namespace SubTrack.Models
{
    public class Subscription : IValidatableObject
    {
        public int Id { get; set; }

        [StringLength(450)]
        public string? UserId { get; set; }

        [Required(ErrorMessage = "Subscription name is required.")]
        [StringLength(100, ErrorMessage = "Name cannot exceed 100 characters.")]
        public string Name { get; set; } = string.Empty;

        [Required(ErrorMessage = "Please select a category.")]
        [StringLength(50)]
        public string Category { get; set; } = string.Empty;

        [Range(0.01, 1000000,
            ErrorMessage = "Price must be greater than zero and at most 1,000,000.")]
        public decimal Price { get; set; }

        [Required(ErrorMessage = "Please select a billing cycle.")]
        [StringLength(20)]
        [RegularExpression("^(Monthly|Yearly)$",
            ErrorMessage = "Billing cycle must be Monthly or Yearly.")]
        public string BillingCycle { get; set; } = "Monthly";

        [DataType(DataType.Date)]
        public DateTime StartDate { get; set; } = DateTime.Today;

        [DataType(DataType.Date)]
        public DateTime NextRenewalDate { get; set; } = DateTime.Today.AddMonths(1);

        public bool IsActive { get; set; } = true;

        [StringLength(500, ErrorMessage = "Notes cannot exceed 500 characters.")]
        public string? Notes { get; set; }

        public IEnumerable<ValidationResult> Validate(
            ValidationContext validationContext)
        {
            if (StartDate == default)
            {
                yield return new ValidationResult(
                    "Please provide a valid start date.",
                    new[] { nameof(StartDate) });
            }

            if (NextRenewalDate == default)
            {
                yield return new ValidationResult(
                    "Please provide a valid renewal date.",
                    new[] { nameof(NextRenewalDate) });
            }

            if (StartDate != default &&
                NextRenewalDate != default &&
                NextRenewalDate.Date < StartDate.Date)
            {
                yield return new ValidationResult(
                    "The next renewal date cannot be before the start date.",
                    new[] { nameof(NextRenewalDate) });
            }
        }
    }
}
