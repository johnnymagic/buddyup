using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using NetTopologySuite.Geometries;

namespace BuddyUp.API.Models.Domain
{
    public class UserProfile
    {
        [Key]
        public Guid ProfileId { get; set; }
        
        [Required]
        public Guid UserId { get; set; }
        
        public string Bio { get; set; }
        
        [MaxLength(255)]
        public string ProfilePictureUrl { get; set; }
        
        public Point PreferredLocation { get; set; }
        
        public int MaxTravelDistance { get; set; } = 20; // Default to 20 km
        
        // Store as comma-separated values or JSON array
        [MaxLength(100)]
        public List<string> PreferredDays { get; set; }
        
        // Store as comma-separated values or JSON array
        [MaxLength(100)]
        public List<string> PreferredTimes { get; set; }
        
        [MaxLength(50)]
        public string VerificationStatus { get; set; } = "Unverified";
        
        public DateTime? VerificationCompletedAt { get; set; }
        
        public bool PublicProfile { get; set; } = true;
        
        public DateTime CreatedAt { get; set; }
        
        public DateTime UpdatedAt { get; set; }
        
        // Navigation property
        [ForeignKey("UserId")]
        public virtual User User { get; set; }
    }
}