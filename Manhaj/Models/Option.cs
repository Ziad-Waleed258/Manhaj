using System.ComponentModel.DataAnnotations.Schema;

namespace Manhaj.Models
{
    public class Option
    {
        public int Id { get; set; }
        public string Content { get; set; }

        [ForeignKey("QuestionId")]
        public Question Question { get; set; }
        public int QuestionId { get; set; }
    }
}
