using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Manhaj.Models
{
    public class Lecture
    {
        [Key]
        public int Id { get; set; }
        [Required(ErrorMessage = "عنوان المحاضرة مطلوب")]
        public string Title { get; set; }
        [Required(ErrorMessage = "رابط الفيديو مطلوب")]
        public string VideoUrl { get; set; }
        [Required(ErrorMessage = "وصف المحاضرة مطلوب")]
        public string Description { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.Now;


        public virtual Course Course { get; set; }
        public int CourseID { get; set; }

        public virtual ICollection<Material> Materials { get; set; }

        public virtual Quiz? Quiz { get; set; }

        public virtual Assignment? assignment { get; set; }


    }
}
