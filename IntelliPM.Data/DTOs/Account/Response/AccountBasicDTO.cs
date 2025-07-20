using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IntelliPM.Data.DTOs.Account.Response
{
    public class AccountBasicDTO
    {
        public int Id { get; set; }
        public string? Username { get; set; }
        public string? FullName { get; set; }
    }
}
