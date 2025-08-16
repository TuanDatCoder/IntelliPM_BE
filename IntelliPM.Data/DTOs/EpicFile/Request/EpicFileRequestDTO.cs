using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IntelliPM.Data.DTOs.EpicFile.Request
{
    public class EpicFileRequestDTO
    {
        [Required(ErrorMessage = "EpicId is required")]
        public string EpicId { get; set; } = null!;

        [Required(ErrorMessage = "Title is required")]
        public string Title { get; set; } = null!;

        [Required(ErrorMessage = "UrlFile is required")]
        public IFormFile UrlFile { get; set; } = null!;

        [Required(ErrorMessage = "CreatedBy is required")]
        public int CreatedBy { get; set; } 

    }
}
