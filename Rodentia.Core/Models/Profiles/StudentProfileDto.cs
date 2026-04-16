using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Rodentia.Core.Models.Profiles;

public sealed class StudentProfileDto
{
    public Guid UserId { get; init; }
    public string FirstName { get; init; } = string.Empty;
    public string LastName { get; init; } = string.Empty;
    public string Email { get; init; } = string.Empty;
    public string PhoneNumber { get; init; }
    public string AvatarPath { get; init; }
    public string StudentCode { get; init; }
    public string StudentClass { get; init; }
}