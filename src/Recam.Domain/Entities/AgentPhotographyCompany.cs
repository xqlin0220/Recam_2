namespace Recam.Domain.Entities;
public class AgentPhotographyCompany
{
    public string AgentId {get;set;} = string.Empty;
    public string UserId {get;set;} = string.Empty;

    public Agent Agent {get;set;} = null!;
    public PhotographyCompany PhotographyCompany {get;set;} = null!;

}