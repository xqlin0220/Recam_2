namespace Remp.Models.Entities;
public class Agent
{
    public string Id {get;set;}= string.Empty;
    public string AgentFirstName {get;set;}= string.Empty;
    public string AgentLastName {get;set;}= string.Empty;
    public string? AvatarUrl {get;set;}
    public string? CompanyName {get;set;}

    public AppUser User{get;set;} = null!;
    public ICollection<AgentPhotographyCompany> AgentPhotographyCompanies {get;set;} = new List<AgentPhotographyCompany>();
    public ICollection<AgentListingCase> AgentListingCases {get;set;} = new List<AgentListingCase>();
}