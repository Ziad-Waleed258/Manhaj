using System;
using System.ComponentModel.DataAnnotations;

namespace Manhaj.Models
{
    public class Blacklist
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [EmailAddress]
        public string Email { get; set; }

        public string? Reason { get; set; }

        public DateTime AddedDate { get; set; } = DateTime.Now;
    }
}
