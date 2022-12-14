using Ganss.Xss;
using Microsoft.EntityFrameworkCore;
using SharedTrip.Core.Contracts;
using SharedTrip.Core.Models.Comments;
using SharedTrip.Infrastructure.Data;
using SharedTrip.Infrastructure.Data.Entities;

namespace SharedTrip.Core.Services
{
    public class CommentService : ICommentService
    {
        private readonly ApplicationDbContext context;

        public CommentService(ApplicationDbContext context)
        {
            this.context = context;
        }

        public async Task<bool> CreateComment(AddCommentViewModel model)
        {
            var isAdded = false;

            var sanitizer = new HtmlSanitizer();
            sanitizer.AllowedTags.Clear();

            var sanitizedMessage = sanitizer.Sanitize(model.Content);

            if (string.IsNullOrEmpty(sanitizedMessage)
                || string.IsNullOrWhiteSpace(sanitizedMessage))
            {
                return isAdded;
            }

            var comment = new Comment
            {
                CreatorId = model.CreatorId,
                ReceiverId = model.ReceiverId,
                Content = sanitizedMessage,
                CreatedOn = DateTime.Now
            };

            await this.context.Comments.AddAsync(comment);
            await this.context.SaveChangesAsync();
            isAdded = true;

            return isAdded;
        }

        public async Task DeleteCommentAsync(int commentId)
        {
            var comment = await this.context
                .Comments
                .FirstOrDefaultAsync(c => c.Id == commentId);

            comment.IsDeleted = true;
            await this.context.SaveChangesAsync();
        }

        public async Task<CommentQueryServiceModel> GetAllByCommentsByUserIdAsync(string receiverId, int currentPage = 1, int commentsPerPage = 3)
        {
            var result = new CommentQueryServiceModel();

            var commentsQuery = this.context
                .Comments
                .Where(c => c.ReceiverId == receiverId && c.IsDeleted == false)
                .OrderByDescending(c => c.CreatedOn)
                .AsQueryable();

            var comments = await commentsQuery
                .Skip((currentPage - 1) * commentsPerPage)
                .Take(commentsPerPage)
                .Select(c => new CommentViewModel
                {
                    Id = c.Id,
                    Content = c.Content,
                    CreatedOn = c.CreatedOn.ToString("MM/dd/yy H:mm"),
                    CreatorId = c.CreatorId,
                    CreatorName = $"{c.Creator.FirstName} {c.Creator.LastName}",
                    CreatorProfileImageUrl = c.Creator.ProfilePictureUrl
                })
                .ToListAsync();

            result.Comments = comments;
            result.TotalCommentsCount = await commentsQuery.CountAsync();

            return result;
        }

        public async Task<int> GetCountOfCommentsAsync()
        {
            return await this.context
                .Comments
                .Where(c => c.IsDeleted == false)
                .CountAsync();
        }
    }
}