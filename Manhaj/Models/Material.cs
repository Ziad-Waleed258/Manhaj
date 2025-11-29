using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Manhaj.Models
{
    public class Material
    {
        [Key]
        public int Id { get; set; }
        [Required]
        public string Filetype { get; set; }
        [Required]
        public string Title { get; set; }
        [Required]
        public string FilePath { get; set; }

        public virtual Lecture Lecture { get; set; }
        public int LectureID { get; set; }
    }
}
