using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IntelliPM.Data.DTOs.Auth
{
    public class LoginResponseDTO
    {
        public int Id { get; set; }

        public string Username { get; set; } = null!;

        public string Email { get; set; } = null!;

        public string? Role { get; set; }

        public string AccessToken { get; set; }

        public string RefreshToken { get; set; }
    }
}
