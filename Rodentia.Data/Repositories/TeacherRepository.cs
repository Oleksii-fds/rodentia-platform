using Microsoft.EntityFrameworkCore;
using Rodentia.Core.Entities;
using Rodentia.Core.Interfaces;

namespace Rodentia.Data.Repositories;

public class TeacherRepository : ITeacherRepository
{
    private readonly RodentiaDbContext _db;

    public TeacherRepository(RodentiaDbContext db)
    {
        _db = db;
    }

    public async Task<IEnumerable<User>> GetStudentsByTeacherIdAsync(Guid teacherId)
    {
        return await _db.Set<TeacherStudentLink>()
            .Where(l => l.TeacherId == teacherId && l.IsActive)
            .Join(_db.Users, l => l.StudentId, u => u.Id, (l, u) => u)
            .ToListAsync();
    }

    public async Task<bool> LinkExistsAsync(Guid teacherId, Guid studentId)
    {
        return await _db.Set<TeacherStudentLink>()
            .AnyAsync(l => l.TeacherId == teacherId && l.StudentId == studentId && l.IsActive);
    }

    public async Task AddLinkAsync(TeacherStudentLink link)
    {
        await _db.Set<TeacherStudentLink>().AddAsync(link);
        await _db.SaveChangesAsync();
    }

    public async Task RemoveLinkAsync(Guid teacherId, Guid studentId)
    {
        var link = await _db.Set<TeacherStudentLink>()
            .FirstOrDefaultAsync(l => l.TeacherId == teacherId && l.StudentId == studentId);

        if (link != null)
        {
            _db.Set<TeacherStudentLink>().Remove(link);
            await _db.SaveChangesAsync();
        }
    }
}