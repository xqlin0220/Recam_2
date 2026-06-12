namespace Recam.Domain.Entities;
public class AgentListingCase
{
    public string AgentId {get;set;} = string.Empty;
    public int ListingCaseId {get;set;}

    public ListingCase ListingCase {get;set;} = null!;
    public Agent Agent {get;set;} = null!;

}