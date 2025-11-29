using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Manhaj.Models
{
    public class Option
    {
        [Key]
        public int Id { get; set; }
        [Required]
        public string Content { get; set; }

        
        public virtual Question Question { get; set; }
        public int QuestionId { get; set; }
    }
}
