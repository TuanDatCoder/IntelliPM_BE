using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IntelliPM.Data.DTOs.EpicFile.Request
{
    public class EpicFileRequestDTO
    {

        public string EpicId { get; set; } = null!;

        public string Title { get; set; } = null!;

        public IFormFile UrlFile { get; set; } = null!;
    }
}
