using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.VisualBasic;

namespace Manhaj.Models
{
    public class Assignment
    {
        [Key]
        public int Id { get; set; }
        [Required]
        public string Title { get; set; }
        [Required]
        public string Description { get; set; }
        
        public string? MediaUrl { get; set; }

        public string? Comment { get; set; }
        [Required]
        public DateTime Deadline { get; set; }


        public virtual ICollection<Student_Assignment>? Assignments { get; set; }


        public virtual Lecture? Lecture {  get; set; } 
        public int lectureID { get; set; }
    }
}
