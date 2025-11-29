using System;

namespace Manhaj.ViewModels
{
    public class ActivityVM
    {
        public string Type { get; set; } // "Lecture", "Quiz", "Assignment"
        public string Title { get; set; }
        public string CourseTitle { get; set; }
        public DateTime Date { get; set; } // CreatedAt for Lecture, Deadline for others
        public string Link { get; set; }
        public string LinkText { get; set; }
    }
}
