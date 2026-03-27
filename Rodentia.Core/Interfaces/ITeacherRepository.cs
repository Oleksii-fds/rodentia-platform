using Rodentia.Core.Entities;

namespace Rodentia.Core.Interfaces;

public interface ITeacherRepository
{
    Task<IEnumerable<User>> GetStudentsByTeacherIdAsync(Guid teacherId);
    Task<bool> LinkExistsAsync(Guid teacherId, Guid studentId);
    Task AddLinkAsync(TeacherStudentLink link);
    Task RemoveLinkAsync(Guid teacherId, Guid studentId);
}