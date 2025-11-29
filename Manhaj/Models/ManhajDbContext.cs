using Microsoft.EntityFrameworkCore;

namespace Manhaj.Models
{
    public class ManhajDbContext : DbContext
    {
        public virtual DbSet<User> Users { get; set; }
        public virtual DbSet<Admin> Admins { get; set; }
        public virtual DbSet<Student> Students { get; set; }
        public virtual DbSet<Teacher> Teachers { get; set; }
        public virtual DbSet<Course> Courses { get; set; }
        public virtual DbSet<Course_Registration> Course_Registrations { get; set; }
        public virtual DbSet<Course_Rating> Course_Ratings { get; set; } 
        public virtual DbSet<Lecture> Lectures { get; set; }
        public virtual DbSet<Material> Materials { get; set; }
        public virtual DbSet<Option> Options { get; set; }
        public virtual DbSet<Question> Questions { get; set; }
        public virtual DbSet<Quiz> Quizzes { get; set; }
        public virtual DbSet<Rating> Ratings { get; set; }
        public virtual DbSet<Assignment> Assignments { get; set; }
        public virtual DbSet<Student_Assignment> Student_Assignments { get; set; }
        public virtual DbSet<Student_Quiz> Student_Quizzes { get; set; }
        public virtual DbSet<Student_Lecture> Student_Lectures { get; set; }
        public virtual DbSet<Blacklist> Blacklist { get; set; }
        public virtual DbSet<Student_Quiz_Answer> Student_Quiz_Answers { get; set; }


