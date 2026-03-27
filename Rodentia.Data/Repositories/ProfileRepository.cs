using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Rodentia.Core.Entities;
using Rodentia.Core.Interfaces;
using Rodentia.Data;
namespace Rodentia.Data.Repositories;

public sealed class ProfileRepository : IProfileRepository
{
    private readonly RodentiaDbContext _dbContext;
    private readonly UserManager<User> _userManager;

    public ProfileRepository(
        RodentiaDbContext dbContext,
        UserManager<User> userManager)
    {
        _dbContext = dbContext;
        _userManager = userManager;
    }

    public async Task<User> GetByIdAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        return await _dbContext.Users
            .FirstOrDefaultAsync(x => x.Id == userId, cancellationToken);
    }

    public Task<IdentityResult> UpdateAsync(User user)
    {
        return _userManager.UpdateAsync(user);
    }

    public Task<IdentityResult> ChangePasswordAsync(
        User user,
        string currentPassword,
        string newPassword)
    {
        return _userManager.ChangePasswordAsync(user, currentPassword, newPassword);
    }
}