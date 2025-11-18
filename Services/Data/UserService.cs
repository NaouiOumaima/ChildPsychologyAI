using MongoDB.Driver;
using ChildPsychologyAI.Models.Entities;
using ChildPsychologyAI.Interfaces;
using ChildPsychologyAI.Models.Enums;

namespace ChildPsychologyAI.Services.Data;

public class UserService : IUserService
{
    private readonly MongoDbContext _context;
    private readonly ILogger<UserService> _logger;

    public UserService(MongoDbContext context, ILogger<UserService> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<User> CreateUserAsync(User user)
    {
        try
        {
            // Vérifier si l'email existe déjà
            var existingUser = await _context.Users
                .Find(u => u.Email == user.Email)
                .FirstOrDefaultAsync();

            if (existingUser != null)
                throw new InvalidOperationException("Un utilisateur avec cet email existe déjà");

            await _context.Users.InsertOneAsync(user);
            _logger.LogInformation("Utilisateur créé avec ID: {UserId}", user.Id);

            return user;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erreur lors de la création de l'utilisateur");
            throw;
        }
    }

    public async Task<User?> GetUserByIdAsync(string userId)
    {
        var filter = Builders<User>.Filter.Eq(u => u.Id, userId);
        return await _context.Users.Find(filter).FirstOrDefaultAsync();
    }

    public async Task<User?> GetUserByEmailAsync(string email)
    {
        var filter = Builders<User>.Filter.Eq(u => u.Email, email);
        return await _context.Users.Find(filter).FirstOrDefaultAsync();
    }

    public async Task<List<User>> GetUsersByRoleAsync(UserRole role)
    {
        var filter = Builders<User>.Filter.Eq(u => u.Role, role);
        return await _context.Users.Find(filter).ToListAsync();
    }

    public async Task<bool> DeleteUserAsync(string userId)
    {
        var filter = Builders<User>.Filter.Eq(u => u.Id, userId);
        var result = await _context.Users.DeleteOneAsync(filter);

        return result.DeletedCount > 0;
    }
}