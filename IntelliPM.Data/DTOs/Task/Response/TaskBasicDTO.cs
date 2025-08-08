using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IntelliPM.Data.DTOs.Task.Response
{
    public class TaskBasicDTO
    {
        public string Id { get; set; } = null!;
        public string Title { get; set; } = null!;
        public string? Status { get; set; }
        public decimal? PercentComplete { get; set; }
    }
}
