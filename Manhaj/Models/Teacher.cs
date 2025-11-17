using System.Collections.Generic;

namespace Manhaj.Models
{
    public class Teacher : User
    {
        public string Specialization { get; set; }

        // Navigation property: one teacher → many students
        public ICollection<Student> Students { get; set; }
    }
}
