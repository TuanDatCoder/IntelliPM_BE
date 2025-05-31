
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IntelliPM.Data.DTOs.Account
{
    public class AccountResponseDTO
    {

        public int? StoreId { get; set; }
        public string? StoreName { get; set; }

        public string? Username { get; set; }
        public string? FullName { get; set; }
        public string? Gender { get; set; }

        public string? Email { get; set; }

        public string? Phone { get; set; }

        public string? Address { get; set; }

        public DateOnly? DateOfBirth { get; set; }

        public string? Picture { get; set; }

        public string? GoogleId { get; set; }

        public string? Role { get; set; }

        public string? Status { get; set; }

        public int? Points { get; set; }

        public DateTime? CreatedAt { get; set; }

        public DateTime? UpdatedAt { get; set; }
    }
}
