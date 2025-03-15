using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Newtonsoft.Json;

namespace BuddyUp.API.Models.DTOs
{
    public class ProfileDto
    {
        public Guid ProfileId { get; set; }
        
        public Guid UserId { get; set; }

        public string Auth0UserId { get; set; }
        
        public string FirstName { get; set; }
        
        public string LastName { get; set; }
        
        public string Email { get; set; }
        
        public string Bio { get; set; }
        
        public string ProfilePictureUrl { get; set; }

        [JsonIgnore]
       public NetTopologySuite.Geometries.Point PreferredLocation { get; set; }
        
        public double? Latitude { get; set; }
        
        public double? Longitude { get; set; }
        
        public int MaxTravelDistance { get; set; }
        
        public List<string> PreferredDays { get; set; }
        
        public List<string> PreferredTimes { get; set; }
        
        public string VerificationStatus { get; set; }
        
        public DateTime? VerificationCompletedAt { get; set; }
        
        public bool PublicProfile { get; set; }
        
        public DateTime UpdatedAt { get; set; }
    }
    
    public class ProfileUpdateDto
    {
        public string Bio { get; set; }
        
        public string ProfilePictureUrl { get; set; }
        
        public double? Latitude { get; set; }
        
        public double? Longitude { get; set; }
        
        public int? MaxTravelDistance { get; set; }
        
        public List<string> PreferredDays { get; set; }
        
        public List<string> PreferredTimes { get; set; }
        
        public bool? PublicProfile { get; set; }
    }
    
    public class UserSportDto
    {
        public Guid UserSportId { get; set; }
        
        public string UserId { get; set; }
        
        public Guid SportId { get; set; }
        
        public string SportName { get; set; }
        
        public string IconUrl { get; set; }
        
        [Required]
        public string SkillLevel { get; set; }
        
        public int? YearsExperience { get; set; }
        
        public string Notes { get; set; }
        
        public bool IsPublic { get; set; }
        
        public DateTime UpdatedAt { get; set; }
    }
    
    public class VerificationDto
    {
        public Guid VerificationId { get; set; }
        
        public string UserId { get; set; }
        
        public string VerificationType { get; set; }
        
        public string VerificationProvider { get; set; }
        
        public string Status { get; set; }
        
        public DateTime InitiatedAt { get; set; }
        
        public DateTime? CompletedAt { get; set; }
        
        public DateTime? ExpiresAt { get; set; }
    }
    
    public class VerificationRequestDto
    {
        public string UserId { get; set; }
        
        [Required]
        public string VerificationType { get; set; }
        
        [Required]
        public string VerificationProvider { get; set; }
        
        public Dictionary<string, object> VerificationData { get; set; }
    }
}