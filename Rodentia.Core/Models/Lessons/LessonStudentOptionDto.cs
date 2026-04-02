using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Rodentia.Core.Models.Lessons;

public sealed class LessonStudentOptionDto
{
    public Guid StudentId { get; init; }

    public string FullName { get; init; } = string.Empty;
}