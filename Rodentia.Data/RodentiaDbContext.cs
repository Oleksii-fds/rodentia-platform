using Microsoft.EntityFrameworkCore;
using Rodentia.Core.Entities;

namespace Rodentia.Data;

public class RodentiaDbContext : DbContext
{
    public RodentiaDbContext(DbContextOptions<RodentiaDbContext> options)
        : base(options)
    {
    }

    public DbSet<User> Users { get; set; } = null!;
    public DbSet<Payment> Payments { get; set; } = null!;
    public DbSet<Lesson> Lessons { get; set; } = null!;
    public DbSet<TeacherStudentLink> TeacherStudentLinks { get; set; } = null!;
    public DbSet<ProgressNote> ProgressNotes { get; set; } = null!;
    public DbSet<Homework> Homeworks { get; set; } = null!;
    public DbSet<MaterialLink> MaterialLinks { get; set; } = null!;

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<User>().HasIndex(u => u.Email).IsUnique();

        modelBuilder.Entity<Lesson>()
            .HasOne(l => l.Teacher)
            .WithMany()
            .HasForeignKey(l => l.TeacherId)
            .OnDelete(DeleteBehavior.Restrict); 

        modelBuilder.Entity<Lesson>()
            .HasOne(l => l.Student)
            .WithMany()
            .HasForeignKey(l => l.StudentId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<TeacherStudentLink>()
            .HasOne<User>().WithMany().HasForeignKey(l => l.TeacherId).OnDelete(DeleteBehavior.Restrict);
        modelBuilder.Entity<TeacherStudentLink>()
            .HasOne<User>().WithMany().HasForeignKey(l => l.StudentId).OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<ProgressNote>()
            .HasOne<Lesson>().WithMany().HasForeignKey(n => n.LessonId).OnDelete(DeleteBehavior.Cascade);
        modelBuilder.Entity<ProgressNote>()
            .HasOne<User>().WithMany().HasForeignKey(n => n.TeacherId).OnDelete(DeleteBehavior.Restrict);
                
        modelBuilder.Entity<Lesson>().ToTable("lessons");
        modelBuilder.Entity<Payment>().ToTable("payments");
        modelBuilder.Entity<User>().ToTable("users");
        modelBuilder.Entity<TeacherStudentLink>().ToTable("teacher_student_links");
        modelBuilder.Entity<ProgressNote>().ToTable("progress_notes");
        modelBuilder.Entity<Homework>().ToTable("homeworks");
        modelBuilder.Entity<MaterialLink>().ToTable("material_links");
    }
}