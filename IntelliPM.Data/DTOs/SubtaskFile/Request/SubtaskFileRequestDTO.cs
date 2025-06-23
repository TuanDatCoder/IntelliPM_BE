using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IntelliPM.Data.DTOs.SubtaskFile.Request
{
    public class SubtaskFileRequestDTO
    {
        public string SubtaskId { get; set; } 

        public string Title { get; set; } = null!;

        public IFormFile File { get; set; } = null!;
    }
}
