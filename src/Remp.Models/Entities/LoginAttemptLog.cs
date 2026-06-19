using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace Remp.Models.Entities;

public class LoginAttemptLog
{
    [BsonId]
    [BsonRepresentation(BsonType.ObjectId)]
    public string Id { get; set; } = ObjectId.GenerateNewId().ToString();

    public string Email { get; set; } = string.Empty;   

    public bool IsSuccess { get; set; }           

    public string? FailureReason { get; set; }             

    public string? UserId { get; set; }    
    public string? Role { get; set; }                       

    public string IpAddress { get; set; } = string.Empty;     

    public string? UserAgent { get; set; }          

    public DateTime AttemptedAt { get; set; } = DateTime.UtcNow;
}