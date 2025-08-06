using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IntelliPM.Data.DTOs.Account.Response
{
    public class AccountInformationResponseDTO
    {

        public string? Username { get; set; }
        public string? FullName { get; set; }
        public string Gender { get; set; }

        public string? Email { get; set; }

        public string? Phone { get; set; }

        public string? Address { get; set; }

        public DateOnly? DateOfBirth { get; set; }

        public string? Picture { get; set; }

        public string? Role { get; set; }


    }
}
