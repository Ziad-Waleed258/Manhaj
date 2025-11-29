using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Manhaj.Models
{
    public class Student_Quiz
    {
       
        public virtual Student Student { get; set; }
        public int StudentId { get; set; }

    
        public virtual Quiz Quiz { get; set; }
        public int QuizId { get; set; }

        [Required]
        public decimal Grade { get; set; }
        
        public DateTime StartTime { get; set; } = DateTime.Now; // When the student started the quiz
        public DateTime SubmissionDate { get; set; } = DateTime.Now;
        public bool IsSubmitted { get; set; } = false;

        public virtual ICollection<Student_Quiz_Answer> Answers { get; set; }
    }
}
