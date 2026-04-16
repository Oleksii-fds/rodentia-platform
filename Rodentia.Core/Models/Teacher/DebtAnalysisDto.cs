using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Rodentia.Core.Models.Teacher;

public sealed class DebtAnalysisDto
{
    public decimal TotalDebt { get; init; }
    public List<StudentDebtDto> Students { get; init; } = [];
}

public sealed class StudentDebtDto
{
    public Guid StudentId { get; init; }
    public string FullName { get; init; } = string.Empty;
    public decimal TotalDebt { get; init; }
    public int UnpaidLessonsCount { get; init; }
    public List<UnpaidLessonDto> UnpaidLessons { get; init; } = [];
}

public sealed class UnpaidLessonDto
{
    public Guid LessonId { get; init; }
    public DateTime ScheduledAt { get; init; }
    public string Subject { get; init; } = string.Empty;
    public string Topic { get; init; }
    public decimal Price { get; init; }
}
