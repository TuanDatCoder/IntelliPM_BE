using IntelliPM.Repositories.DocumentExportFileRepos;
using IntelliPM.Services.DocumentExportService;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace IntelliPM.API.Controllers
{
    [Authorize]

    [ApiController]
    [Route("api/[controller]")]
    public class DocumentExportFileController : ControllerBase

    {
        private readonly DocumentExportService _documentExportService;
        private readonly IDocumentExportFileRepository _documentExportRepo;

        public DocumentExportFileController(
            DocumentExportService documentExportService,
            IDocumentExportFileRepository documentExportRepo)
        {
            _documentExportService = documentExportService;
            _documentExportRepo = documentExportRepo;
        }

        [HttpPost("{documentId}/export")]
        public async Task<IActionResult> ExportDocument(int documentId, IFormFile file)
        {
            var accountIdClaim = User.FindFirst("accountId")?.Value;
            if (!int.TryParse(accountIdClaim, out int accountId))
                return Unauthorized("Invalid account ID");

            if (file == null || file.Length == 0)
                return BadRequest("No file uploaded");
            Console.WriteLine($"Received file: {file.FileName}, ContentType: {file.ContentType}, Length: {file.Length}");
            var fileUrl = await _documentExportService.ExportAndSavePdfAsync(file, documentId, accountId);
            return Ok(new { fileUrl });
        }

        [HttpGet("lookup")]
        public async Task<IActionResult> LookupByFileUrl([FromQuery] string fileUrl)
        {
            if (string.IsNullOrWhiteSpace(fileUrl))
                return BadRequest("Missing fileUrl");

            var record = await _documentExportRepo.GetByFileUrlAsync(fileUrl);

            if (record == null)
                return NotFound("No matching export record found");

            return Ok(new
            {
                record.DocumentId,
                record.ExportedAt,
                record.ExportedBy,
                DocumentTitle = record.Document?.Title
            });
        }
    }
}
