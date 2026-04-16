using Rodentia.Core.Entities;
using Rodentia.Core.Models;
using Rodentia.Core.Models.Teacher;

namespace Rodentia.Core.Interfaces;

public interface ITeacherService
{
    Task<IEnumerable<User>> GetMyStudentsAsync(Guid teacherId);
    Task<Result> AddStudentAsync(Guid teacherId, string identifier);
    Task<Result> RemoveStudentAsync(Guid teacherId, Guid studentId);
    Task<Result> ConfirmPaymentAsync(Guid teacherId, Guid lessonId);
    Task<Result<DebtAnalysisDto>> GetDebtAnalysisAsync(Guid teacherId);
}