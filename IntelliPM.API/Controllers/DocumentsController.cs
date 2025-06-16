using IntelliPM.Data.DTOs.Document.Request;

using IntelliPM.Services.DocumentServices;
using Microsoft.AspNetCore.Mvc;
using IntelliPM.Data.DTOs.Document.Response;

namespace IntelliPM.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class DocumentsController : ControllerBase
    {
        private readonly IDocumentService _documentService;

        public DocumentsController(IDocumentService documentService)
        {
            _documentService = documentService;
        }

      
        [HttpPost]
        public async Task<ActionResult<DocumentResponseDTO>> Create([FromBody] DocumentRequestDTO request)
        {
            var result = await _documentService.CreateDocument(request);
            return Ok(result);
        }

        [HttpPut("{id}")]
        public async Task<ActionResult<DocumentResponseDTO>> Update(int id, [FromBody] UpdateDocumentRequest request)
        {
            var result = await _documentService.UpdateDocument(id, request);
            return Ok(result);
        }

   
        [HttpGet("project/{projectId}")]
        public async Task<ActionResult<List<DocumentResponseDTO>>> GetByProject(int projectId)
        {
            var result = await _documentService.GetDocumentsByProject(projectId);
            return Ok(result);
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<DocumentResponseDTO>> GetById(int id)
        {
            var result = await _documentService.GetDocumentById(id);
            return Ok(result);
        }

    }
}
