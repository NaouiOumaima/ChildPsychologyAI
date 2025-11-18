using ChildPsychologyAI.Models.Enums;

namespace ChildPsychologyAI.Models.Entities;

public class User : EntityBase
{
    public string Email { get; set; } = string.Empty;
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public UserRole Role { get; set; }
    public string PasswordHash { get; set; } = string.Empty;
}