using System.ComponentModel.DataAnnotations;
using Manhaj.Models.Validation;

namespace Manhaj.Models
{
    public class User
    {
        [Key]
        public int Id { get; set; }
        [StringLength(20, ErrorMessage = "يجب ألا يتجاوز الاسم الأول 20 حرفاً")]
        [Required(ErrorMessage = "الاسم الأول مطلوب")]
        public string FirstName { get; set; }
        [StringLength(20, ErrorMessage = "يجب ألا يتجاوز الاسم الأخير 20 حرفاً")]
        [Required(ErrorMessage = "الاسم الأخير مطلوب")]
        public string LastName { get; set; }
        [RegularExpression(@"^[\w\.-]+@([\w-]+\.)+[\w-]{2,4}$", ErrorMessage = "البريد الإلكتروني غير صحيح")]
        [Required(ErrorMessage = "البريد الإلكتروني مطلوب")]
        public string Email { get; set; }
        [Required(ErrorMessage = "كلمة المرور مطلوبة")]
        [PasswordValidation(ErrorMessage = "كلمة المرور يجب أن تحتوي على حرف كبير، حرف صغير، رقم، وحرف خاص")]
        public string Password { get; set; }
        [Required(ErrorMessage = "رقم الهاتف مطلوب")]
        [RegularExpression(@"01(0|5|1|2)[0-9]{8}", ErrorMessage = "رقم الهاتف غير صحيح")]
        public string Phone { get; set; }



    }
}
