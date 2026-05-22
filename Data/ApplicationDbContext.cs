using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using UM_Project.Models;

namespace UM_Project.Data
{
    public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options) { }

        public DbSet<Department> Departments { get; set; }
        public DbSet<Student> Students { get; set; }
        public DbSet<Professor> Professors { get; set; }
        public DbSet<Course> Courses { get; set; }
        public DbSet<CourseDocument> CourseDocuments { get; set; }
        public DbSet<Enrollment> Enrollments { get; set; }
        public DbSet<Grade> Grades { get; set; }
        public DbSet<Schedule> Schedules { get; set; }
        public DbSet<ParentGuardian> ParentGuardians { get; set; }
        public DbSet<AuthAuditLog> AuthAuditLogs { get; set; }
        public DbSet<AdminActivityLog> AdminActivityLogs { get; set; }
        public DbSet<RefreshToken> RefreshTokens { get; set; }
        public DbSet<LessonSlot> LessonSlots { get; set; }
        public DbSet<AttendanceRecord> AttendanceRecords { get; set; }
        public DbSet<SystemSetting> SystemSettings { get; set; }
        public DbSet<AcademicTerm> AcademicTerms { get; set; }
        public DbSet<SchoolCalendarEvent> SchoolCalendarEvents { get; set; }
        public DbSet<CourseAssignment> CourseAssignments { get; set; }
        public DbSet<DocumentRequest> DocumentRequests { get; set; }
        public DbSet<InterventionNote> InterventionNotes { get; set; }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            builder.Entity<Enrollment>().HasKey(e => e.EnrollmentId);
            builder.Entity<Grade>().HasKey(g => g.GradeId);
            builder.Entity<Schedule>().HasKey(s => s.ScheduleId);
            builder.Entity<ParentGuardian>().HasKey(p => p.ParentId);
            builder.Entity<CourseDocument>().HasKey(d => d.CourseDocumentId);

            builder.Entity<CourseDocument>()
                .HasOne(d => d.Course)
                .WithMany(c => c.Documents)
                .HasForeignKey(d => d.CourseId)
                .OnDelete(DeleteBehavior.Cascade);
            builder.Entity<AuthAuditLog>().HasKey(a => a.Id);
            builder.Entity<AdminActivityLog>().HasKey(a => a.Id);
            builder.Entity<RefreshToken>().HasKey(t => t.Id);

            builder.Entity<Schedule>()
                .HasOne(s => s.Course)
                .WithMany(c => c.Schedules)
                .HasForeignKey(s => s.CourseId);

            builder.Entity<Schedule>()
                .HasIndex(s => new { s.CourseId, s.Day, s.Time })
                .IsUnique();

            builder.Entity<RefreshToken>(e =>
            {
                e.HasIndex(t => t.TokenHash).IsUnique();
                e.HasIndex(t => t.UserId);
                e.HasIndex(t => new { t.UserId, t.ExpiresAtUtc });
            });

            builder.Entity<AuthAuditLog>(e =>
            {
                e.HasIndex(a => a.CreatedAtUtc);
                e.HasIndex(a => a.EventType);
                e.HasIndex(a => a.Email);
            });

            builder.Entity<AdminActivityLog>(e =>
            {
                e.HasIndex(a => a.CreatedAtUtc);
                e.HasIndex(a => a.EntityType);
                e.HasIndex(a => a.Email);
            });

            builder.Entity<ParentGuardian>(e =>
            {
                e.HasOne(p => p.Student)
                    .WithMany()
                    .HasForeignKey(p => p.StudentId)
                    .OnDelete(DeleteBehavior.Cascade);
            });

            builder.Entity<LessonSlot>().HasKey(l => l.LessonSlotId);
            builder.Entity<LessonSlot>().HasIndex(l => l.SlotNumber).IsUnique();

            builder.Entity<SystemSetting>().HasKey(s => s.SystemSettingId);
            builder.Entity<SystemSetting>().HasIndex(s => s.SettingKey).IsUnique();

            builder.Entity<AttendanceRecord>().HasKey(a => a.AttendanceId);
            builder.Entity<AttendanceRecord>()
                .HasIndex(a => new { a.StudentId, a.CourseId, a.LessonSlotId, a.AttendanceDate })
                .IsUnique();

            builder.Entity<AttendanceRecord>()
                .HasOne(a => a.Student).WithMany().HasForeignKey(a => a.StudentId).OnDelete(DeleteBehavior.Cascade);
            builder.Entity<AttendanceRecord>()
                .HasOne(a => a.Course).WithMany().HasForeignKey(a => a.CourseId).OnDelete(DeleteBehavior.Cascade);
            builder.Entity<AttendanceRecord>()
                .HasOne(a => a.LessonSlot).WithMany().HasForeignKey(a => a.LessonSlotId).OnDelete(DeleteBehavior.Restrict);
            builder.Entity<AttendanceRecord>()
                .HasOne(a => a.Professor).WithMany().HasForeignKey(a => a.ProfessorId).OnDelete(DeleteBehavior.Restrict);

            builder.Entity<AcademicTerm>().HasKey(t => t.AcademicTermId);
            builder.Entity<SchoolCalendarEvent>().HasKey(e => e.SchoolCalendarEventId);
            builder.Entity<SchoolCalendarEvent>()
                .HasOne(e => e.AcademicTerm).WithMany(t => t.Events)
                .HasForeignKey(e => e.AcademicTermId).OnDelete(DeleteBehavior.SetNull);
            builder.Entity<SchoolCalendarEvent>().HasIndex(e => e.EventDate);

            builder.Entity<CourseAssignment>().HasKey(a => a.CourseAssignmentId);
            builder.Entity<CourseAssignment>()
                .HasOne(a => a.Course).WithMany().HasForeignKey(a => a.CourseId).OnDelete(DeleteBehavior.Cascade);

            builder.Entity<DocumentRequest>().HasKey(d => d.DocumentRequestId);
            builder.Entity<DocumentRequest>()
                .HasOne(d => d.Student).WithMany().HasForeignKey(d => d.StudentId).OnDelete(DeleteBehavior.Cascade);

            builder.Entity<InterventionNote>().HasKey(n => n.InterventionNoteId);
            builder.Entity<InterventionNote>()
                .HasOne(n => n.Student).WithMany().HasForeignKey(n => n.StudentId).OnDelete(DeleteBehavior.Cascade);
        }
    }
}
