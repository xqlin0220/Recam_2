namespace Recam.Domain.Entities;

public class PhotographyCompany
{
    public string id{get;  set;} = string.Empty;
    public string PhotographyCompanyName { get; set; } = string.Empty;

    public AppUser User { get; set; } = null!; 
    public ICollection<AgentPhotographyCompany> AgentPhotographyCompanies { get; set; } = new List<AgentPhotographyCompany>();

}