using Microsoft.EntityFrameworkCore;
using SoruCevapPortali.Api.Models;
using AutoMapper;
using SoruCevapPortali.Api.DTOs;
using SoruCevapPortali.Api.Data;

namespace SoruCevapPortali.Api.Repositories
{
    public class QuestionRepository : GenericRepository<Question>
    {
        private readonly IMapper _mapper;

        public QuestionRepository(ApplicationDbContext context, IMapper mapper) : base(context)
        {
            _mapper = mapper;
        }

        public async Task<IEnumerable<QuestionDto>> GetAllQuestionsWithDetailsAsync(string searchTerm = "", string sortBy = "newest")
        {
            var query = _dbSet
                .Include(q => q.User)
                .Include(q => q.Answers)
                .AsQueryable();

            // Apply search filter
            if (!string.IsNullOrWhiteSpace(searchTerm))
            {
                searchTerm = searchTerm.ToLower();
                query = query.Where(q => 
                    q.Title.ToLower().Contains(searchTerm) || 
                    q.Content.ToLower().Contains(searchTerm));
            }

            // Apply sorting
            query = sortBy.ToLower() switch
            {
                "oldest" => query.OrderBy(q => q.CreatedDate),
                "mostAnswers" => query.OrderByDescending(q => q.Answers.Count),
                _ => query.OrderByDescending(q => q.CreatedDate) // Default: newest
            };

            var questions = await query.ToListAsync();
            return _mapper.Map<IEnumerable<QuestionDto>>(questions);
        }

        public async Task<IEnumerable<QuestionDto>> GetQuestionsByUserIdAsync(string userId)
        {
            var questions = await _dbSet
                .Include(q => q.User)
                .Include(q => q.Answers)
                .Where(q => q.UserId == userId)
                .OrderByDescending(q => q.CreatedDate)
                .ToListAsync();

            var questionDtos = _mapper.Map<IEnumerable<QuestionDto>>(questions);
            foreach (var dto in questionDtos)
            {
                dto.IsOwner = true; // Kullanıcının kendi soruları olduğu için hepsi true
            }

            return questionDtos;
        }

        public async Task<QuestionDto> GetQuestionWithDetailsAsync(int id)
        {
            var question = await _dbSet
                .Include(q => q.User)
                .Include(q => q.Answers)
                    .ThenInclude(a => a.User)
                .FirstOrDefaultAsync(q => q.Id == id);

            return _mapper.Map<QuestionDto>(question);
        }

        public async Task<IEnumerable<AnswerDto>> GetQuestionAnswersAsync(int questionId)
        {
            var question = await _dbSet
                .Include(q => q.Answers)
                    .ThenInclude(a => a.User)
                .FirstOrDefaultAsync(q => q.Id == questionId);

            if (question == null)
                return Enumerable.Empty<AnswerDto>();

            return _mapper.Map<IEnumerable<AnswerDto>>(question.Answers);
        }

        public async Task<Question> CreateQuestionAsync(CreateQuestionDto questionDto, string userId)
        {
            var question = _mapper.Map<Question>(questionDto);
            question.UserId = userId;
            await AddAsync(question);
            return question;
        }

        public async Task<bool> DeleteQuestionAsync(int id, string userId, bool isAdmin)
        {
            var question = await _dbSet
                .Include(q => q.Answers)
                .FirstOrDefaultAsync(q => q.Id == id);

            if (question == null)
                return false;

            // Only allow deletion if user is admin or the question owner
            if (!isAdmin && question.UserId != userId)
                return false;

            await DeleteAsync(question);
            return true;
        }

        public async Task<bool> UpdateQuestionAsync(int id, UpdateQuestionDto questionDto, string userId)
        {
            var question = await _dbSet.FirstOrDefaultAsync(q => q.Id == id);
            if (question == null || question.UserId != userId)
                return false;

            question.Title = questionDto.Title;
            question.Content = questionDto.Content;
            await UpdateAsync(question);
            return true;
        }
    }
}