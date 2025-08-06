using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IntelliPM.Data.DTOs.Auth.Request
{
    public class LoginGoogleRequestDTO
    {
        public required string Token { get; set; }
    }
}
