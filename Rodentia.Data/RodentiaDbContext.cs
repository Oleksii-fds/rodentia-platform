using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Rodentia.Core.Entities;

namespace Rodentia.Data;

public class RodentiaDbContext : IdentityDbContext<User, IdentityRole<Guid>, Guid>
{
    public RodentiaDbContext(DbContextOptions<RodentiaDbContext> options)
        : base(options)
    {
        
    }


    public DbSet<Payment> Payments { get; set; } = null!;
    public DbSet<Lesson> Lessons { get; set; } = null!;
    public DbSet<LessonRescheduleRequest> LessonRescheduleRequests { get; set; } = null!;
    public DbSet<TeacherStudentLink> TeacherStudentLinks { get; set; } = null!;
    public DbSet<ProgressNote> ProgressNotes { get; set; } = null!;
    public DbSet<Homework> Homeworks { get; set; } = null!;
    public DbSet<MaterialLink> MaterialLinks { get; set; } = null!;

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<User>().ToTable("users"); 
        modelBuilder.Entity<Lesson>().ToTable("lessons");
        modelBuilder.Entity<LessonRescheduleRequest>().ToTable("lesson_reschedule_requests");
        modelBuilder.Entity<Payment>().ToTable("payments");
        modelBuilder.Entity<TeacherStudentLink>().ToTable("teacher_student_links");
        modelBuilder.Entity<ProgressNote>().ToTable("progress_notes");
        modelBuilder.Entity<Homework>().ToTable("homeworks");
        modelBuilder.Entity<MaterialLink>().ToTable("material_links");

        modelBuilder.Entity<Lesson>()
            .Property(l => l.Price)
            .HasPrecision(18, 2);

        modelBuilder.Entity<Lesson>()
            .HasOne<User>() 
            .WithMany()
            .HasForeignKey(l => l.TeacherId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<Lesson>()
            .HasOne<User>()
            .WithMany()
            .HasForeignKey(l => l.StudentId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<LessonRescheduleRequest>()
            .HasOne<Lesson>()
            .WithMany()
            .HasForeignKey(x => x.LessonId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<TeacherStudentLink>()
            .HasOne<User>().WithMany().HasForeignKey(l => l.TeacherId).OnDelete(DeleteBehavior.Restrict);
        modelBuilder.Entity<TeacherStudentLink>()
            .HasOne<User>().WithMany().HasForeignKey(l => l.StudentId).OnDelete(DeleteBehavior.Restrict);
    }
    
}