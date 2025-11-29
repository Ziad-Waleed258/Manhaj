using System.ComponentModel.DataAnnotations;

namespace Manhaj.DTOs
{
    public class CourseVM
    {
        [Required]
        public int Id { get; set; }

        [Required]
        public string Title { get; set; }
        public string Description { get; set; }
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
        [Required]
        public int TeacherID { get; set; }
        
    }
}
