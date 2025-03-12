using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace BuddyUp.API.Models.Domain
{
    public class Verification
    {
        [Key]
        public Guid VerificationId { get; set; }
        
        [Required]
        public Guid UserId { get; set; }
        
        [Required]
        [MaxLength(50)]
        public string VerificationType { get; set; } // Identity, Background, etc.
        
        [MaxLength(100)]
        public string VerificationProvider { get; set; } // Clear, etc.
        
        [MaxLength(255)]
        public string ProviderReferenceId { get; set; }
        
        [Required]
        [MaxLength(50)]
        public string Status { get; set; } // Pending, Completed, Failed
        
        public DateTime InitiatedAt { get; set; }
        
        public DateTime? CompletedAt { get; set; }
        
        public DateTime? ExpiresAt { get; set; }
        
        // Additional data related to verification (stored as JSON)
        public string VerificationData { get; set; }
        
        // Navigation property
        [ForeignKey("UserId")]
        public virtual User User { get; set; }
    }
}