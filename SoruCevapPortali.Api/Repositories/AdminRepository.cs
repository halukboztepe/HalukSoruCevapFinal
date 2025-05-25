using Microsoft.EntityFrameworkCore;
using SoruCevapPortali.Api.Models;
using AutoMapper;
using SoruCevapPortali.Api.DTOs;
using SoruCevapPortali.Api.Data;

namespace SoruCevapPortali.Api.Repositories
{
    public class AdminRepository
    {
        private readonly ApplicationDbContext _context;
        private readonly IMapper _mapper;

        public AdminRepository(ApplicationDbContext context, IMapper mapper)
        {
            _context = context;
            _mapper = mapper;
        }

        public async Task<AdminDashboardDto> GetDashboardStatsAsync()
        {
            var totalQuestions = await _context.Questions.CountAsync();
            var totalAnswers = await _context.Answers.CountAsync();
            var totalUsers = await _context.Users.CountAsync();
            var totalUser = await _context.UserRoles
                .Where(ur => ur.Role.Name == Roles.User)
                .CountAsync();

            return new AdminDashboardDto
            {
                TotalQuestions = totalQuestions,
                TotalAnswers = totalAnswers,
                TotalUsers = totalUsers,
                TotalStudents = totalUser
            };
        }

        public async Task<IEnumerable<QuestionDto>> GetAllQuestionsWithDetailsAsync(string searchTerm = "", string sortBy = "newest", int page = 1)
        {
            const int pageSize = 10;
            
            var query = _context.Questions
                .Include(q => q.User)
                .Include(q => q.Answers)
                .AsQueryable();

            // Apply search if provided
            if (!string.IsNullOrWhiteSpace(searchTerm))
            {
                searchTerm = searchTerm.ToLower();
                query = query.Where(q => 
                    q.Title.ToLower().Contains(searchTerm) || 
                    q.Content.ToLower().Contains(searchTerm));
            }

            // Apply sorting
            query = sortBy switch
            {
                "oldest" => query.OrderBy(q => q.CreatedDate),
                "mostAnswers" => query.OrderByDescending(q => q.Answers.Count),
                _ => query.OrderByDescending(q => q.CreatedDate) // "newest" is default
            };

            // Apply pagination
            var questions = await query
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            return _mapper.Map<IEnumerable<QuestionDto>>(questions);
        }

        public async Task<int> GetTotalQuestionsCountAsync(string searchTerm = "")
        {
            var query = _context.Questions.AsQueryable();

            if (!string.IsNullOrWhiteSpace(searchTerm))
            {
                searchTerm = searchTerm.ToLower();
                query = query.Where(q => 
                    q.Title.ToLower().Contains(searchTerm) || 
                    q.Content.ToLower().Contains(searchTerm));
            }

            return await query.CountAsync();
        }

        public async Task<(IEnumerable<AnswerDto> Answers, int TotalCount)> GetAllAnswersWithDetailsAsync(string searchTerm = "", string sortBy = "newest", int page = 1)
        {
            const int pageSize = 10;
            
            var query = _context.Answers
                .Include(a => a.User)
                .Include(a => a.Question)
                .AsQueryable();

            // Apply search if provided
            if (!string.IsNullOrWhiteSpace(searchTerm))
            {
                searchTerm = searchTerm.ToLower();
                query = query.Where(a => 
                    a.Content.ToLower().Contains(searchTerm) || 
                    a.Question.Title.ToLower().Contains(searchTerm) ||
                    a.User.UserName.ToLower().Contains(searchTerm));
            }

            // Get total count before applying pagination
            var totalCount = await query.CountAsync();

            // Apply sorting
            query = sortBy switch
            {
                "oldest" => query.OrderBy(a => a.CreatedDate),
                _ => query.OrderByDescending(a => a.CreatedDate) // "newest" is default
            };

            // Apply pagination
            var answers = await query
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            var answerDtos = _mapper.Map<IEnumerable<AnswerDto>>(answers);

            // Ensure question titles are included
            foreach (var dto in answerDtos)
            {
                var answer = answers.First(a => a.Id == dto.Id);
                dto.QuestionTitle = answer.Question?.Title ?? "Silinmiş Soru";
            }

            return (answerDtos, totalCount);
        }

        public async Task<bool> DeleteQuestionAsync(int questionId)
        {
            var question = await _context.Questions
                .Include(q => q.Answers)
                .FirstOrDefaultAsync(q => q.Id == questionId);

            if (question == null)
                return false;

            _context.Questions.Remove(question);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> DeleteAnswerAsync(int answerId)
        {
            var answer = await _context.Answers.FindAsync(answerId);
            if (answer == null)
                return false;

            _context.Answers.Remove(answer);
            await _context.SaveChangesAsync();
            return true;
        }
    }
}