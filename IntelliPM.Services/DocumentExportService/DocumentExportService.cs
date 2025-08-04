using CloudinaryDotNet;
using CloudinaryDotNet.Actions;
using IntelliPM.Data.Contexts;
using IntelliPM.Data.Entities;
using IntelliPM.Repositories.DocumentExportFileRepos;
using IntelliPM.Services.CloudinaryStorageServices;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IntelliPM.Services.DocumentExportService
{

    public class DocumentExportService
    {
        private readonly ICloudinaryStorageService _cloudinaryService;
       
        private readonly IDocumentExportFileRepository _exportFileRepo;

        public DocumentExportService(
            ICloudinaryStorageService cloudinaryService,
            Su25Sep490IntelliPmContext context,
            IDocumentExportFileRepository exportFileRepo)
        {
            _cloudinaryService = cloudinaryService;
         
            _exportFileRepo = exportFileRepo;
        }

        public async Task<string> ExportAndSavePdfAsync(IFormFile file, int documentId, int accountId)
        {
            using var stream = file.OpenReadStream();
            var fileUrl = await _cloudinaryService.UploadFileAsync(stream, file.FileName);

            var exportRecord = new DocumentExportFile
            {
                DocumentId = documentId,
                ExportedFileUrl = fileUrl,
                ExportedAt = DateTime.UtcNow,
                ExportedBy = accountId
            };

            try
            {
                await _exportFileRepo.AddAsync(exportRecord);
                await _exportFileRepo.SaveChangesAsync(); 
            }
            catch (DbUpdateException dbEx)
            {
                var sqlError = dbEx.InnerException?.Message;
                throw new Exception("Save failed: " + sqlError);
            }

            return fileUrl;
        }

    }

}
