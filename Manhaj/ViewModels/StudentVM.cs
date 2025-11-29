using Manhaj.Models.Validation;
using System.ComponentModel.DataAnnotations;

namespace Manhaj.DTOs
{
    public class StudentVM
    {
        [Key]
        public int Id { get; set; }
        [StringLength(20)]
        [Required]
        public string FirstName { get; set; }
        [StringLength(20)]
        [Required]
        public string LastName { get; set; }
        [RegularExpression(@"^[\w\.-]+@([\w-]+\.)+[\w-]{2,4}$", ErrorMessage = "Invalid email format.")]
        [Required]
        public string Email { get; set; }
        [Required]
        [PasswordValidation]
        public string Password { get; set; }
        [Required]
        [RegularExpression(@"01(0|5|1|2)[0-9]{8}")]
        public string Phone { get; set; }

        [Required]
        public String Level { get; set; }

        [Required(ErrorMessage = "رقم الهاتف مطلوب")]
        [RegularExpression(@"01(0|5|1|2)[0-9]{8}", ErrorMessage = "رقم الهاتف غير صحيح")]
        public string ParentPhone { get; set; }
    }
}
