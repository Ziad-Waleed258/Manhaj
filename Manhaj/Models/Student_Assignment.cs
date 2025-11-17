using System.ComponentModel.DataAnnotations.Schema;

namespace Manhaj.Models
{
    public class Student_Assignment
    {
        
        public decimal Grade { get; set; }

        [ForeignKey("StudentId")]
        public Student Student { get; set; }
        public int StudentId { get; set; }

        [ForeignKey("AssignmentId")]
        public Assignment Assignment { get; set; }
        public int AssignmentId  { get; set; }
    }
}
