using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Manhaj.Models
{
    public class Question
    {
        [Key]
        public int Id { get; set; }
        [Required]
        public string Content { get; set; }
        [Required]
        public string TrueAnswer { get; set; }
        
        [Required]
        public decimal Points { get; set; } = 1;


        public virtual Quiz Quiz { get; set; }
        public int QuizId { get; set; }

        public virtual ICollection<Option> Options { get; set; } = new List<Option>();
    }
}
