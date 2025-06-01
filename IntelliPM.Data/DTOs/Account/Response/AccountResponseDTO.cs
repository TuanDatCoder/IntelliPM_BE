using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IntelliPM.Data.DTOs.Account.Response
{
        public class AccountResponseDTO
        {
            public int Id { get; set; }
            public string? Username { get; set; }
            public string? Email { get; set; }
            public string? Gender { get; set; }
            public string? Position { get; set; }
            public DateOnly? DateOfBirth { get; set; }
            public string? Status { get; set; }
            public string? Role { get; set; }
        }
    
}
