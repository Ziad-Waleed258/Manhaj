using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Manhaj.Models
{
    public class Student_Assignment
    {
        [Required]
        public decimal Grade { get; set; }
        
        public string? SubmissionPath { get; set; }
        public DateTime? SubmissionDate { get; set; }

     
        public virtual Student Student { get; set; }
        public int StudentId { get; set; }

       
        public virtual Assignment Assignment { get; set; }
        public int AssignmentId  { get; set; }
    }
}