        public ManhajDbContext(DbContextOptions<ManhajDbContext> options)
            : base(options)
        {
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // =======================
            // Teacher → Courses (1:N)
            // =======================
            modelBuilder.Entity<Course>()
                .HasOne(c => c.Teacher)
                .WithMany(t => t.Courses)
                .HasForeignKey(c => c.TeacherID)
                .OnDelete(DeleteBehavior.Cascade);

            // =======================
            // Teacher ↔ Student (many-to-many)
            // =======================
            modelBuilder.Entity<Student>()
                .HasMany(s => s.Teachers)
                .WithMany(t => t.Students)
                .UsingEntity(j => j.ToTable("StudentTeachers"));

            // =======================
            // Course → Lectures (1:N)
            // =======================
            modelBuilder.Entity<Lecture>()
                .HasOne(l => l.Course)
                .WithMany(c => c.Lectures)
                .HasForeignKey(l => l.CourseID)
                .OnDelete(DeleteBehavior.Cascade);

            // Lecture → Materials (1:N)
            modelBuilder.Entity<Material>()
                .HasOne(m => m.Lecture)
                .WithMany(l => l.Materials)
                .HasForeignKey(m => m.LectureID)
                .OnDelete(DeleteBehavior.Cascade);

            // Lecture → Quiz (1:1, optional)
            modelBuilder.Entity<Quiz>()
                .HasOne(q => q.Lecture)
                .WithOne(l => l.Quiz)
                .HasForeignKey<Quiz>(q => q.LectureId)
                .IsRequired(false)
                .OnDelete(DeleteBehavior.Restrict);

            // Lecture → Assignment (1:1, optional from Lecture side)
            modelBuilder.Entity<Assignment>()
                .HasOne(a => a.Lecture)
                .WithOne(l => l.assignment)
                .HasForeignKey<Assignment>(a => a.lectureID)
                .IsRequired()
                .OnDelete(DeleteBehavior.Restrict);

            // =======================
            // Quiz → Questions (1:N)
            // =======================
            modelBuilder.Entity<Question>()
                .HasOne(q => q.Quiz)
                .WithMany(qz => qz.Questions)
                .HasForeignKey(q => q.QuizId)
                .OnDelete(DeleteBehavior.Cascade);

            // Question → Options (1:N)
            modelBuilder.Entity<Option>()
                .HasOne(o => o.Question)
                .WithMany(q => q.Options)
                .HasForeignKey(o => o.QuestionId)
                .OnDelete(DeleteBehavior.Cascade);

            // =======================
            // Student ↔ Quiz (Student_Quiz) (composite key)
            // =======================
            modelBuilder.Entity<Student_Quiz>()
                .HasKey(sq => new { sq.StudentId, sq.QuizId });

            modelBuilder.Entity<Student_Quiz>()
                .HasOne(sq => sq.Student)
                .WithMany(s => s.Quizzes)
                .HasForeignKey(sq => sq.StudentId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Student_Quiz>()
                .HasOne(sq => sq.Quiz)
                .WithMany(q => q.Quizzes)
                .HasForeignKey(sq => sq.QuizId)
                .OnDelete(DeleteBehavior.Restrict);

            // =======================
            // Student ↔ Assignment (Student_Assignment) (composite key)
            // =======================
            modelBuilder.Entity<Student_Assignment>()
                .HasKey(sa => new { sa.StudentId, sa.AssignmentId });

            modelBuilder.Entity<Student_Assignment>()
                .HasOne(sa => sa.Student)
                .WithMany(s => s.Assignments)
                .HasForeignKey(sa => sa.StudentId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Student_Assignment>()
                .HasOne(sa => sa.Assignment)
                .WithMany(a => a.Assignments)
                .HasForeignKey(sa => sa.AssignmentId)
                .OnDelete(DeleteBehavior.Restrict);

            // =======================
            // Student ↔ Course (Course_Registration) (composite key)
            // =======================
            modelBuilder.Entity<Course_Registration>()
                .HasKey(cr => new { cr.StudentID, cr.CourseID });

            modelBuilder.Entity<Course_Registration>()
                .HasOne(cr => cr.Student)
                .WithMany(s => s.Course_Registrations)
                .HasForeignKey(cr => cr.StudentID)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Course_Registration>()
                .HasOne(cr => cr.Course)
                .WithMany(c => c.Course_Registrations)
                .HasForeignKey(cr => cr.CourseID)
                .OnDelete(DeleteBehavior.Restrict);

            // =======================
            // Course_Rating (ternary: Student ↔ Course ↔ Rating)
            // =======================
            modelBuilder.Entity<Course_Rating>()
                .HasKey(cr => new { cr.StudentId, cr.CourseID, cr.RatingID });

            modelBuilder.Entity<Course_Rating>()
                .HasOne(cr => cr.Student)
                .WithMany(s => s.Ratings)
                .HasForeignKey(cr => cr.StudentId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Course_Rating>()
                .HasOne(cr => cr.Course)
                .WithMany(c => c.Ratings)
                .HasForeignKey(cr => cr.CourseID)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Course_Rating>()
                .HasOne(cr => cr.Rating)
                .WithMany(r => r.Ratings)
                .HasForeignKey(cr => cr.RatingID)
                .OnDelete(DeleteBehavior.Restrict);

            // =======================
            // Student ↔ Lecture (Student_Lecture) (Progress Tracking)
            // =======================
            modelBuilder.Entity<Student_Lecture>()
                .HasKey(sl => sl.Id); // Or composite key { StudentId, LectureId } if preferred

            modelBuilder.Entity<Student_Lecture>()
                .HasOne(sl => sl.Student)
                .WithMany() // Assuming no collection on Student for now, or add if needed
                .HasForeignKey(sl => sl.StudentId)
                .OnDelete(DeleteBehavior.Restrict); // Changed to Restrict to avoid cycles

            modelBuilder.Entity<Student_Lecture>()
                .HasOne(sl => sl.Lecture)
                .WithMany() // Assuming no collection on Lecture for now
                .HasForeignKey(sl => sl.LectureId)
                .OnDelete(DeleteBehavior.Restrict); // Changed to Restrict to avoid cycles

            // =======================
            // Student_Quiz_Answer
            // =======================
            modelBuilder.Entity<Student_Quiz_Answer>()
                .HasOne(sqa => sqa.Student)
                .WithMany()
                .HasForeignKey(sqa => sqa.StudentId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Student_Quiz_Answer>()
                .HasOne(sqa => sqa.Quiz)
                .WithMany()
                .HasForeignKey(sqa => sqa.QuizId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Student_Quiz_Answer>()
                .HasOne(sqa => sqa.Question)
                .WithMany()
                .HasForeignKey(sqa => sqa.QuestionId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Student_Quiz_Answer>()
                .HasOne(sqa => sqa.SelectedOption)
                .WithMany()
                .HasForeignKey(sqa => sqa.SelectedOptionId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Student_Quiz>()
                .HasMany(sq => sq.Answers)
                .WithOne()
                .HasForeignKey(sqa => new { sqa.StudentId, sqa.QuizId });
        }

    }
}