using Microsoft.EntityFrameworkCore;
using SoruCevapPortali.Api.Models;
using AutoMapper;
using SoruCevapPortali.Api.DTOs;
using SoruCevapPortali.Api.Data;

namespace SoruCevapPortali.Api.Repositories
{
    public class AnswerRepository : GenericRepository<Answer>
    {
        private readonly IMapper _mapper;

        public AnswerRepository(ApplicationDbContext context, IMapper mapper) : base(context)
        {
            _mapper = mapper;
        }

        public async Task<IEnumerable<AnswerDto>> GetAnswersByQuestionIdAsync(int questionId)
        {
            var answers = await _dbSet
                .Include(a => a.User)
                .Include(a => a.Question)
                .Where(a => a.QuestionId == questionId)
                .OrderByDescending(a => a.CreatedDate)
                .ToListAsync();

            var answerDtos = _mapper.Map<IEnumerable<AnswerDto>>(answers);
            foreach (var dto in answerDtos)
            {
                dto.UserName = answers.First(a => a.Id == dto.Id).User?.UserName ?? "Anonim";
                dto.UserId = answers.First(a => a.Id == dto.Id).UserId;
            }

            return answerDtos;
        }

        public async Task<AnswerDto> GetAnswerWithDetailsAsync(int id)
        {
            var answer = await _dbSet
                .Include(a => a.User)
                .Include(a => a.Question)
                .FirstOrDefaultAsync(a => a.Id == id);

            return _mapper.Map<AnswerDto>(answer);
        }

        public async Task<Answer> CreateAnswerAsync(CreateAnswerDto answerDto, string userId)
        {
            var answer = _mapper.Map<Answer>(answerDto);
            answer.UserId = userId;
            await AddAsync(answer);
            return answer;
        }

        public async Task<bool> UpdateAnswerAsync(int id, string content, string userId, bool isAdmin)
        {
            var answer = await _dbSet.FirstOrDefaultAsync(a => a.Id == id);
            if (answer == null)
                return false;

            if (!isAdmin && answer.UserId != userId)
                return false;

            answer.Content = content;
            await UpdateAsync(answer);
            return true;
        }

        public async Task<bool> DeleteAnswerAsync(int id, string userId, bool isAdmin)
        {
            var answer = await _dbSet.FirstOrDefaultAsync(a => a.Id == id);
            if (answer == null)
                return false;

            if (!isAdmin && answer.UserId != userId)
                return false;

            await DeleteAsync(answer);
            return true;
        }
        
       

    }
}