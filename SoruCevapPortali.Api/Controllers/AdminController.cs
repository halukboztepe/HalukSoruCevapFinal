using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SoruCevapPortali.Api.DTOs;
using SoruCevapPortali.Api.Models;
using SoruCevapPortali.Api.Repositories;

namespace SoruCevapPortali.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize(Roles = Roles.Admin)]
    public class AdminController : ControllerBase
    {
        private readonly AdminRepository _adminRepository;

        public AdminController(AdminRepository adminRepository)
        {
            _adminRepository = adminRepository;
        }

        [HttpGet("dashboard")]
        public async Task<ActionResult<AdminDashboardDto>> GetDashboardStats()
        {
            var stats = await _adminRepository.GetDashboardStatsAsync();
            return Ok(stats);
        }

        [HttpGet("questions")]
        public async Task<ActionResult<IEnumerable<QuestionDto>>> GetAllQuestions([FromQuery] string searchTerm = "", [FromQuery] string sortBy = "newest", [FromQuery] int page = 1)
        {
            var questions = await _adminRepository.GetAllQuestionsWithDetailsAsync(searchTerm, sortBy, page);
            var totalQuestions = await _adminRepository.GetTotalQuestionsCountAsync(searchTerm);
            var totalPages = (int)Math.Ceiling(totalQuestions / 10.0);

            return Ok(new { 
                questions = questions,
                currentPage = page,
                totalPages = totalPages,
                totalQuestions = totalQuestions
            });
        }

        [HttpGet("answers")]
        public async Task<ActionResult<IEnumerable<AnswerDto>>> GetAllAnswers([FromQuery] string searchTerm = "", [FromQuery] string sortBy = "newest", [FromQuery] int page = 1)
        {
            var (answers, totalCount) = await _adminRepository.GetAllAnswersWithDetailsAsync(searchTerm, sortBy, page);
            var totalPages = (int)Math.Ceiling(totalCount / 10.0);

            return Ok(new
            {
                answers = answers,
                currentPage = page,
                totalPages = totalPages,
                totalAnswers = totalCount
            });
        }

        [HttpDelete("questions/{id}")]
        public async Task<IActionResult> DeleteQuestion(int id)
        {
            var result = await _adminRepository.DeleteQuestionAsync(id);
            if (!result)
                return NotFound(new { message = $"ID'si {id} olan soru bulunamadı." });

            return Ok(new { message = "Soru başarıyla silindi." });
        }

        [HttpDelete("answers/{id}")]
        public async Task<IActionResult> DeleteAnswer(int id)
        {
            var result = await _adminRepository.DeleteAnswerAsync(id);
            if (!result)
                return NotFound(new { message = $"ID'si {id} olan cevap bulunamadı." });

            return Ok(new { message = "Cevap başarıyla silindi." });
        }
    }
} 