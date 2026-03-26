using Rodentia.Core.Entities;
using Rodentia.Core.Models;

namespace Rodentia.Core.Interfaces;

public interface ILessonService
{
    Task<Result<IEnumerable<Lesson>>> GetScheduleAsync(Guid userId);
}