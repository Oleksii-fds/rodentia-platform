using Microsoft.AspNetCore.Identity;
using Rodentia.Core.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

public interface IProfileRepository
{
    Task<User> GetByIdAsync(Guid userId, CancellationToken cancellationToken = default);

    Task<IdentityResult> UpdateAsync(User user);

    Task<IdentityResult> ChangePasswordAsync(
        User user,
        string currentPassword,
        string newPassword);
}