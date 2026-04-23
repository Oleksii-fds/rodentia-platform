namespace Rodentia.Core.Models.Lessons;

public sealed class StudentPaymentOverviewDto
{
    public decimal PaidTotal { get; init; }
    public decimal DebtTotal { get; init; }
    public List<StudentPaymentLessonDto> PaidLessons { get; init; } = [];
    public List<StudentPaymentLessonDto> DebtLessons { get; init; } = [];
}

public sealed class StudentPaymentLessonDto
{
    public Guid LessonId { get; init; }
    public DateTime ScheduledAt { get; init; }
    public string Subject { get; init; } = string.Empty;
    public string Topic { get; init; } = string.Empty;
    public decimal Price { get; init; }
    public bool IsPaid { get; init; }
}
