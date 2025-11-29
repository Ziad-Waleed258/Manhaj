using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Manhaj.Models
{
    public class Rating
    {
        [Key]
        public int Id { get; set; }
        [Required]
        [Range(1, 5)]
        public float Value { get; set; }
        [Required]
        public DateTime Date { get; set; }
        public string Comment { get; set; }

        public virtual ICollection<Course_Rating> Ratings { get; set; }
    }
}
