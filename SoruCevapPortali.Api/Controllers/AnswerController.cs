using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SoruCevapPortali.Api.DTOs;
using SoruCevapPortali.Api.Repositories;
using System.Security.Claims;

namespace SoruCevapPortali.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AnswerController : ControllerBase
    {
        private readonly AnswerRepository _answerRepository;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public AnswerController(AnswerRepository answerRepository, IHttpContextAccessor httpContextAccessor)
        {
            _answerRepository = answerRepository;
            _httpContextAccessor = httpContextAccessor;
        }

        [HttpGet("question/{questionId}")]
        public async Task<ActionResult<IEnumerable<AnswerDto>>> GetAnswersByQuestion(int questionId)
        {
            var answers = await _answerRepository.GetAnswersByQuestionIdAsync(questionId);
            if (!answers.Any())
                return Ok(new { message = "Bu soruya henüz cevap verilmemiş." });

            var userId = _httpContextAccessor.HttpContext.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            foreach (var answer in answers)
            {
                answer.IsOwner = !string.IsNullOrEmpty(userId) && answer.UserId == userId;
            }

            return Ok(answers);
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<AnswerDto>> GetAnswer(int id)
        {
            var answer = await _answerRepository.GetAnswerWithDetailsAsync(id);
            if (answer == null)
                return NotFound(new { message = $"ID'si {id} olan cevap bulunamadı." });

            return Ok(answer);
        }

        [Authorize]
        [HttpPost]
        public async Task<ActionResult<AnswerDto>> CreateAnswer(CreateAnswerDto answerDto)
        {
            var userId = _httpContextAccessor.HttpContext.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId))
                return Unauthorized(new { message = "Bu işlem için giriş yapmanız gerekmektedir." });

            var answer = await _answerRepository.CreateAnswerAsync(answerDto, userId);
            var createdAnswer = await _answerRepository.GetAnswerWithDetailsAsync(answer.Id);
            return CreatedAtAction(nameof(GetAnswer), new { id = answer.Id }, new { 
                message = "Cevap başarıyla oluşturuldu.",
                answer = createdAnswer 
            });
        }

        [Authorize]
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateAnswer(int id, [FromBody] string content)
        {
            var userId = _httpContextAccessor.HttpContext.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId))
                return Unauthorized(new { message = "Bu işlem için giriş yapmanız gerekmektedir." });

            var isAdmin = _httpContextAccessor.HttpContext.User.IsInRole("Admin");
            var result = await _answerRepository.UpdateAnswerAsync(id, content, userId, isAdmin);

            if (!result)
                return NotFound(new { message = "Cevap bulunamadı veya güncelleme yetkiniz bulunmamaktadır." });

            return Ok(new { message = "Cevap başarıyla güncellendi." });
        }

        [Authorize]
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteAnswer(int id)
        {
            var userId = _httpContextAccessor.HttpContext.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId))
                return Unauthorized(new { message = "Bu işlem için giriş yapmanız gerekmektedir." });

            var isAdmin = _httpContextAccessor.HttpContext.User.IsInRole("Admin");
            var result = await _answerRepository.DeleteAnswerAsync(id, userId, isAdmin);

            if (!result)
                return NotFound(new { message = "Cevap bulunamadı veya silme yetkiniz bulunmamaktadır." });

            return Ok(new { message = "Cevap başarıyla silindi." });
        }
    
    }
} 