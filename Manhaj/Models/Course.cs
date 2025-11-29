using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Manhaj.Models
{
    public class Course
    {
        [Key]
        public int Id { get; set; }
        [Required]
        public string Title { get; set; }
        public string? Description { get; set; }
        [Required]
        public string level { get; set; }
        [Required]
        public decimal Price { get; set; }
        [Required]
        public double Duration { get; set; }
        [Required]
        public int Num_Of_Lectures { get; set; }
        [Required]
        public DateTime Creation_Date { get; set; }
        public DateTime? CreatedAt { get; set; }

       
        public virtual Teacher Teacher { get; set; }
        public int TeacherID { get; set; }

        // One Course → Many Ratings
    
        public virtual ICollection<Lecture> Lectures { get; set; }
        public virtual ICollection<Course_Registration> Course_Registrations { get; set; }
        public virtual ICollection<Course_Rating> Ratings { get; set; }


    }
}
