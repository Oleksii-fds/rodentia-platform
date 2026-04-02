using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Rodentia.Core.Entities;

namespace Rodentia.Core.Models.Lessons;

public sealed class CreateLessonModalDto
{
    public List<LessonStudentOptionDto> Students { get; init; } = [];

    public DateTime DefaultDate { get; init; } = DateTime.Today;

    public int DefaultDurationMinutes { get; init; } = 60;

    public LessonStatus DefaultStatus { get; init; } = LessonStatus.Scheduled;
}
