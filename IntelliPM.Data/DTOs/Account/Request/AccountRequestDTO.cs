using IntelliPM.Common.Attributes;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IntelliPM.Data.DTOs.Account.Request
{
    public class AccountRequestDTO
    {
        [Required(ErrorMessage = "Username is required")]
        [MaxLength(25, ErrorMessage = "Username contains a maximum of 25 characters")]
        public string Username { get; set; } = null!;

        [Required(ErrorMessage = "Password is required")]
        [RegularExpression("^(?=.*[A-Z])(?=.*\\d)(?=.*[!@#$%^&*])[A-Za-z\\d!@#$%^&*]{6,12}$",
            ErrorMessage = "Password must be 6-12 characters with at least one uppercase letter, one number, and one special character (!@#$%^&*)")]
        [MinLength(6, ErrorMessage = "Password must be at least 6 characters long")]
        public string Password { get; set; } = null!;

        [Required(ErrorMessage = "Email is required")]
        [EmailAddress(ErrorMessage = "Invalid email address format")]
        public string Email { get; set; } = null!;

        [Required(ErrorMessage = "Gender is required")]
        [RegularExpression("^(MALE|FEMALE|OTHER)$", ErrorMessage = "Gender must be 'MALE', 'FEMALE', or 'OTHER'")]
        public string Gender { get; set; } = null!;

        [MaxLength(50, ErrorMessage = "Position cannot exceed 50 characters")]
        [DynamicCategoryValidation("account_position", Required = true)]
        public string? Position { get; set; }

        [Required(ErrorMessage = "Date of birth is required")]
        public DateOnly DateOfBirth { get; set; }
    }
}
