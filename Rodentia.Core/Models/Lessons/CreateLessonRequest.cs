using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Rodentia.Core.Entities;

namespace Rodentia.Core.Models.Lessons;

public sealed class CreateLessonRequest
{
    public Guid StudentId { get; init; }

    public DateTime LessonDate { get; init; }

    public TimeSpan StartTime { get; init; }

    public int DurationMinutes { get; init; } = 60;

    public string Subject { get; init; } = "Математика";

    public string Topic { get; init; }

    public decimal Price { get; init; }

    public LessonStatus Status { get; init; } = LessonStatus.Scheduled;

    public string Notes { get; init; }
}
