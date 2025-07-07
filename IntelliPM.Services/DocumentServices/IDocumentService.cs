
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using IntelliPM.Data.DTOs.Document.Response;
using IntelliPM.Data.DTOs.ShareDocument.Response;
using IntelliPM.Data.DTOs.ShareDocument.Request;
using IntelliPM.Data.DTOs.Document.Request;

namespace IntelliPM.Services.DocumentServices
{


    public interface IDocumentService
    {
        Task<List<DocumentResponseDTO>> GetDocumentsByProject(int projectId);
        Task<DocumentResponseDTO> GetDocumentById(int id);

        Task<DocumentResponseDTO> CreateDocument(DocumentRequestDTO req);
        Task<DocumentResponseDTO> UpdateDocument(int id, UpdateDocumentRequest req);
        Task<List<DocumentResponseDTO>> GetDocumentsCreatedByUser(int userId);

        Task<string> SummarizeContent(int documentId);

        Task<ShareDocumentResponseDTO> ShareDocumentByEmail(int documentId, ShareDocumentRequestDTO req);

        //Task<DocumentResponseDTO> SubmitForApproval(int documentId);
        //Task<DocumentResponseDTO> UpdateApprovalStatus(int documentId, UpdateDocumentStatusRequest request);
        //Task<List<DocumentResponseDTO>> GetDocumentsByStatus(string status);
        //Task<List<DocumentResponseDTO>> GetDocumentsByStatusAndProject(string status, int projectId);



    }

}
