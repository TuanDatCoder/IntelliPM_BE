using IntelliPM.Data.DTOs.Document.Request;
using IntelliPM.Data.DTOs.Document.Response;
using IntelliPM.Data.DTOs.ShareDocument.Request;
using IntelliPM.Data.DTOs.ShareDocument.Response;
using IntelliPM.Data.DTOs.ShareDocumentViaEmail;

namespace IntelliPM.Services.DocumentServices
{


    public interface IDocumentService
    {
        Task<List<DocumentResponseDTO>> GetDocumentsByProject(int projectId, int userId);
        Task<DocumentResponseDTO> GetDocumentById(int id);
        Task<List<DocumentResponseDTO>> GetAllDocuments();

        //Task<DocumentResponseDTO> CreateDocumentRequest(DocumentRequestDTO req, int userId);
        Task<DocumentResponseDTO> CreateDocument(DocumentRequestDTO req, int userId);

        Task<DocumentResponseDTO> UpdateDocument(int id, UpdateDocumentRequest req, int userId);

        Task<bool> DeleteDocument(int id, int deletedBy);

        //Task<List<DocumentResponseDTO>> GetDocumentsCreatedByUser(int userId);

        Task<string> SummarizeContent(int documentId);
        Task<string> GenerateAIContent(int documentId, string prompt);
        Task<string> GenerateFreeAIContent(string prompt);




        Task<ShareDocumentResponseDTO> ShareDocumentByEmail(int documentId, ShareDocumentRequestDTO req);

        //Task<DocumentResponseDTO> SubmitForApproval(int documentId);
        //Task<DocumentResponseDTO> UpdateApprovalStatus(int documentId, UpdateDocumentStatusRequest request);
        //Task<List<DocumentResponseDTO>> GetDocumentsByStatus(string status);
        //Task<List<DocumentResponseDTO>> GetDocumentsByStatusAndProject(string status, int projectId);

        //Task<DocumentResponseDTO?> GetByKey(int projectId, string? epicId, string? taskId, string? subTaskId);

        Task<Dictionary<string, int>> GetUserDocumentMappingAsync(int projectId, int userId);

        //Task<Dictionary<string, int>> GetStatusCount();

        //Task<Dictionary<string, int>> GetStatusCountByProject(int projectId);


        Task<GenerateDocumentResponse> GenerateFromProject(int documentId);
        Task<GenerateDocumentResponse> GenerateFromTask(int documentId);


        Task ShareDocumentViaEmailWithFile(ShareDocumentViaEmailRequest request);

        Task<string> GetUserPermissionLevel(int documentId, int userId);

        Task<DocumentResponseDTO> ChangeVisibilityAsync(int documentId, ChangeVisibilityRequest request, int currentUserId);

        Task<List<DocumentResponseDTO>> GetDocumentsSharedToUser(int userId);

        Task<List<DocumentResponseDTO>> GetDocumentsSharedToUserInProject(int userId, int projectId);







    }

}
