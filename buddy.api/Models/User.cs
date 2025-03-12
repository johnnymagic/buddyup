using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace BuddyUp.API.Models.Domain
{
    public class User
    {
        [Key]
        public Guid UserId { get; set; }
        
        [Required]
        [MaxLength(128)]
        public string Auth0Id { get; set; }
        
        [Required]
        [EmailAddress]
        [MaxLength(255)]
        public string Email { get; set; }
        
        [Required]
        [MaxLength(100)]
        public string FirstName { get; set; }
        
        [Required]
        [MaxLength(100)]
        public string LastName { get; set; }
        
        public bool IsVerified { get; set; }
        
        public bool IsAdmin { get; set; }
        
        public DateTime CreatedAt { get; set; }
        
        public DateTime? LastLoginAt { get; set; }
        
        public bool Active { get; set; } = true;
        
        // Navigation properties
        public virtual UserProfile Profile { get; set; }
        
        public virtual ICollection<UserSport> Sports { get; set; }
        
        public virtual ICollection<Match> SentMatchRequests { get; set; }
        
        public virtual ICollection<Match> ReceivedMatchRequests { get; set; }
        
        public virtual ICollection<Message> SentMessages { get; set; }
        
        public virtual ICollection<UserReport> ReportsSubmitted { get; set; }
        
        public virtual ICollection<UserReport> ReportsReceived { get; set; }
    }
}