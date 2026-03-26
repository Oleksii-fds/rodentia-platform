using Rodentia.Core.Entities;

namespace Rodentia.Core.Interfaces;

public interface ILessonRepository
{
    Task<IEnumerable<Lesson>> GetByUserIdAsync(Guid userId);
}