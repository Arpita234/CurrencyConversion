using System.ComponentModel.DataAnnotations;

namespace CurrencyConverter.Models
{
    public class QueryParameters
    {
        [Required]
        [StringLength(3, MinimumLength = 3, ErrorMessage ="Currency code must be of three characters length")]
        public string? SourceCurrency { get; set; } 

        [Required]
        [StringLength(3, MinimumLength = 3, ErrorMessage = "Currency code must be of three characters length")]

        public string? TargetCurrency { get; set; }

        [Required]
        [Range(0.0000001, Double.MaxValue, ErrorMessage ="Amount must be greater than zero")]
        [RegularExpression(@"^\d+(\.(\d{1,4}))?$", ErrorMessage = "Invalid Amount format")]
        public decimal Amount { get; set; }


    }
}
