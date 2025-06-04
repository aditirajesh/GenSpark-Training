using Microsoft.AspNetCore.Mvc;
using BankApplication.Services;
using BankApplication.Models.DTOs;

namespace BankApplication.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class FAQController : ControllerBase
    {
        private readonly FAQService _faqService;

        public FAQController(FAQService faqService)
        {
            _faqService = faqService;
        }

        [HttpPost("ask")]
        public async Task<IActionResult> Ask([FromBody] QuestionDto dto)
        {
            if (string.IsNullOrWhiteSpace(dto.Question))
                return BadRequest("Question cannot be empty.");

            var answer = await _faqService.AskQuestionAsync(dto.Question);
            return Ok(new { answer });
        }
    }
}
