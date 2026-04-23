using Rodentia.Core.Entities;
using Rodentia.Core.Models;
using Rodentia.Core.Models.Lessons;

namespace Rodentia.Core.Interfaces;

public interface ILessonService
{
    Task<Result<IEnumerable<Lesson>>> GetScheduleAsync(Guid userId);

    Task<Result<CreateLessonModalDto>> GetCreateLessonModalDataAsync(
        Guid teacherId,
        CancellationToken cancellationToken = default);

    Task<Result> CreateLessonAsync(
        Guid teacherId,
        CreateLessonRequest request,
        CancellationToken cancellationToken = default);
    Task<Result<EditLessonModalDto>> GetEditLessonModalDataAsync(
        Guid teacherId, 
        Guid lessonId, 
        CancellationToken cancellationToken = default);
    Task<Result<StudentPaymentOverviewDto>> GetStudentPaymentOverviewAsync(
        Guid studentId,
        CancellationToken cancellationToken = default);
    Task<Result> EditLessonAsync(
        Guid teacherId, 
        EditLessonRequest request, 
        CancellationToken cancellationToken = default);
    Task<Result> DeleteLessonAsync(Guid teacherId, Guid lessonId, CancellationToken cancellationToken = default);
}