using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;

namespace Manhaj.Models
{
    public class Quiz
    {
        public int Id { get; set; }

        public string URL { get; set; }

        public decimal TotalGrade { get; set; }

        // Optional link to Lecture (for lecture-specific quizzes)
        [ForeignKey("LectureId")]
        public Lecture Lecture { get; set; }
        public int? LectureId { get; set; }

        // Optional link to Course (for course-level quizzes, e.g., final exam)
        [ForeignKey("CourseId")]
        public Course Course { get; set; }
        public int? CourseId { get; set; }

        // One Quiz → Many Questions
        public ICollection<Question> Questions { get; set; }

        // One Quiz → Many Student Attempts
        public ICollection<Student_Quiz> Student_Quizzes { get; set; }
    }
}
