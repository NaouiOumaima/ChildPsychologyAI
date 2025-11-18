using MongoDB.Driver;
using ChildPsychologyAI.Models.Entities;
using ChildPsychologyAI.Interfaces;
using ChildPsychologyAI.Models.Enums;

namespace ChildPsychologyAI.Services.Data;

public class ChildService : IChildService
{
    private readonly MongoDbContext _context;
    private readonly ILogger<ChildService> _logger;

    public ChildService(MongoDbContext context, ILogger<ChildService> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<Child> CreateChildAsync(Child child)
    {
        try
        {
            // Vérifier que le parent existe
            var parent = await _context.Users
                .Find(u => u.Id == child.ParentId && u.Role == UserRole.Parent)
                .FirstOrDefaultAsync();

            if (parent == null)
                throw new InvalidOperationException("Parent non trouvé");

            await _context.Children.InsertOneAsync(child);
            _logger.LogInformation("Enfant créé avec ID: {ChildId} pour parent: {ParentId}", child.Id, child.ParentId);

            return child;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erreur lors de la création de l'enfant");
            throw;
        }
    }

    public async Task<Child?> GetChildByIdAsync(string childId)
    {
        var filter = Builders<Child>.Filter.Eq(c => c.Id, childId);
        return await _context.Children.Find(filter).FirstOrDefaultAsync();
    }

    public async Task<List<Child>> GetChildrenByParentIdAsync(string parentId)
    {
        var filter = Builders<Child>.Filter.Eq(c => c.ParentId, parentId);
        return await _context.Children.Find(filter).ToListAsync();
    }

    public async Task<Child> UpdateChildAsync(Child child)
    {
        var filter = Builders<Child>.Filter.Eq(c => c.Id, child.Id);
        child.UpdatedAt = DateTime.UtcNow;

        var result = await _context.Children.ReplaceOneAsync(filter, child);

        if (result.MatchedCount == 0)
            throw new InvalidOperationException("Enfant non trouvé");

        _logger.LogInformation("Enfant mis à jour: {ChildId}", child.Id);
        return child;
    }

    public async Task<bool> DeleteChildAsync(string childId)
    {
        var filter = Builders<Child>.Filter.Eq(c => c.Id, childId);
        var result = await _context.Children.DeleteOneAsync(filter);

        return result.DeletedCount > 0;
    }

    public async Task<bool> GiveConsentAsync(string childId)
    {
        var filter = Builders<Child>.Filter.Eq(c => c.Id, childId);
        var update = Builders<Child>.Update
            .Set(c => c.ConsentGiven, true)
            .Set(c => c.ConsentDate, DateTime.UtcNow)
            .Set(c => c.UpdatedAt, DateTime.UtcNow);

        var result = await _context.Children.UpdateOneAsync(filter, update);

        if (result.ModifiedCount > 0)
        {
            _logger.LogInformation("Consentement donné pour l'enfant: {ChildId}", childId);
            return true;
        }

        return false;
    }
}