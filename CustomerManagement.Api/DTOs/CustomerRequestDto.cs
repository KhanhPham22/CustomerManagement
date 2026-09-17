using System.ComponentModel.DataAnnotations;

namespace CustomerManagement.Api.DTOs;

public class CustomerRequestDto
{
    [Required]
    [MaxLength(20)]
    public string CustomerCode { get; set; } = string.Empty;

    [Required]
    [MaxLength(100)]
    public string FullName { get; set; } = string.Empty;

    [EmailAddress]
    [MaxLength(150)]
    public string? Email { get; set; }

    [Required]
    [RegularExpression(@"^0[35789][0-9]{8}$",
        ErrorMessage = "Phone number must be a valid Vietnamese mobile number.")]
    public string PhoneNumber { get; set; } = string.Empty;

    public DateTime? DateOfBirth { get; set; }

    public bool IsActive { get; set; } = true;
}