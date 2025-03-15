using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace BuddyUp.API.Models.DTOs
{
    /// <summary>
    /// DTO for profile creation and update operations
    /// This is specifically designed to be JSON-friendly with no complex types
    /// </summary>
    public class ProfileUpdateInputDto
    {
        // No complex types or spatial data here
        public string Auth0UserId { get; set; }
        
        public string FirstName { get; set; }
        
        public string LastName { get; set; }
        
        public string Email { get; set; }
        
        public string Bio { get; set; }
        
        public string ProfilePictureUrl { get; set; }
        
        // Using nullable types for optional fields
        public double? Latitude { get; set; }
        
        public double? Longitude { get; set; }
        
        public int MaxTravelDistance { get; set; } = 20; // Default value
        
        public List<string> PreferredDays { get; set; }
        
        public List<string> PreferredTimes { get; set; }
        
        public string VerificationStatus { get; set; }
        
        public bool PublicProfile { get; set; } = true; // Default value
    }
}