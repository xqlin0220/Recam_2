namespace Recam.Domain.Entities;

public class CaseContact
{
    public int ContactId {get;set;}
    public string FirstName {get;set;} = string.Empty;
    public string LastName {get;set;} = string.Empty;
    public string CompanyName {get;set;} = string.Empty;
    public string ProfileUrl {get;set;} = string.Empty;
    public string Email {get;set;} = string.Empty;
    public string PhoneNumber {get;set;} = string.Empty;
    public int ListingCaseId {get;set;}

    public ListingCase ListingCase {get;set;} = null!;

}