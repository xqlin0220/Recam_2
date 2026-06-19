using Microsoft.Extensions.Options;
using MongoDB.Driver;
using Remp.Models.Entities;

namespace Remp.DataAccess.Collections;
public class MongoDbContext
{
    private readonly IMongoDatabase _database;
    private readonly MongoDbSettings _settings;
    
    public MongoDbContext(IOptions<MongoDbSettings> settings)
    {
        _settings = settings.Value;
        var client = new MongoClient(_settings.ConnectionString);
        _database = client.GetDatabase(_settings.DatabaseName);
    }
    public IMongoCollection<LoginAttemptLog> LoginAttemptLogs =>
        _database.GetCollection<LoginAttemptLog>(_settings.LoginAttemptLogsCollectionName);
}