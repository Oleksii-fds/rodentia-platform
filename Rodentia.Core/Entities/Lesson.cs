using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Rodentia.Core.Entities;

public class Lesson
{
    [Key]
    public Guid Id { get; set; } = Guid.NewGuid();

    [Required]
    public Guid TeacherId { get; set; }
    
    [ForeignKey("TeacherId")]
    public virtual User Teacher { get; set; } = null!; 

    [Required]
    public Guid StudentId { get; set; }
    
    [ForeignKey("StudentId")]
    public virtual User Student { get; set; } = null!; 

    [Required]
    [MaxLength(100)]
    public string Subject { get; set; } = "Математика";

    [Required]
    public DateTime ScheduledAt { get; set; }

    public int DurationMinutes { get; set; } = 60;

    public bool IsCompleted { get; set; } = false;

    public string? Notes { get; set; }
}