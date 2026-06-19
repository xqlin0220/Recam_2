namespace Remp.DataAccess.Collections;

public class MongoDbSettings
{
    public const string SectionName = "MongoDbSettings";

    public string ConnectionString { get; set; } = string.Empty;
    public string DatabaseName { get; set; } = string.Empty;
    public string LoginAttemptLogsCollectionName { get; set; } = "LoginAttemptLogs";
}