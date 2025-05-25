using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SoruCevapPortali.Api.DTOs;
using SoruCevapPortali.Api.Repositories;
using System.Security.Claims;

namespace SoruCevapPortali.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class QuestionController : ControllerBase
    {
        private readonly QuestionRepository _questionRepository;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public QuestionController(QuestionRepository questionRepository, IHttpContextAccessor httpContextAccessor)
        {
            _questionRepository = questionRepository;
            _httpContextAccessor = httpContextAccessor;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<QuestionDto>>> GetAllQuestions([FromQuery] string searchTerm = "", [FromQuery] string sortBy = "newest")
        {
            var questions = await _questionRepository.GetAllQuestionsWithDetailsAsync(searchTerm, sortBy);
            if (!questions.Any())
                return Ok(new { message = "Henüz hiç soru bulunmamaktadır." });

            return Ok(questions);
        }

        [Authorize]
        [HttpGet("my-questions")]
        public async Task<ActionResult<IEnumerable<QuestionDto>>> GetMyQuestions()
        {
            var userId = _httpContextAccessor.HttpContext.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId))
                return Unauthorized(new { message = "Bu işlem için giriş yapmanız gerekmektedir." });

            var questions = await _questionRepository.GetQuestionsByUserIdAsync(userId);
            if (!questions.Any())
                return Ok(new { message = "Henüz hiç soru sormamışsınız." });

            return Ok(questions);
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<QuestionDto>> GetQuestion(int id)
        {
            var question = await _questionRepository.GetQuestionWithDetailsAsync(id);
            if (question == null)
                return NotFound(new { message = $"ID'si {id} olan soru bulunamadı." });

            var userId = _httpContextAccessor.HttpContext.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            question.IsOwner = !string.IsNullOrEmpty(userId) && question.UserId == userId;

            return Ok(question);
        }

        [HttpGet("{id}/answers")]
        public async Task<ActionResult<IEnumerable<AnswerDto>>> GetQuestionAnswers(int id)
        {
            var answers = await _questionRepository.GetQuestionAnswersAsync(id);
            if (!answers.Any())
                return Ok(new { message = "Bu soruya henüz cevap verilmemiş." });

            var userId = _httpContextAccessor.HttpContext.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            foreach (var answer in answers)
            {
                answer.IsOwner = !string.IsNullOrEmpty(userId) && answer.UserId == userId;
            }

            return Ok(answers);
        }

        [Authorize]
        [HttpPost]
        public async Task<ActionResult<QuestionDto>> CreateQuestion(CreateQuestionDto questionDto)
        {
            var userId = _httpContextAccessor.HttpContext.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId))
                return Unauthorized(new { message = "Bu işlem için giriş yapmanız gerekmektedir." });

            var question = await _questionRepository.CreateQuestionAsync(questionDto, userId);
            var createdQuestion = await _questionRepository.GetQuestionWithDetailsAsync(question.Id);
            return CreatedAtAction(nameof(GetQuestion), new { id = question.Id }, new { 
                message = "Soru başarıyla oluşturuldu.",
                question = createdQuestion 
            });
        }

        [Authorize]
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateQuestion(int id, UpdateQuestionDto questionDto)
        {
            var userId = _httpContextAccessor.HttpContext.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId))
                return Unauthorized(new { message = "Bu işlem için giriş yapmanız gerekmektedir." });

            var result = await _questionRepository.UpdateQuestionAsync(id, questionDto, userId);
            if (!result)
                return NotFound(new { message = "Soru bulunamadı veya düzenleme yetkiniz bulunmamaktadır." });

            return Ok(new { message = "Soru başarıyla güncellendi." });
        }

        [Authorize]
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteQuestion(int id)
        {
            var userId = _httpContextAccessor.HttpContext.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId))
                return Unauthorized(new { message = "Bu işlem için giriş yapmanız gerekmektedir." });

            var isAdmin = _httpContextAccessor.HttpContext.User.IsInRole("Admin");
            var result = await _questionRepository.DeleteQuestionAsync(id, userId, isAdmin);

            if (!result)
                return NotFound(new { message = "Soru bulunamadı veya silme yetkiniz bulunmamaktadır." });

            return Ok(new { message = "Soru başarıyla silindi." });
        }
    }
} 