using IntelliPM.Data.DTOs.EpicComment.Response;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IntelliPM.Data.DTOs.Account.Response
{
    public class AccountWithWorkItemDTO
    {
        public int Id { get; set; }
        public string? Username { get; set; }
        public string? FullName { get; set; }

        public List<WorkItemResponseDTO> WorkItems { get; set; } = new List<WorkItemResponseDTO>();
    }

    public class WorkItemResponseDTO
    {
        public string Key { get; set; } = null!;
        public int ProjectId { get; set; }
        public string Summary { get; set; } = null!;
        public string Type { get; set; } = null!;
        public string? Status { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
    }

}
