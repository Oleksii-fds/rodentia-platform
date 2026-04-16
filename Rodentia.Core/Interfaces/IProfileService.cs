using Rodentia.Core.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Rodentia.Core.Models.Profiles;

namespace Rodentia.Core.Interfaces;

public interface IProfileService
{
    Task<Result<OwnProfileDto>> GetOwnProfileAsync(
        Guid currentUserId,
        CancellationToken cancellationToken = default);

    Task<Result> UpdateOwnProfileAsync(
        Guid currentUserId,
        UpdateOwnProfileRequest request,
        CancellationToken cancellationToken = default);

    Task<Result<StudentProfileDto>> GetStudentProfileAsync(
        Guid studentId,
        CancellationToken cancellationToken = default);
    Task<Result<TeacherProfileDto>> GetTeacherProfileAsync(
    Guid teacherId, CancellationToken cancellationToken = default);
}