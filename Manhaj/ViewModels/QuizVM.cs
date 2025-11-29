using System.ComponentModel.DataAnnotations;

namespace Manhaj.DTOs
{
    public class QuizVM
    {
        [Key]
        public int Id { get; set; }
        [Required]
        public string Title { get; set; }
        public int NumberOfQuestions { get; set; }
        [Required]
        public decimal TotalGrade { get; set; }
    }
}
