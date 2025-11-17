using System.ComponentModel.DataAnnotations.Schema;

namespace Manhaj.Models
{
    public class Material
    {
        public int Id { get; set; }

        public string Filetype { get; set; }

        public string Title { get; set; }

        public string FilePath { get; set; }

        [ForeignKey("LectureID")]
        public Lecture Lecture { get; set; }

        public int LectureID { get; set; }
    }
}
