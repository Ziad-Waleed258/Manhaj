using System.Collections.Generic;

namespace Manhaj.Models
{
    public enum Level
    {
        Sec1 = 1, Sec2 = 2, Sec3 = 3
    }

    public class Student : User
    {
        public Level Level { get; set; }

        // Navigation property: one student → many teachers
        public ICollection<Teacher> Teachers { get; set; }
        // One Student → Many Ratings
        public ICollection<Rating> Ratings { get; set; }
    }
}
