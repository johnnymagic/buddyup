using System;
using System.ComponentModel.DataAnnotations;

namespace BuddyUp.API.Models.DTOs
{
    public class MatchDto
    {
        public Guid MatchId { get; set; }
        
        public Guid RequesterId { get; set; }
        
        public string RequesterFirstName { get; set; }
        
        public string RequesterProfilePictureUrl { get; set; }
        
        public Guid RecipientId { get; set; }
        
        public string RecipientFirstName { get; set; }
        
        public string RecipientProfilePictureUrl { get; set; }
        
        public Guid? SportId { get; set; }
        
        public string SportName { get; set; }
        
        public string RequesterSkillLevel { get; set; }
        
        public string RecipientSkillLevel { get; set; }
        
        public string Status { get; set; }
        
        public string Message { get; set; }
        
        public DateTime RequestedAt { get; set; }
        
        public DateTime? RespondedAt { get; set; }
        
        public double? Distance { get; set; }
    }
    
    public class MatchRequestDto
    {
        [Required]
        public Guid RecipientId { get; set; }
        
        [Required]
        public Guid SportId { get; set; }
        
        public string Message { get; set; }
    }
    
    public class MatchResponseDto
    {
        [Required]
        public bool Accept { get; set; }
        
        public string Message { get; set; }
    }
    
    public class PotentialMatchDto
    {
        public Guid UserId { get; set; }
        
        public string FirstName { get; set; }
        
        public string ProfilePictureUrl { get; set; }
        
        public string Bio { get; set; }
        
        public Guid SportId { get; set; }
        
        public string Sport { get; set; }
        
        public string SkillLevel { get; set; }
        
        public bool Verified { get; set; }
        
        public double Distance { get; set; }
        
        public List<string> PreferredDays { get; set; }
        
        public List<string> PreferredTimes { get; set; }
    }
    
    public class MatchFilterDto
    {
        public Guid? SportId { get; set; }
        
        public string SkillLevel { get; set; }
        
        public int Distance { get; set; } = 50; // Default to 50km
        
        public List<string> Days { get; set; }
        
        public List<string> Times { get; set; }
    }
}