using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Manhaj.Models
{
    public class Teacher : User
    {
        [Required(ErrorMessage = "التخصص مطلوب")]
        public string Specialization { get; set; }

        public bool IsApproved { get; set; } = false;
        public string? NationalIdFrontPath { get; set; }
        public string? NationalIdBackPath { get; set; }

        // Navigation property: one teacher → many students
        public virtual ICollection<Student> Students { get; set; }
        public virtual ICollection<Course> Courses { get; set; }    
    }
}
