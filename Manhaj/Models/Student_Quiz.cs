using System.ComponentModel.DataAnnotations.Schema;

namespace Manhaj.Models
{
    public class Student_Quiz
    {
        [ForeignKey("StudentId")]
        public Student Student { get; set; }
        public int StudentId { get; set; }

        [ForeignKey("QuizId")]
        public Quiz Quiz { get; set; }
        public int QuizId { get; set; }

        public decimal Grade { get; set; }

        public string Link { get; set; }
    }
}
