using Microsoft.Extensions.Options;
using MongoDB.Driver;
using ChildPsychologyAI.Models.Entities;

namespace ChildPsychologyAI.Services.Data;

public class MongoDbContext
{
    private readonly IMongoDatabase _database;

    public MongoDbContext(IOptions<MongoDbSettings> settings)
    {
        var client = new MongoClient(settings.Value.ConnectionString);
        _database = client.GetDatabase(settings.Value.DatabaseName);
    }

    public IMongoCollection<User> Users => _database.GetCollection<User>("users");
    public IMongoCollection<Child> Children => _database.GetCollection<Child>("children");
    public IMongoCollection<DrawingAnalysis> DrawingAnalyses =>
        _database.GetCollection<DrawingAnalysis>("drawingAnalyses");
}