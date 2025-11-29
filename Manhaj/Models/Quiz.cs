using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Manhaj.Models
{
    public class Quiz
    {
        [Key]
        public int Id { get; set; }
        [Required]
        public string Title { get; set; }
        
        public decimal TotalGrade { get; set; } = 0;
        public int NumberOfQuestions { get; set; } = 0;
        
        [Required]
        public DateTime StartTime { get; set; } // When quiz becomes available

        [Required]
        public DateTime EndTime { get; set; } // When quiz becomes unavailable for new attempts
        
        public DateTime? Deadline { get; set; } // Kept for backward compatibility, but logic will shift to EndTime
        
        [Required]
        public int Duration { get; set; } // Duration in minutes (required)

        public virtual Lecture? Lecture { get; set; }
        public int? LectureId { get; set; }

        public virtual Course? Course { get; set; }
        public int? CourseId { get; set; }

        // One Quiz → Many Questions
        public virtual ICollection<Question> Questions { get; set; } = new List<Question>();

        // One Quiz → Many Student Attempts
        public virtual ICollection<Student_Quiz> Quizzes { get; set; } = new List<Student_Quiz>();
    }
}
