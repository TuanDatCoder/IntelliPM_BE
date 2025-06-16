using IntelliPM.Data.DTOs.Document.Request;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using IntelliPM.Data.DTOs.Document.Response;

namespace IntelliPM.Services.DocumentServices
{


    public interface IDocumentService
    {
        Task<List<DocumentResponseDTO>> GetDocumentsByProject(int projectId);
        Task<DocumentResponseDTO> GetDocumentById(int id);

        Task<DocumentResponseDTO> CreateDocument(DocumentRequestDTO req);
        Task<DocumentResponseDTO> UpdateDocument(int id, UpdateDocumentRequest req);
    }

}
