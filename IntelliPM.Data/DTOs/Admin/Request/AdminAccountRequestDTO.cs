using IntelliPM.Common.Attributes;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IntelliPM.Data.DTOs.Admin.Request
{
    public class AdminAccountRequestDTO
    {
        [Required(ErrorMessage = "Email is required")]
        [EmailAddress(ErrorMessage = "Invalid email address")]
        public string Email { get; set; }

        [DynamicCategoryValidation("account_position", Required = true)]
        public string Position { get; set; }

        [DynamicCategoryValidation("account_role", Required = true)]
        public string Role { get; set; }
    }
}
