using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace BuddyUp.API.Models.DTOs
{
    public class ConversationDto
    {
        public Guid ConversationId { get; set; }
        
        public Guid? MatchId { get; set; }
        
        public Guid OtherUserId { get; set; }
        
        public UserBriefDto OtherUser { get; set; }
        
        public string Sport { get; set; }
        
        public string LastMessage { get; set; }
        
        public DateTime? LastMessageAt { get; set; }
        
        public int UnreadCount { get; set; }
        
        public DateTime CreatedAt { get; set; }
    }
    
    public class UserBriefDto
    {
        public Guid UserId { get; set; }
        
        public string FirstName { get; set; }
        
        public string ProfilePictureUrl { get; set; }
    }
    
    public class MessageDto
    {
        public Guid MessageId { get; set; }
        
        public Guid ConversationId { get; set; }
        
        public Guid SenderId { get; set; }
        
        [Required]
        public string Content { get; set; }
        
        public DateTime SentAt { get; set; }
        
        public DateTime? ReadAt { get; set; }
    }
    
    public class SendMessageDto
    {
        [Required]
        public string Content { get; set; }
    }
    
    public class CreateConversationDto
    {
        [Required]
        public Guid UserId { get; set; }
        
        public Guid? MatchId { get; set; }
    }
    
    public class MessageListDto
    {
        public IEnumerable<MessageDto> Messages { get; set; }
        
        public int TotalCount { get; set; }
        
        public bool HasMore { get; set; }
    }
}