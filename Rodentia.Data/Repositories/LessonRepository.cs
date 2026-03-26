using Microsoft.EntityFrameworkCore;
using Rodentia.Core.Entities;
using Rodentia.Core.Interfaces;

namespace Rodentia.Data.Repositories;

public class LessonRepository(RodentiaDbContext context) : ILessonRepository
{
    public async Task<IEnumerable<Lesson>> GetByUserIdAsync(Guid userId)
    {
        return await context.Lessons
            .Where(l => l.TeacherId == userId || l.StudentId == userId)
            .OrderBy(l => l.ScheduledAt)
            .ToListAsync();
    }
}