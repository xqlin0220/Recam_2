using Microsoft.AspNetCore.Identity; 

namespace Recam.Domain.Entities;

public class AppUser: IdentityUser
{
    public bool IsDeleted {get;set;} = false;
    public DateTime CreatedAt{get; set;} = DateTime.UtcNow;

    public Agent? Agent { get; set; }
    public PhotographyCompany? PhotographyCompany { get; set; }

    public ICollection<ListingCase> CreatedListingCases { get; set; } = new List<ListingCase>();
    public ICollection<MediaAsset> UploadedMediaAssets { get; set; } = new List<MediaAsset>();

}   