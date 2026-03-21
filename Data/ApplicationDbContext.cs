using Learnova.Models.Entities;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace Learnova.Data
{
    public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        // DbSets
        public DbSet<Course> Courses { get; set; }
        public DbSet<Lesson> Lessons { get; set; }
        public DbSet<LessonAttachment> LessonAttachments { get; set; }
        public DbSet<Quiz> Quizzes { get; set; }
        public DbSet<QuizQuestion> QuizQuestions { get; set; }
        public DbSet<QuizOption> QuizOptions { get; set; }
        public DbSet<CourseEnrollment> CourseEnrollments { get; set; }
        public DbSet<LessonProgress> LessonProgresses { get; set; }
        public DbSet<QuizAttempt> QuizAttempts { get; set; }
        public DbSet<CourseReview> CourseReviews { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Course - ResponsibleUser relationship
            modelBuilder.Entity<Course>()
                .HasOne(c => c.ResponsibleUser)
                .WithMany()
                .HasForeignKey(c => c.ResponsibleUserId)
                .OnDelete(DeleteBehavior.SetNull);

            // Lesson - ResponsibleUser relationship
            modelBuilder.Entity<Lesson>()
                .HasOne(l => l.ResponsibleUser)
                .WithMany()
                .HasForeignKey(l => l.ResponsibleUserId)
                .OnDelete(DeleteBehavior.SetNull);

            // CourseEnrollment - unique constraint
            modelBuilder.Entity<CourseEnrollment>()
                .HasIndex(e => new { e.UserId, e.CourseId })
                .IsUnique();

            // LessonProgress - unique constraint
            modelBuilder.Entity<LessonProgress>()
                .HasIndex(lp => new { lp.UserId, lp.LessonId })
                .IsUnique();

            // CourseReview - unique constraint (one review per user per course)
            modelBuilder.Entity<CourseReview>()
                .HasIndex(r => new { r.UserId, r.CourseId })
                .IsUnique();

            // Decimal precision
            modelBuilder.Entity<Course>()
                .Property(c => c.Price)
                .HasPrecision(18, 2);

            modelBuilder.Entity<CourseEnrollment>()
                .Property(e => e.CompletionPercentage)
                .HasPrecision(5, 2);
        }
    }
}