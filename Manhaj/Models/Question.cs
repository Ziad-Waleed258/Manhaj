using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;

namespace Manhaj.Models
{
    public class Question
    {
        public int Id { get; set; }
        public string Content { get; set; }
        public string TrueAnswer { get; set; }

        [ForeignKey("QuizId")]
        public Quiz Quiz { get; set; }
        public int QuizId { get; set; }

        public ICollection<Option> Options { get; set; } = new List<Option>();
    }
}
