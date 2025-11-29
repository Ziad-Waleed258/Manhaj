using System.ComponentModel.DataAnnotations;

namespace Manhaj.DTOs
{
    public class AssignmentVM
    {
        [Key]
        public int Id { get; set; }
        [Required]
        public string Title { get; set; }
        [Required]
        public string Description { get; set; }

        public List<string> Media { get; set; }

        public string Comment { get; set; }
        [Required]
        public DateTime Deadline { get; set; }
    }
}
