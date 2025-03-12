using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using BuddyUp.API.Data.Repositories;
using BuddyUp.API.Models.Domain;
using BuddyUp.API.Models.DTOs;
using BuddyUp.API.Services.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace BuddyUp.API.Services.Implementations
{
    public class MessagingService : IMessagingService
    {
        private readonly IRepository<User> _userRepository;
        private readonly IRepository<Conversation> _conversationRepository;
        private readonly IRepository<Message> _messageRepository;
        private readonly IRepository<Match> _matchRepository;
        private readonly ILogger<MessagingService> _logger;
        private readonly IMapper _mapper;

        public MessagingService(
            IRepository<User> userRepository,
            IRepository<Conversation> conversationRepository,
            IRepository<Message> messageRepository,
            IRepository<Match> matchRepository,
            ILogger<MessagingService> logger,
            IMapper mapper)
        {
            _userRepository = userRepository ?? throw new ArgumentNullException(nameof(userRepository));
            _conversationRepository = conversationRepository ?? throw new ArgumentNullException(nameof(conversationRepository));
            _messageRepository = messageRepository ?? throw new ArgumentNullException(nameof(messageRepository));
            _matchRepository = matchRepository ?? throw new ArgumentNullException(nameof(matchRepository));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
        }

        public async Task<IEnumerable<ConversationDto>> GetConversations(string userId)
        {
            try
            {
                var user = await _userRepository.Query()
                    .FirstOrDefaultAsync(u => u.Auth0Id == userId);

                if (user == null)
                {
                    throw new KeyNotFoundException($"User not found with ID: {userId}");
                }

                // Get all conversations where this user has messages
                var conversations = await _conversationRepository.Query()
                    .Include(c => c.Match)
                        .ThenInclude(m => m.Sport)
                    .Include(c => c.Match)
                        .ThenInclude(m => m.Requester)
                    .Include(c => c.Match)
                        .ThenInclude(m => m.Recipient)
                    .Include(c => c.Messages.OrderByDescending(m => m.SentAt).Take(1))
                    .Where(c => c.Messages.Any(m => m.SenderId == user.UserId || 
                               (c.Match != null && (c.Match.RequesterId == user.UserId || c.Match.RecipientId == user.UserId))))
                    .OrderByDescending(c => c.LastMessageAt)
                    .ToListAsync();

                // Map to DTOs
                var conversationDtos = new List<ConversationDto>();
                foreach (var conversation in conversations)
                {
                    var dto = _mapper.Map<ConversationDto>(conversation);
                    
                    // Determine the other user
                    Guid? otherUserId = null;
                    if (conversation.Match != null)
                    {
                        otherUserId = conversation.Match.RequesterId == user.UserId 
                            ? conversation.Match.RecipientId 
                            : conversation.Match.RequesterId;
                        
                        var otherUser = conversation.Match.RequesterId == user.UserId
                            ? conversation.Match.Recipient
                            : conversation.Match.Requester;
                        
                        dto.OtherUserId = otherUserId.Value;
                        dto.OtherUser = new UserBriefDto
                        {
                            UserId = otherUserId.Value,
                            FirstName = otherUser.FirstName,
                            ProfilePictureUrl = otherUser.Profile?.ProfilePictureUrl
                        };
                    }

                    // Get unread message count for this conversation
                    int unreadCount = await _messageRepository.Query()
                        .CountAsync(m => m.ConversationId == conversation.ConversationId && 
                                        m.SenderId != user.UserId && 
                                        m.ReadAt == null);
                    
                    dto.UnreadCount = unreadCount;
                    conversationDtos.Add(dto);
                }

                return conversationDtos;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error getting conversations for user {userId}");
                throw;
            }
        }

        public async Task<ConversationDto> GetConversation(Guid conversationId, string userId)
        {
            try
            {
                var user = await _userRepository.Query()
                    .FirstOrDefaultAsync(u => u.Auth0Id == userId);

                if (user == null)
                {
                    throw new KeyNotFoundException($"User not found with ID: {userId}");
                }

                var conversation = await _conversationRepository.Query()
                    .Include(c => c.Match)
                        .ThenInclude(m => m.Sport)
                    .Include(c => c.Match)
                        .ThenInclude(m => m.Requester)
                            .ThenInclude(u => u.Profile)
                    .Include(c => c.Match)
                        .ThenInclude(m => m.Recipient)
                            .ThenInclude(u => u.Profile)
                    .Include(c => c.Messages.OrderByDescending(m => m.SentAt).Take(1))
                    .FirstOrDefaultAsync(c => c.ConversationId == conversationId);

                if (conversation == null)
                {
                    throw new KeyNotFoundException($"Conversation not found with ID: {conversationId}");
                }

                // Verify the user is part of this conversation
                bool isUserInConversation = false;
                
                if (conversation.Match != null)
                {
                    isUserInConversation = conversation.Match.RequesterId == user.UserId || 
                                          conversation.Match.RecipientId == user.UserId;
                }
                else
                {
                    // Check if user has messages in this conversation
                    isUserInConversation = await _messageRepository.Query()
                        .AnyAsync(m => m.ConversationId == conversationId && m.SenderId == user.UserId);
                }

                if (!isUserInConversation)
                {
                    throw new UnauthorizedAccessException("You are not authorized to access this conversation");
                }

                var dto = _mapper.Map<ConversationDto>(conversation);
                
                // Determine the other user
                if (conversation.Match != null)
                {
                    var otherUserId = conversation.Match.RequesterId == user.UserId 
                        ? conversation.Match.RecipientId 
                        : conversation.Match.RequesterId;
                    
                    var otherUser = conversation.Match.RequesterId == user.UserId
                        ? conversation.Match.Recipient
                        : conversation.Match.Requester;
                    
                    dto.OtherUserId = otherUserId;
                    dto.OtherUser = new UserBriefDto
                    {
                        UserId = otherUserId,
                        FirstName = otherUser.FirstName,
                        ProfilePictureUrl = otherUser.Profile?.ProfilePictureUrl
                    };
                }

                // Get unread message count
                int unreadCount = await _messageRepository.Query()
                    .CountAsync(m => m.ConversationId == conversationId && 
                                    m.SenderId != user.UserId && 
                                    m.ReadAt == null);
                
                dto.UnreadCount = unreadCount;

                return dto;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error getting conversation {conversationId} for user {userId}");
                throw;
            }
        }

        public async Task<MessageListDto> GetMessages(Guid conversationId, string userId, int page = 1, int pageSize = 20)
        {
            try
            {
                var user = await _userRepository.Query()
                    .FirstOrDefaultAsync(u => u.Auth0Id == userId);

                if (user == null)
                {
                    throw new KeyNotFoundException($"User not found with ID: {userId}");
                }

                var conversation = await _conversationRepository.GetByIdAsync(conversationId);
                if (conversation == null)
                {
                    throw new KeyNotFoundException($"Conversation not found with ID: {conversationId}");
                }

                // Verify the user is part of this conversation
                bool isUserInConversation = false;
                
                if (conversation.MatchId.HasValue)
                {
                    var match = await _matchRepository.GetByIdAsync(conversation.MatchId.Value);
                    isUserInConversation = match.RequesterId == user.UserId || match.RecipientId == user.UserId;
                }
                else
                {
                    // Check if user has messages in this conversation
                    isUserInConversation = await _messageRepository.Query()
                        .AnyAsync(m => m.ConversationId == conversationId && m.SenderId == user.UserId);
                }

                if (!isUserInConversation)
                {
                    throw new UnauthorizedAccessException("You are not authorized to access this conversation");
                }

                // Get total count for pagination
                var totalCount = await _messageRepository.Query()
                    .CountAsync(m => m.ConversationId == conversationId);

                // Get paginated messages, newest first
                var messages = await _messageRepository.Query()
                    .Where(m => m.ConversationId == conversationId)
                    .OrderByDescending(m => m.SentAt)
                    .Skip((page - 1) * pageSize)
                    .Take(pageSize)
                    .ToListAsync();

                // Order chronologically for display
                var orderedMessages = messages.OrderBy(m => m.SentAt).ToList();
                
                return new MessageListDto
                {
                    Messages = _mapper.Map<IEnumerable<MessageDto>>(orderedMessages),
                    TotalCount = totalCount,
                    HasMore = (page * pageSize) < totalCount
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error getting messages for conversation {conversationId} for user {userId}");
                throw;
            }
        }

        public async Task<MessageDto> SendMessage(Guid conversationId, string userId, SendMessageDto messageDto)
        {
            try
            {
                var user = await _userRepository.Query()
                    .FirstOrDefaultAsync(u => u.Auth0Id == userId);

                if (user == null)
                {
                    throw new KeyNotFoundException($"User not found with ID: {userId}");
                }

                var conversation = await _conversationRepository.GetByIdAsync(conversationId);
                if (conversation == null)
                {
                    throw new KeyNotFoundException($"Conversation not found with ID: {conversationId}");
                }

                // Verify the user is part of this conversation
                bool isUserInConversation = false;
                
                if (conversation.MatchId.HasValue)
                {
                    var match = await _matchRepository.GetByIdAsync(conversation.MatchId.Value);
                    isUserInConversation = match.RequesterId == user.UserId || match.RecipientId == user.UserId;
                    
                    // Also verify the match is accepted
                    if (match.Status != "Accepted")
                    {
                        throw new InvalidOperationException("You cannot send messages in this conversation until the match is accepted");
                    }
                }
                else
                {
                    // Check if user has messages in this conversation
                    isUserInConversation = await _messageRepository.Query()
                        .AnyAsync(m => m.ConversationId == conversationId && m.SenderId == user.UserId);
                }

                if (!isUserInConversation)
                {
                    throw new UnauthorizedAccessException("You are not authorized to send messages in this conversation");
                }

                // Create new message
                var message = new Message
                {
                    MessageId = Guid.NewGuid(),
                    ConversationId = conversationId,
                    SenderId = user.UserId,
                    Content = messageDto.Content,
                    SentAt = DateTime.UtcNow
                };

                await _messageRepository.AddAsync(message);
                
                // Update conversation's last message timestamp
                conversation.LastMessageAt = message.SentAt;
                await _conversationRepository.UpdateAsync(conversation);
                
                await _messageRepository.SaveChangesAsync();

                return _mapper.Map<MessageDto>(message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error sending message in conversation {conversationId} by user {userId}");
                throw;
            }
        }

        public async Task MarkMessagesAsRead(Guid conversationId, string userId)
        {
            try
            {
                var user = await _userRepository.Query()
                    .FirstOrDefaultAsync(u => u.Auth0Id == userId);

                if (user == null)
                {
                    throw new KeyNotFoundException($"User not found with ID: {userId}");
                }

                var conversation = await _conversationRepository.GetByIdAsync(conversationId);
                if (conversation == null)
                {
                    throw new KeyNotFoundException($"Conversation not found with ID: {conversationId}");
                }

                // Verify the user is part of this conversation
                bool isUserInConversation = false;
                
                if (conversation.MatchId.HasValue)
                {
                    var match = await _matchRepository.GetByIdAsync(conversation.MatchId.Value);
                    isUserInConversation = match.RequesterId == user.UserId || match.RecipientId == user.UserId;
                }
                else
                {
                    // Check if user has messages in this conversation
                    isUserInConversation = await _messageRepository.Query()
                        .AnyAsync(m => m.ConversationId == conversationId && m.SenderId == user.UserId);
                }

                if (!isUserInConversation)
                {
                    throw new UnauthorizedAccessException("You are not authorized to access this conversation");
                }

                // Get all unread messages sent by others
                var unreadMessages = await _messageRepository.Query()
                    .Where(m => m.ConversationId == conversationId && 
                               m.SenderId != user.UserId && 
                               m.ReadAt == null)
                    .ToListAsync();

                // Mark them as read
                foreach (var message in unreadMessages)
                {
                    message.ReadAt = DateTime.UtcNow;
                    await _messageRepository.UpdateAsync(message);
                }

                await _messageRepository.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error marking messages as read in conversation {conversationId} for user {userId}");
                throw;
            }
        }

        public async Task<ConversationDto> CreateConversation(string userId, CreateConversationDto conversationDto)
        {
            try
            {
                var user = await _userRepository.Query()
                    .FirstOrDefaultAsync(u => u.Auth0Id == userId);

                if (user == null)
                {
                    throw new KeyNotFoundException($"User not found with ID: {userId}");
                }

                var otherUser = await _userRepository.Query()
                    .Include(u => u.Profile)
                    .FirstOrDefaultAsync(u => u.UserId == conversationDto.UserId);

                if (otherUser == null)
                {
                    throw new KeyNotFoundException($"Other user not found with ID: {conversationDto.UserId}");
                }

                // Check if a conversation already exists between these users
                Conversation existingConversation = null;

                if (conversationDto.MatchId.HasValue)
                {
                    // Check if a conversation exists for this match
                    existingConversation = await _conversationRepository.Query()
                        .FirstOrDefaultAsync(c => c.MatchId == conversationDto.MatchId);
                    
                    // Verify the match exists and involves both users
                    var match = await _matchRepository.GetByIdAsync(conversationDto.MatchId.Value);
                    if (match == null)
                    {
                        throw new KeyNotFoundException($"Match not found with ID: {conversationDto.MatchId}");
                    }
                    
                    bool isUserInMatch = match.RequesterId == user.UserId || match.RecipientId == user.UserId;
                    bool isOtherUserInMatch = match.RequesterId == otherUser.UserId || match.RecipientId == otherUser.UserId;
                    
                    if (!isUserInMatch || !isOtherUserInMatch)
                    {
                        throw new UnauthorizedAccessException("You cannot create a conversation for a match you are not part of");
                    }
                    
                    // Verify the match is accepted
                    if (match.Status != "Accepted")
                    {
                        throw new InvalidOperationException("You cannot create a conversation until the match is accepted");
                    }
                }
                else
                {
                    // Check direct conversations between users (rare case, usually goes through match)
                    var matchesWithUser = await _matchRepository.Query()
                        .Where(m => 
                            ((m.RequesterId == user.UserId && m.RecipientId == otherUser.UserId) ||
                             (m.RequesterId == otherUser.UserId && m.RecipientId == user.UserId)) &&
                            m.Status == "Accepted")
                        .ToListAsync();
                    
                    foreach (var match in matchesWithUser)
                    {
                        var conversation = await _conversationRepository.Query()
                            .FirstOrDefaultAsync(c => c.MatchId == match.MatchId);
                        
                        if (conversation != null)
                        {
                            existingConversation = conversation;
                            break;
                        }
                    }
                }

                if (existingConversation != null)
                {
                    // Return the existing conversation
                    return await GetConversation(existingConversation.ConversationId, userId);
                }

                // Create a new conversation
                var newConversation = new Conversation
                {
                    ConversationId = Guid.NewGuid(),
                    MatchId = conversationDto.MatchId,
                    CreatedAt = DateTime.UtcNow,
                    LastMessageAt = DateTime.UtcNow,
                    IsActive = true
                };

                await _conversationRepository.AddAsync(newConversation);
                await _conversationRepository.SaveChangesAsync();

                // Return the created conversation
                return await GetConversation(newConversation.ConversationId, userId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error creating conversation for user {userId}");
                throw;
            }
        }

        public async Task<ConversationDto> GetConversationWithUser(string userId, string otherUserId)
        {
            try
            {
                var user = await _userRepository.Query()
                    .FirstOrDefaultAsync(u => u.Auth0Id == userId);

                if (user == null)
                {
                    throw new KeyNotFoundException($"User not found with ID: {userId}");
                }

                var otherUser = await _userRepository.Query()
                    .FirstOrDefaultAsync(u => u.Auth0Id == otherUserId);

                if (otherUser == null)
                {
                    throw new KeyNotFoundException($"Other user not found with ID: {otherUserId}");
                }

                // Find matches between the users
                var matches = await _matchRepository.Query()
                    .Where(m => 
                        ((m.RequesterId == user.UserId && m.RecipientId == otherUser.UserId) ||
                         (m.RequesterId == otherUser.UserId && m.RecipientId == user.UserId)) &&
                        m.Status == "Accepted")
                    .ToListAsync();

                if (!matches.Any())
                {
                    throw new InvalidOperationException("No accepted match exists between these users");
                }

                // Check for existing conversations
                foreach (var match in matches)
                {
                    var conversation = await _conversationRepository.Query()
                        .FirstOrDefaultAsync(c => c.MatchId == match.MatchId);
                    
                    if (conversation != null)
                    {
                        return await GetConversation(conversation.ConversationId, userId);
                    }
                }

                // No conversation exists yet, create one with the first match
                var firstMatch = matches.First();
                
                var newConversation = new Conversation
                {
                    ConversationId = Guid.NewGuid(),
                    MatchId = firstMatch.MatchId,
                    CreatedAt = DateTime.UtcNow,
                    LastMessageAt = DateTime.UtcNow,
                    IsActive = true
                };

                await _conversationRepository.AddAsync(newConversation);
                await _conversationRepository.SaveChangesAsync();

                return await GetConversation(newConversation.ConversationId, userId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error getting conversation between user {userId} and {otherUserId}");
                throw;
            }
        }

        public async Task<ConversationDto> GetConversationForMatch(Guid matchId, string userId)
        {
            try
            {
                var user = await _userRepository.Query()
                    .FirstOrDefaultAsync(u => u.Auth0Id == userId);

                if (user == null)
                {
                    throw new KeyNotFoundException($"User not found with ID: {userId}");
                }

                var match = await _matchRepository.GetByIdAsync(matchId);
                if (match == null)
                {
                    throw new KeyNotFoundException($"Match not found with ID: {matchId}");
                }

                // Verify the user is part of this match
                if (match.RequesterId != user.UserId && match.RecipientId != user.UserId)
                {
                    throw new UnauthorizedAccessException("You are not authorized to access this match");
                }

                // Verify the match is accepted
                if (match.Status != "Accepted")
                {
                    throw new InvalidOperationException("You cannot access the conversation until the match is accepted");
                }

                // Look for existing conversation
                var conversation = await _conversationRepository.Query()
                    .FirstOrDefaultAsync(c => c.MatchId == matchId);

                if (conversation != null)
                {
                    return await GetConversation(conversation.ConversationId, userId);
                }

                // Create a new conversation
                var newConversation = new Conversation
                {
                    ConversationId = Guid.NewGuid(),
                    MatchId = matchId,
                    CreatedAt = DateTime.UtcNow,
                    LastMessageAt = DateTime.UtcNow,
                    IsActive = true
                };

                await _conversationRepository.AddAsync(newConversation);
                await _conversationRepository.SaveChangesAsync();

                return await GetConversation(newConversation.ConversationId, userId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error getting conversation for match {matchId} for user {userId}");
                throw;
            }
        }

        public async Task<int> GetUnreadMessageCount(string userId)
        {
            try
            {
                var user = await _userRepository.Query()
                    .FirstOrDefaultAsync(u => u.Auth0Id == userId);

                if (user == null)
                {
                    throw new KeyNotFoundException($"User not found with ID: {userId}");
                }

                // Get conversations this user is part of
                var conversationIds = await GetUserConversationIds(user.UserId);

                // Count unread messages in these conversations
                var unreadCount = await _messageRepository.Query()
                    .CountAsync(m => 
                        conversationIds.Contains(m.ConversationId) && 
                        m.SenderId != user.UserId && 
                        m.ReadAt == null);

                return unreadCount;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error getting unread message count for user {userId}");
                throw;
            }
        }

        // Helper method to get all conversation IDs for a user
        private async Task<List<Guid>> GetUserConversationIds(Guid userId)
        {
            // Get conversations from matches
            var matchConversations = await _conversationRepository.Query()
                .Include(c => c.Match)
                .Where(c => c.Match != null && 
                         (c.Match.RequesterId == userId || c.Match.RecipientId == userId))
                .Select(c => c.ConversationId)
                .ToListAsync();

            // Get conversations where the user has sent messages (direct conversations without matches)
            var directConversations = await _messageRepository.Query()
                .Where(m => m.SenderId == userId)
                .Select(m => m.ConversationId)
                .Distinct()
                .ToListAsync();

            // Combine and remove duplicates
            return matchConversations
                .Concat(directConversations)
                .Distinct()
                .ToList();
        }
    }
}