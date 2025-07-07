using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace IntelliPM.Data.DTOs.Epic.Request
{
    public class TaskAssignedMembersRequestDTO
    {
        public int AccountId { get; set; }
        public string FullName { get; set; } 
        public string Picture { get; set; } 
    }
}
