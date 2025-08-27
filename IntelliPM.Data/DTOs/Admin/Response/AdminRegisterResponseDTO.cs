using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IntelliPM.Data.DTOs.Admin.Response
{
    public class AdminRegisterResponseDTO
    {
        public List<string> Successful { get; set; } = new List<string>();
        public List<RegistrationError> Failed { get; set; } = new List<RegistrationError>();
    }
    public class RegistrationError
    {
        public string Email { get; set; }
        public string ErrorMessage { get; set; }
    }
}
