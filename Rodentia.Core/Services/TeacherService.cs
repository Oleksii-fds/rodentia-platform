using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Rodentia.Core.Entities;
using Rodentia.Core.Interfaces;
using Rodentia.Core.Models;

namespace Rodentia.Core.Services;

public class TeacherService : ITeacherService
{
    private readonly ITeacherRepository _teacherRepository;
    private readonly UserManager<User> _userManager;

    public TeacherService(ITeacherRepository teacherRepository, UserManager<User> userManager)
    {
        _teacherRepository = teacherRepository;
        _userManager = userManager;
    }

    public async Task<IEnumerable<User>> GetMyStudentsAsync(Guid teacherId)
    {
        return await _teacherRepository.GetStudentsByTeacherIdAsync(teacherId);
    }

    public async Task<Result> AddStudentAsync(Guid teacherId, string identifier)
    {
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
}