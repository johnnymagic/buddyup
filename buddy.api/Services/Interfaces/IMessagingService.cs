using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using BuddyUp.API.Models.DTOs;

namespace BuddyUp.API.Services.Interfaces
{
    public interface IMessagingService
    {
        /// <summary>
        /// Get all conversations for a user
        /// </summary>
        Task<IEnumerable<ConversationDto>> GetConversations(string userId);
        
        /// <summary>
        /// Get a specific conversation by ID
        /// </summary>
        Task<ConversationDto> GetConversation(Guid conversationId, string userId);
        
        /// <summary>
        /// Get messages for a specific conversation
        /// </summary>
        Task<MessageListDto> GetMessages(Guid conversationId, string userId, int page = 1, int pageSize = 20);
        
        /// <summary>
        /// Send a message in a conversation
        /// </summary>
        Task<MessageDto> SendMessage(Guid conversationId, string userId, SendMessageDto messageDto);
        
        /// <summary>
        /// Mark messages in a conversation as read
        /// </summary>
        Task MarkMessagesAsRead(Guid conversationId, string userId);
        
        /// <summary>
        /// Create a new conversation between users
        /// </summary>
        Task<ConversationDto> CreateConversation(string userId, CreateConversationDto conversationDto);
        
        /// <summary>
        /// Get conversation between two users if it exists
        /// </summary>
        Task<ConversationDto> GetConversationWithUser(string userId, string otherUserId);
        
        /// <summary>
        /// Get conversation for a specific match if it exists
        /// </summary>
        Task<ConversationDto> GetConversationForMatch(Guid matchId, string userId);
        
        /// <summary>
        /// Get unread message count
        /// </summary>
        Task<int> GetUnreadMessageCount(string userId);
    }
}