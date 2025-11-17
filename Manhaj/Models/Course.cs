using System.ComponentModel.DataAnnotations.Schema;

namespace Manhaj.Models
{
    public class Course
    {
        public int Id { get; set; } 

        public string Title { get; set; }

        public Level level { get; set; }

        public decimal Price { get; set; }

        public double Duration { get; set; }

        public int Num_Of_Lectures { get; set; }

        public DateTime Creation_Date { get; set; }

        [ForeignKey("TeacherID")]
        public Teacher Teacher { get; set; }

        public int TeacherID { get; set; }

        // One Course → Many Ratings
        public ICollection<Rating> Ratings { get; set; }
        public ICollection<Lecture> Lectures { get; set; }

    }
}
