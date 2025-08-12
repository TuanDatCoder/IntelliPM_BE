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
                AuthorAvatar = c.Author?.Picture, 
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
                // ✅ 1) Kiểm tra document tồn tại
                var documentExists = await _repo.DocumentExistsAsync(request.DocumentId);
                if (!documentExists)
                {
                    throw new KeyNotFoundException($"Document with ID {request.DocumentId} not found.");
                }

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
                Console.WriteLine($"❌ Error creating comment: {detailedMessage}");
                throw;
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




        public async Task<DocumentCommentResponseDTO> UpdateAsync(
      int id,
      UpdateDocumentCommentRequestDTO request,
      int authorId)
        {
            if (request is null)
                throw new ArgumentNullException(nameof(request));

            var comment = await _repo.GetByIdAsync(id);
            if (comment is null)
                throw new KeyNotFoundException($"Comment with ID {id} not found.");

            if (comment.AuthorId != authorId)
                throw new UnauthorizedAccessException("Bạn không có quyền sửa bình luận này.");

            // Không có trường nào để cập nhật
            if (request.FromPos is null &&
                request.ToPos is null &&
                request.Content is null &&
                request.Comment is null)
                throw new ArgumentException("No field to update.");

            // Merge trước để validate logic tổng thể
            var newFromPos = request.FromPos ?? comment.FromPos;
            var newToPos = request.ToPos ?? comment.ToPos;
            var newContent = request.Content ?? comment.Content;
            var newText = request.Comment ?? comment.Comment;

            // Validate sau khi merge
            if (newFromPos < 0 || newToPos < 0)
                throw new ArgumentException("FromPos and ToPos must be >= 0.");

            if (newFromPos > newToPos)
                throw new ArgumentException("FromPos must be <= ToPos.");

            if (string.IsNullOrWhiteSpace(newContent))
                throw new ArgumentException("Content is required.");

            if (string.IsNullOrWhiteSpace(newText))
                throw new ArgumentException("Comment is required.");

            // Chỉ áp dụng field client gửi lên
            if (request.FromPos.HasValue) comment.FromPos = newFromPos;
            if (request.ToPos.HasValue) comment.ToPos = newToPos;
            if (request.Content != null) comment.Content = request.Content!;
            if (request.Comment != null) comment.Comment = request.Comment!;

            comment.UpdatedAt = DateTime.UtcNow;
            await _repo.UpdateAsync(comment);

            var author = await _repo.GetAuthorByIdAsync(authorId);

            return new DocumentCommentResponseDTO
            {
                Id = comment.Id,
                DocumentId = comment.DocumentId,
                AuthorId = authorId,
                AuthorName = author?.FullName ?? "Unknown",
                AuthorAvatar = author?.Picture,
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
