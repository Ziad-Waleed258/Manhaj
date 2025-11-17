using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.VisualBasic;

namespace Manhaj.Models
{
    public class Assignment
    {
        public int Id { get; set; }

        public string Title { get; set; }

        public string Description { get; set; }

        public List<string> Media { get; set; }

        public string Comment { get; set; }

        public DateTime Deadline { get; set; }

        [ForeignKey("lectureID")]
        public Lecture Lecture {  get; set; } 
        public int lectureID { get; set; }
    }
}
