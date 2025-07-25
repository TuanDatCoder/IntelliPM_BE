﻿
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

        Task<DocumentResponseDTO> CreateDocument(DocumentRequestDTO req, int userId);
        Task<DocumentResponseDTO> UpdateDocument(int id, UpdateDocumentRequest req);
        Task<List<DocumentResponseDTO>> GetDocumentsCreatedByUser(int userId);

        Task<string> SummarizeContent(int documentId);
        Task<string> GenerateAIContent(int documentId, string prompt);
        Task<string> GenerateFreeAIContent(string prompt);




        Task<ShareDocumentResponseDTO> ShareDocumentByEmail(int documentId, ShareDocumentRequestDTO req);

        Task<DocumentResponseDTO> SubmitForApproval(int documentId);
        Task<DocumentResponseDTO> UpdateApprovalStatus(int documentId, UpdateDocumentStatusRequest request);
        Task<List<DocumentResponseDTO>> GetDocumentsByStatus(string status);
        Task<List<DocumentResponseDTO>> GetDocumentsByStatusAndProject(string status, int projectId);

        Task<DocumentResponseDTO?> GetByKey(int projectId, string? epicId, string? taskId, string? subTaskId);

        Task<Dictionary<string, int>> GetUserDocumentMappingAsync(int projectId, int userId);










    }

}
