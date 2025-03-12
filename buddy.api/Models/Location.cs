using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using NetTopologySuite.Geometries;

namespace BuddyUp.API.Models.Domain
{
    public class Location
    {
        [Key]
        public Guid LocationId { get; set; }
        
        [Required]
        [MaxLength(255)]
        public string Name { get; set; }
        
        public string Address { get; set; }
        
        [MaxLength(100)]
        public string City { get; set; }
        
        [MaxLength(100)]
        public string State { get; set; }
        
        [MaxLength(20)]
        public string PostalCode { get; set; }
        
        [MaxLength(100)]
        public string Country { get; set; }
        
        [Required]
        public Point Coordinates { get; set; }
        
        public bool IsVerified { get; set; } = false;
        
        public DateTime CreatedAt { get; set; }
        
        public Guid? CreatedByUserId { get; set; }
        
        public bool IsActive { get; set; } = true;
        
        // Navigation property
        [ForeignKey("CreatedByUserId")]
        public virtual User CreatedBy { get; set; }
    }
}