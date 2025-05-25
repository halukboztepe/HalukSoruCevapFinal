using System.ComponentModel.DataAnnotations;

namespace SoruCevapPortali.Api.DTOs
{
    public class QuestionDto
    {
        public int Id { get; set; }
        public string Title { get; set; }
        public string Content { get; set; }
        public DateTime CreatedDate { get; set; }
        public string UserName { get; set; }
        public string UserId { get; set; }
        public int AnswerCount { get; set; }
        public bool IsOwner { get; set; }
    }

    public class CreateQuestionDto
    {
        [Required]
        [MaxLength(500)]
        public string Title { get; set; }

        [Required]
        public string Content { get; set; }
    }

    public class UpdateQuestionDto
    {
        [Required]
        [MaxLength(500)]
        public string Title { get; set; }

        [Required]
        public string Content { get; set; }
    }

    public class AnswerDto
    {
        public int Id { get; set; }
        public string Content { get; set; }
        public DateTime CreatedDate { get; set; }
        public string UserName { get; set; }
        public string UserId { get; set; }
        public int QuestionId { get; set; }
        public string QuestionTitle { get; set; }
        public bool IsOwner { get; set; }
    }

    public class CreateAnswerDto
    {
        [Required]
        public string Content { get; set; }

        [Required]
        public int QuestionId { get; set; }
    }
}