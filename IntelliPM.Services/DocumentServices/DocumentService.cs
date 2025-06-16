using IntelliPM.Data.DTOs.Document.Request;
using IntelliPM.Data.Entities;
using IntelliPM.Repositories.DocumentRepos;
using IntelliPM.Data.DTOs.Document.Response;


namespace IntelliPM.Services.DocumentServices
{
 


    public class DocumentService : IDocumentService
    {
        private readonly IDocumentRepository _repo;

        public DocumentService(IDocumentRepository repo)
        {
            _repo = repo;
        }

        public async Task<List<DocumentResponseDTO>> GetDocumentsByProject(int projectId)
        {
            var docs = await _repo.GetByProjectAsync(projectId);
            return docs.Select(ToResponse).ToList();
        }

        public async Task<DocumentResponseDTO> GetDocumentById(int id)
        {
            var doc = await _repo.GetByIdAsync(id);
            if (doc == null)
                throw new Exception("Document not found");

            return ToResponse(doc);
        }


        public async Task<DocumentResponseDTO> CreateDocument(DocumentRequestDTO req)
        {
            var doc = new Document
            {
                ProjectId = req.ProjectId,
                TaskId = req.TaskId,
                Title = req.Title,
                Type = req.Type,
                Template = req.Template,
                Content = req.Content,
                FileUrl = req.FileUrl,
                CreatedBy = req.CreatedBy,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow,
                IsActive = true
            };

            await _repo.AddAsync(doc);
            await _repo.SaveChangesAsync();

            return ToResponse(doc);
        }

        public async Task<DocumentResponseDTO> UpdateDocument(int id, UpdateDocumentRequest req)
        {
            var doc = await _repo.GetByIdAsync(id);
            if (doc == null) throw new Exception("Document not found");

            doc.Content = req.Content ?? doc.Content;
            doc.FileUrl = req.FileUrl ?? doc.FileUrl;
            doc.UpdatedBy = req.UpdatedBy;
            doc.UpdatedAt = DateTime.UtcNow;

            await _repo.UpdateAsync(doc);
            await _repo.SaveChangesAsync();

            return ToResponse(doc);
        }

        private static DocumentResponseDTO ToResponse(Document doc)
        {
            return new DocumentResponseDTO
            {
                Id = doc.Id,
                ProjectId = doc.ProjectId,
                TaskId = doc.TaskId,
                Title = doc.Title,
                Type = doc.Type,
                Template = doc.Template,
                Content = doc.Content,
                FileUrl = doc.FileUrl,
                IsActive = doc.IsActive,
                CreatedBy = doc.CreatedBy,
                UpdatedBy = doc.UpdatedBy,
                CreatedAt = doc.CreatedAt,
                UpdatedAt = doc.UpdatedAt
            };
        }
    }

}
