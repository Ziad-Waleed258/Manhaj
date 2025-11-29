namespace Manhaj.Models
{
    public class Course_Rating
    {
        public virtual Student Student { get; set; }
        public int StudentId { get; set; }

        public virtual Course Course { get; set; }
        public int CourseID { get; set; }

        public virtual Rating Rating { get; set; }
        public int RatingID { get; set; }
    }
}
