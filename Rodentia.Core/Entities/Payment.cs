using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Rodentia.Core.Entities;

public class Payment
{
    [Key]
    public Guid Id { get; set; } = Guid.NewGuid();

    [Required]
    public Guid LessonId { get; set; } 

    [ForeignKey("LessonId")]
    public virtual Lesson Lesson { get; set; } = null!;

    [Required]
    [Column(TypeName = "decimal(12,2)")] 
    public decimal Amount { get; set; }

    [Required]
    public PaymentStatus Status { get; set; } = PaymentStatus.Pending;

    public DateTime? PaidAt { get; set; } 

    public Guid? ConfirmedByTeacherId { get; set; } 
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}