using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IntelliPM.Data.DTOs.TaskFile.Request
{
    public class TaskFileRequestDTO
    {
        public int TaskId { get; set; }
        public string Title { get; set; } = null!;
        public IFormFile File { get; set; } = null!;
    }
}
