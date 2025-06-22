using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IntelliPM.Data.DTOs.TaskFile.Response
{
    public class TaskFileResponseDTO
    {
        public int Id { get; set; }
        public string TaskId { get; set; }
        public string Title { get; set; } = null!;
        public string UrlFile { get; set; } = null!;
        public string? Status { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}
