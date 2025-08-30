using IntelliPM.Common.Attributes;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IntelliPM.Data.DTOs.Auth.Request
{
    public class LoginRequestDTO
    {
        [Required(ErrorMessage = "Username is required")]
        [DynamicMaxLength("username_length_limit")]
        public string Username { get; set; }

        [Required(ErrorMessage = "Password is required")]
        [DynamicMinLength("password_length_limit")]
        [DynamicMaxLength("password_length_limit")]
        public string Password { get; set; }
    }
    
}
