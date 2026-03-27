using Rodentia.Core.Entities;
using Rodentia.Core.Models;

namespace Rodentia.Core.Interfaces;

public interface ITeacherService
{
    Task<IEnumerable<User>> GetMyStudentsAsync(Guid teacherId);
    Task<Result> AddStudentAsync(Guid teacherId, string identifier);
    Task<Result> RemoveStudentAsync(Guid teacherId, Guid studentId);
}