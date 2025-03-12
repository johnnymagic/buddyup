using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace BuddyUp.API.Models.DTOs
{
    public class UserDto
    {
        public Guid UserId { get; set; }
        
        public string Auth0Id { get; set; }
        
        [Required]
        [EmailAddress]
        public string Email { get; set; }
        
        [Required]
        public string FirstName { get; set; }
        
        [Required]
        public string LastName { get; set; }
        
        public bool IsVerified { get; set; }
        
        public bool IsAdmin { get; set; }
        
        public DateTime CreatedAt { get; set; }
        
        public DateTime? LastLoginAt { get; set; }
        
        public bool Active { get; set; }
        
        // Associated profile data
        public string ProfilePictureUrl { get; set; }
        
        public string Bio { get; set; }
        
        // Optional: Include user sports
        public ICollection<UserSportDto> Sports { get; set; }
    }
    
    public class UserListItemDto
    {
        public Guid UserId { get; set; }
        
        public string FirstName { get; set; }
        
        public string LastName { get; set; }
        
        public string Email { get; set; }
        
        public bool IsVerified { get; set; }
        
        public bool IsAdmin { get; set; }
        
        public bool Active { get; set; }
        
        public DateTime CreatedAt { get; set; }
        
        public string ProfilePictureUrl { get; set; }
        
        public List<SportBriefDto> Sports { get; set; }
    }
    
    public class SportBriefDto
    {
        public Guid SportId { get; set; }
        
        public string Name { get; set; }
        
        public string SkillLevel { get; set; }
    }
    
    public class UserSearchDto
    {
        public string Search { get; set; }
        
        public string Status { get; set; } // active, inactive
        
        public string IsVerified { get; set; } // true, false
        
        public string Sport { get; set; } // sport id or name
        
        public int Page { get; set; } = 1;
        
        public int PageSize { get; set; } = 10;
    }
    
    public class UserStatusUpdateDto
    {
        [Required]
        public bool Active { get; set; }
    }
    
    public class UserAdminUpdateDto
    {
        public bool? IsAdmin { get; set; }
        
        public bool? IsVerified { get; set; }
        
        public bool? Active { get; set; }
    }
}