using System.Security.Principal;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Recam.Domain.Entities;
using Remp.Models.Entities;
using Remp.Models.Enums;

namespace Recamp.DataAccess.Data;

public class AppDbContext: IdentityDbContext<AppUser>
{
    public AppDbContext(DbContextOptions<AppDbContext> options): base(options)
    {   
    }
    public DbSet<PhotographyCompany> PhotographyCompanies {get;set;}
    public DbSet<Agent> Agents {get;set;}
    public DbSet<AgentPhotographyCompany> AgentPhotographyCompanies {get;set;}
    public DbSet<ListingCase> ListingCases {get;set;}
    public DbSet<AgentListingCase> AgentListingCases {get;set;}
    public DbSet<MediaAsset> MediaAssets {get;set;}
    public DbSet<CaseContact> CaseContacts {get;set;}

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<PhotographyCompany>(entity => {
            entity.HasKey(e => e.Id); 

                entity.Property(e => e.PhotographyCompanyName)        
                    .HasMaxLength(200);     

                entity.HasOne(e => e.AppUser)
                    .WithOne()
                    .HasForeignKey<PhotographyCompany>(e => e.Id)
                    .OnDelete(DeleteBehavior.Cascade); 
            });

        modelBuilder.Entity<Agent>(entity =>
        {
            entity.HasKey(e => e.Id);

            entity.Property(e => e.AgentFirstName).HasMaxLength(50);
            entity.Property(e => e.AgentLastName).HasMaxLength(50);
            entity.Property(e => e.AvatarUrl).HasMaxLength(500);

            entity.HasOne(e => e.User)
                .WithOne()
                .HasForeignKey<Agent>(e => e.Id)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<AgentPhotographyCompany>(entity =>
        {
            entity.HasKey(e => new {e.AgentId, e.PhotographyCompanyId});
            entity.HasOne(e => e.Agent)
                .WithMany(a => a.AgentPhotographyCompanies)
                .HasForeignKey(e => e.AgentId)
                .OnDelete(DeleteBehavior.Cascade);
            entity.HasOne(e => e.PhotographyCompany)
                .WithMany(p => p.AgentPhotographyCompanies)
                .HasForeignKey(e => e.PhotographyCompanyId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<ListingCase>(entity =>
        {
            entity.HasKey(e => e.Id);

            entity.Property(e => e.Title).HasMaxLength(100);
            entity.Property(e => e.Street).HasMaxLength(100);
            entity.Property(e => e.City).HasMaxLength(100);
            entity.Property(e => e.State).HasMaxLength(100);

            entity.Property(e => e.Longitude).HasPrecision(18, 6);
            entity.Property(e => e.Latitude).HasPrecision(18, 6);

            entity.Property(e => e.Longitude).HasPrecision(18, 6);
            entity.Property(e => e.Latitude).HasPrecision(18, 6);

            entity.Property(e => e.PropertyType).HasConversion<int>();
            entity.Property(e => e.SaleCategory).HasConversion<int>();
            entity.Property(e => e.ListCaseStatus).HasConversion<int>();
            
            entity.HasIndex(e => e.IsDeleted);

            entity.HasOne(e => e.CreatedByUser)
                .WithMany(u => u.CreatedListingCases)
                .HasForeignKey (e => e.UserId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<AgentListingCase>(entity =>
        {
            entity.HasKey(e => new {e.AgentId, e.ListingCaseId});
            
            entity.HasOne(e=> e.Agent)
                .WithMany(a => a.AgentListingCases)
                .HasForeignKey(e => e.AgentId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(e=> e.ListingCase)
                .WithMany(lc => lc.AgentListingCases)
                .HasForeignKey(e => e.ListingCaseId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<MediaAsset>(entity =>
        {
            entity.HasKey(e=> e.Id);

            entity.Property(e => e.MediaUrl)
                .HasMaxLength(500);   

            entity.Property(e => e.MediaType)
                .HasConversion <int>();  
            
            entity.Property(e => e.UploadedAt)
                .IsRequired();  

            entity.Property(e => e.IsSelect)
                .HasDefaultValue(false); 
            entity.Property(e => e.IsHero)
                .HasDefaultValue(false); 
            entity.Property(e => e.IsDeleted)
                .HasDefaultValue(false); 
            entity.HasIndex(e => e.IsDeleted);

            entity.HasOne(e=>e.ListingCase)
                .WithMany(m=>m.MediaAssets)
                .HasForeignKey(e=>e.ListingCaseId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(e=>e.UploadedByUser)
                .WithMany(e=>e.UploadedMediaAssets)
                .HasForeignKey(e=>e.UserId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<CaseContact>(entity =>
        {
            entity.HasKey(e => e.ContactId);

            entity.Property(e => e.FirstName).HasMaxLength(50);
            entity.Property(e => e.LastName).HasMaxLength(50);
            entity.Property(e => e.CompanyName).HasMaxLength(100);
            entity.Property(e => e.ProfileUrl).HasMaxLength(100);
            entity.Property(e => e.Email).HasMaxLength(100);
            entity.Property(e => e.PhoneNumber).HasMaxLength(20);

            entity.HasOne(e => e.ListingCase)
                .WithMany(lc => lc.CaseContacts)
                .HasForeignKey(e => e.ListingCaseId)
                .OnDelete(DeleteBehavior.Cascade);
        });
    }
}