using Rodentia.Core.Entities;
using Rodentia.Core.Interfaces;
using Rodentia.Core.Models;

namespace Rodentia.Core.Services;

public class LessonService(ILessonRepository lessonRepository) : ILessonService
{
    public async Task<Result<IEnumerable<Lesson>>> GetScheduleAsync(Guid userId)
    {
        var lessons = await lessonRepository.GetByUserIdAsync(userId);
        return Result<IEnumerable<Lesson>>.SuccessData(lessons);
    }
}