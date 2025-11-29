using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Manhaj.ViewModels
{
    public class QuizWizardVM
    {
        [Required(ErrorMessage = "عنوان الاختبار مطلوب")]
        public string Title { get; set; }

        public int LectureId { get; set; }

        [Required(ErrorMessage = "وقت البدء مطلوب")]
        public DateTime StartTime { get; set; }

        [Required(ErrorMessage = "وقت الانتهاء مطلوب")]
        public DateTime EndTime { get; set; }

        [Required(ErrorMessage = "مدة الاختبار مطلوبة")]
        [Range(1, 1000, ErrorMessage = "المدة يجب أن تكون بين 1 و 1000 دقيقة")]
        public int Duration { get; set; }

        public List<QuestionInputVM> Questions { get; set; } = new List<QuestionInputVM>();
    }

    public class QuestionInputVM
    {
        [Required(ErrorMessage = "نص السؤال مطلوب")]
        public string Content { get; set; }

        [Required(ErrorMessage = "الإجابة الصحيحة مطلوبة")]
        public string TrueAnswer { get; set; }

        [Required(ErrorMessage = "النقاط مطلوبة")]
        [Range(0.5, 100, ErrorMessage = "النقاط يجب أن تكون أكبر من 0")]
        public decimal Points { get; set; }

        [Required(ErrorMessage = "الخيارات مطلوبة")]
        [MinLength(2, ErrorMessage = "يجب إضافة خيارين على الأقل")]
        public List<string> Options { get; set; } = new List<string>();
    }
}
