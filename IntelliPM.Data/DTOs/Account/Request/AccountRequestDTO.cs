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
        [DynamicMaxLength("username_length_limit")]
        public string Username { get; set; } = null!;

        [Required(ErrorMessage = "Password is required")]
        [RegularExpression("^(?=.*[A-Z])(?=.*\\d)(?=.*[!@#$%^&*])[A-Za-z\\d!@#$%^&*]{6,12}$",
            ErrorMessage = "Password must be 6-12 characters with at least one uppercase letter, one number, and one special character (!@#$%^&*)")]
        [DynamicMinLength("password_length_limit")]
        [DynamicMaxLength("password_length_limit")]
        public string Password { get; set; } = null!;

        [Required(ErrorMessage = "Email is required")]
        [EmailAddress(ErrorMessage = "Invalid email address format")]
        public string Email { get; set; } = null!;

        [DynamicCategoryValidation("account_gender", Required = false)]
        public string? Gender { get; set; }

        [DynamicCategoryValidation("account_position", Required = true)]
        public string? Position { get; set; }


    }
}
