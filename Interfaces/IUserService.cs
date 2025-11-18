using ChildPsychologyAI.Models.Entities;
using ChildPsychologyAI.Models.Enums;

namespace ChildPsychologyAI.Interfaces;

public interface IUserService
{
    Task<User> CreateUserAsync(User user);
    Task<User?> GetUserByIdAsync(string userId);
    Task<User?> GetUserByEmailAsync(string email);
    Task<List<User>> GetUsersByRoleAsync(UserRole role);
    Task<bool> DeleteUserAsync(string userId);
}