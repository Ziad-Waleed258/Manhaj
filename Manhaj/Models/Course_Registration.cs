using System.ComponentModel.DataAnnotations.Schema;

namespace Manhaj.Models
{
    public class Course_Registration
    {
        public DateTime Registration_Date { get; set; }

        public decimal Grade { get; set; }

        public int Progress { get; set; }

        [ForeignKey("CourseID")]
        public Course Course { get; set; }
        public int CourseID { get; set; }

        [ForeignKey("StudentID")]
        public Student Student { get; set; }
        public int StudentID { get; set; }
    }
}
