using System;
using System.ComponentModel.DataAnnotations.Schema;

namespace Manhaj.Models
{
    public class Rating
    {
        public int Id { get; set; }
        public float Value { get; set; }
        public DateTime Date { get; set; }
        public string Comment { get; set; }

        // Each Rating belongs to one Course
        [ForeignKey("CourseId")]
        public Course Course { get; set; }
        public int CourseId { get; set; }

        // Each Rating belongs to one Student
        [ForeignKey("StudentId")]
        public Student Student { get; set; }
        public int StudentId { get; set; }
    }
}
