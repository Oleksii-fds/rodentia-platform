using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Rodentia.Core.Entities;
using Rodentia.Core.Interfaces;
using Rodentia.Core.Models;
using Rodentia.Core.Models.Teacher;

namespace Rodentia.Core.Services;

public class TeacherService : ITeacherService
{
    private readonly ITeacherRepository _teacherRepository;
    private readonly UserManager<User> _userManager;
    private readonly RodentiaOptions _options;

    public TeacherService(
        ITeacherRepository teacherRepository,
        UserManager<User> userManager,
        IOptions<RodentiaOptions> options)
    {
        _teacherRepository = teacherRepository;
        _userManager = userManager;
        _options = options.Value;
    }

    public async Task<IEnumerable<User>> GetMyStudentsAsync(Guid teacherId)
    {
        return await _teacherRepository.GetStudentsByTeacherIdAsync(teacherId);
    }

    public async Task<Result> AddStudentAsync(Guid teacherId, string identifier)
    {
        if (string.IsNullOrWhiteSpace(identifier) || identifier.Trim().Length < _options.SearchMinLength)
            return Result.Failure($"Введіть щонайменше {_options.SearchMinLength} символи для пошуку.");

        var student = await _userManager.Users.FirstOrDefaultAsync(u =>
            u.Email == identifier || u.UniqueCode == identifier);

        if (student == null)
            return Result.Failure("Користувача не знайдено.");

        if (student.Role != UserRole.Student)
            return Result.Failure("Цей користувач не є учнем.");

        if (await _teacherRepository.LinkExistsAsync(teacherId, student.Id))
            return Result.Failure("Учень вже є у вашому списку.");

        var link = new TeacherStudentLink
        {
            TeacherId = teacherId,
            StudentId = student.Id,
            IsActive = true
        };

        await _teacherRepository.AddLinkAsync(link);

        return Result.Ok();
    }

    public async Task<Result> RemoveStudentAsync(Guid teacherId, Guid studentId)
    {
        await _teacherRepository.RemoveLinkAsync(teacherId, studentId);
        return Result.Ok();
    }
    public async Task<Result> ConfirmPaymentAsync(Guid teacherId, Guid lessonId)
    {
        var lesson = await _teacherRepository.GetLessonForTeacherAsync(teacherId, lessonId);

        if (lesson is null)
            return Result.Failure("Заняття не знайдено.");

        if (lesson.IsPaid)
            return Result.Failure("Заняття вже відмічено як оплачене.");

        lesson.IsPaid = true;
        await _teacherRepository.SaveChangesAsync();

        return Result.Ok();
    }

    public async Task<Result<DebtAnalysisDto>> GetDebtAnalysisAsync(Guid teacherId)
    {
        var unpaidLessons = (await _teacherRepository
            .GetUnpaidLessonsByTeacherAsync(teacherId))
            .ToList();

        if (!unpaidLessons.Any())
        {
            return Result<DebtAnalysisDto>.SuccessData(new DebtAnalysisDto
            {
                TotalDebt = 0,
                Students = []
            });
        }

        var studentIds = unpaidLessons.Select(l => l.StudentId).Distinct().ToList();
        var students = await _userManager.Users
            .Where(u => studentIds.Contains(u.Id))
            .ToDictionaryAsync(u => u.Id);

        var studentDebts = unpaidLessons
            .GroupBy(l => l.StudentId)
            .Select(g =>
            {
                students.TryGetValue(g.Key, out var student);
                return new StudentDebtDto
                {
                    StudentId = g.Key,
                    FullName = student is not null
                        ? $"{student.FirstName} {student.LastName}".Trim()
                        : "Невідомий",
                    TotalDebt = g.Sum(l => l.Price),
                    UnpaidLessonsCount = g.Count(),
                    UnpaidLessons = g.Select(l => new UnpaidLessonDto
                    {
                        LessonId = l.Id,
                        ScheduledAt = l.ScheduledAt,
                        Subject = l.Subject,
                        Topic = l.Topic,
                        Price = l.Price
                    }).ToList()
                };
            })
            .OrderByDescending(s => s.TotalDebt)
            .ToList();

        return Result<DebtAnalysisDto>.SuccessData(new DebtAnalysisDto
        {
            TotalDebt = studentDebts.Sum(s => s.TotalDebt),
            Students = studentDebts
        });
    }
}