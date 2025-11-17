using System.ComponentModel.DataAnnotations.Schema;

namespace Manhaj.Models
{
    public class Lecture
    {
        public int Id { get; set; }

        public string Title { get; set; }

        public string VideoUrl { get; set; }

        public string Description { get; set; }

        [ForeignKey("CourseID")]
        public Course Course { get; set; }
        public int CourseID { get; set; }

        public ICollection<Material> Materials { get; set; }


    }
}
