using IntelliPM.Data.DTOs.DocumentComment;
using IntelliPM.Data.Entities;
using IntelliPM.Repositories.DocumentCommentRepos;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IntelliPM.Services.DocumentCommentServices
{
    public class DocumentCommentService : IDocumentCommentService
    {
        private readonly IDocumentCommentRepository _repo;

        public DocumentCommentService(IDocumentCommentRepository repo)
        {
            _repo = repo;
        }

        public async Task<List<DocumentCommentResponseDTO>> GetByDocumentIdAsync(int documentId)
        {
            var comments = await _repo.GetByDocumentIdAsync(documentId);

            return comments.Select(c => new DocumentCommentResponseDTO
            {
                Id = c.Id,
                DocumentId = c.DocumentId,
                AuthorId = c.AuthorId,
                AuthorName = c.Author?.FullName ?? "Unknown", 
                FromPos = c.FromPos,
                ToPos = c.ToPos,
                Content = c.Content,
                Comment = c.Comment,
                CreatedAt = c.CreatedAt,
                UpdatedAt = c.UpdatedAt
            }).ToList();
        }


        public async Task<DocumentCommentResponseDTO> CreateAsync(DocumentCommentRequestDTO request, int userId)
        {
            try
            {
                var comment = new DocumentComment
                {
                    DocumentId = request.DocumentId,
                    AuthorId = userId,
                    FromPos = request.FromPos,
                    ToPos = request.ToPos,
                    Content = request.Content,
                    Comment = request.Comment,
                    CreatedAt = DateTime.UtcNow
                };

                await _repo.AddAsync(comment);

                var author = await _repo.GetAuthorByIdAsync(userId);

                return new DocumentCommentResponseDTO
                {
                    Id = comment.Id,
                    DocumentId = comment.DocumentId,
                    AuthorId = userId,
                    AuthorName = author?.FullName ?? "Unknown",
                    FromPos = comment.FromPos,
                    ToPos = comment.ToPos,
                    Content = comment.Content,
                    Comment = comment.Comment,
                    CreatedAt = comment.CreatedAt
                };
            }
            catch (Exception ex)
            {
                var detailedMessage = ex.InnerException?.Message ?? ex.Message;

                // ✅ Ghi log ra console hoặc logger nếu có
                Console.WriteLine($"❌ Error creating comment: {detailedMessage}");

                // 👉 Gợi ý: Nếu bạn có ILogger<DocumentCommentService> logger:
                // logger.LogError(ex, "Error creating comment");

                // ✅ Ném ra để controller bắt 500
                throw new Exception($"Lỗi khi tạo comment: {detailedMessage}");
            }
        }



        public async Task<DocumentCommentResponseDTO?> GetByIdAsync(int id)
        {
            var comment = await _repo.GetByIdAsync(id);
            if (comment == null) return null;

            return new DocumentCommentResponseDTO
            {
                Id = comment.Id,
                DocumentId = comment.DocumentId,
                AuthorId = comment.AuthorId,
                AuthorName = comment.Author?.FullName ?? "Unknown", 
                FromPos = comment.FromPos,
                ToPos = comment.ToPos,
                Content = comment.Content,
                Comment = comment.Comment,
                CreatedAt = comment.CreatedAt,
                UpdatedAt = comment.UpdatedAt
            };
        }




        public async Task<DocumentCommentResponseDTO> UpdateAsync(int id, DocumentCommentRequestDTO request, int authorId)
        {
            var comment = await _repo.GetByIdAsync(id);
            if (comment == null || comment.AuthorId != authorId)
                throw new UnauthorizedAccessException("Không có quyền hoặc comment không tồn tại");

            comment.Content = request.Content;
            comment.Comment = request.Comment;
            comment.FromPos = request.FromPos;
            comment.ToPos = request.ToPos;
            comment.UpdatedAt = DateTime.UtcNow;

            await _repo.UpdateAsync(comment);

            var author = await _repo.GetAuthorByIdAsync(authorId);

            return new DocumentCommentResponseDTO
            {
                Id = comment.Id,
                DocumentId = comment.DocumentId,
                AuthorId = authorId,
                AuthorName = author?.FullName ?? "Unknown",
                Content = comment.Content,
                Comment = comment.Comment,
                FromPos = comment.FromPos,
                ToPos = comment.ToPos,
                CreatedAt = comment.CreatedAt,
                UpdatedAt = comment.UpdatedAt
            };
        }



        public async Task<bool> DeleteAsync(int id, int authorId)
        {
            var comment = await _repo.GetByIdAsync(id);
            if (comment == null || comment.AuthorId != authorId)
                return false;

            await _repo.DeleteAsync(comment);
            return true;
        }



    }


}
