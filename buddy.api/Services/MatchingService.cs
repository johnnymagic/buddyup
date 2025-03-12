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
using NetTopologySuite.Geometries;

namespace BuddyUp.API.Services.Implementations
{
    public class MatchingService : IMatchingService
    {
        private readonly IRepository<User> _userRepository;
        private readonly IRepository<UserProfile> _profileRepository;
        private readonly IRepository<UserSport> _userSportRepository;
        private readonly IRepository<Match> _matchRepository;
        private readonly IRepository<Sport> _sportRepository;
        private readonly ILogger<MatchingService> _logger;
        private readonly IMapper _mapper;

        public MatchingService(
            IRepository<User> userRepository,
            IRepository<UserProfile> profileRepository,
            IRepository<UserSport> userSportRepository,
            IRepository<Match> matchRepository,
            IRepository<Sport> sportRepository,
            ILogger<MatchingService> logger,
            IMapper mapper)
        {
            _userRepository = userRepository ?? throw new ArgumentNullException(nameof(userRepository));
            _profileRepository = profileRepository ?? throw new ArgumentNullException(nameof(profileRepository));
            _userSportRepository = userSportRepository ?? throw new ArgumentNullException(nameof(userSportRepository));
            _matchRepository = matchRepository ?? throw new ArgumentNullException(nameof(matchRepository));
            _sportRepository = sportRepository ?? throw new ArgumentNullException(nameof(sportRepository));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
        }

        public async Task<IEnumerable<PotentialMatchDto>> GetPotentialMatches(string userId, MatchFilterDto filter)
        {
            try
            {
                // Get the current user and their profile
                var currentUser = await _userRepository.Query()
                    .Include(u => u.Profile)
                    .FirstOrDefaultAsync(u => u.Auth0Id == userId);

                if (currentUser == null)
                {
                    throw new KeyNotFoundException($"User not found with ID: {userId}");
                }

                // Get the user's sports
                var userSports = await _userSportRepository.Query()
                    .Include(us => us.Sport)
                    .Where(us => us.UserId == currentUser.UserId)
                    .ToListAsync();

                if (!userSports.Any())
                {
                    return new List<PotentialMatchDto>();
                }

                // Build a query for potential matches
                var query = _userSportRepository.Query()
                    .Include(us => us.User)
                        .ThenInclude(u => u.Profile)
                    .Include(us => us.Sport)
                    .Where(us =>
                        us.User.Active &&
                        us.User.UserId != currentUser.UserId);

                // Filter by sport if specified
                if (filter.SportId.HasValue)
                {
                    query = query.Where(us => us.SportId == filter.SportId.Value);
                }
                else
                {
                    // Otherwise, only include users with the same sports as the current user
                    var sportIds = userSports.Select(us => us.SportId).ToList();
                    query = query.Where(us => sportIds.Contains(us.SportId));
                }

                // Filter by skill level if specified
                if (!string.IsNullOrEmpty(filter.SkillLevel))
                {
                    query = query.Where(us => us.SkillLevel == filter.SkillLevel);
                }

                // Get user sport matches
                var userSportMatches = await query.ToListAsync();

                // Group by user ID to avoid duplicates
                var userSportsByUser = userSportMatches
                    .GroupBy(us => us.UserId)
                    .ToDictionary(g => g.Key, g => g.ToList());

                // Get current user's location
                Point userLocation = currentUser.Profile?.PreferredLocation;
                int maxDistance = filter.Distance;

                // Final list of potential matches
                var potentialMatches = new List<PotentialMatchDto>();

                // Check for existing matches to exclude
                var existingMatches = await _matchRepository.Query()
                    .Where(m =>
                        (m.RequesterId == currentUser.UserId || m.RecipientId == currentUser.UserId) &&
                        m.Status != "Rejected")
                    .ToListAsync();

                var matchedUserIds = existingMatches
                    .Select(m => m.RequesterId == currentUser.UserId ? m.RecipientId : m.RequesterId)
                    .ToHashSet();

                foreach (var userGroup in userSportsByUser)
                {
                    var otherUserId = userGroup.Key;
                    var otherUserSports = userGroup.Value;

                    // Skip if already matched
                    if (matchedUserIds.Contains(otherUserId))
                    {
                        continue;
                    }

                    // Get the first sport to use for info
                    var otherUserSport = otherUserSports.First();
                    var otherUser = otherUserSport.User;
                    var otherUserProfile = otherUser.Profile;

                    // Calculate distance if both users have locations
                    double? distance = null;
                    if (userLocation != null && otherUserProfile?.PreferredLocation != null)
                    {
                        distance = userLocation.Distance(otherUserProfile.PreferredLocation) / 1000; // Convert to km

                        // Skip if distance exceeds filter
                        if (distance > maxDistance)
                        {
                            continue;
                        }
                    }

                    // Filter by days and times if specified
                    if (filter.Days != null && filter.Days.Count > 0 && otherUserProfile != null)
                    {
                        var otherUserDays = otherUserProfile.PreferredDays ?? new List<string>();

                        if (!filter.Days.Any(d => otherUserDays.Contains(d)))
                        {
                            continue;
                        }
                    }

                    if (filter.Times != null && filter.Times.Count > 0 && otherUserProfile != null)
                    {
                        var otherUserTimes = otherUserProfile.PreferredTimes ?? new List<string>();


                        if (!filter.Times.Any(t => otherUserTimes.Contains(t)))
                        {
                            continue;
                        }
                    }

                    // Create potential match objectß
                    var match = new PotentialMatchDto
                    {
                        UserId = Guid.Parse(otherUser.Auth0Id),
                        FirstName = otherUser.FirstName,
                        ProfilePictureUrl = otherUserProfile?.ProfilePictureUrl,
                        Bio = otherUserProfile?.Bio,
                        SportId = otherUserSport.SportId,
                        Sport = otherUserSport.Sport.Name,
                        SkillLevel = otherUserSport.SkillLevel,
                        Verified = otherUser.IsVerified,
                        Distance = distance ?? 0,
                        PreferredDays = otherUserProfile?.PreferredDays ?? new List<string>(),
                        PreferredTimes = otherUserProfile?.PreferredTimes ?? new List<string>()
                    };

                    potentialMatches.Add(match);
                }

                // Sort by distance
                return potentialMatches.OrderBy(m => m.Distance);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error getting potential matches for user {userId}");
                throw;
            }
        }

        public async Task<IEnumerable<MatchDto>> GetCurrentMatches(string userId)
        {
            try
            {
                var user = await _userRepository.Query()
                    .FirstOrDefaultAsync(u => u.Auth0Id == userId);

                if (user == null)
                {
                    throw new KeyNotFoundException($"User not found with ID: {userId}");
                }

                var matches = await _matchRepository.Query()
                    .Include(m => m.Requester)
                        .ThenInclude(u => u.Profile)
                    .Include(m => m.Recipient)
                        .ThenInclude(u => u.Profile)
                    .Include(m => m.Sport)
                    .Where(m =>
                        (m.RequesterId == user.UserId || m.RecipientId == user.UserId) &&
                        m.Status == "Accepted")
                    .ToListAsync();

                return _mapper.Map<IEnumerable<MatchDto>>(matches);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error getting current matches for user {userId}");
                throw;
            }
        }

        public async Task<IEnumerable<MatchDto>> GetSentRequests(string userId)
        {
            try
            {
                var user = await _userRepository.Query()
                    .FirstOrDefaultAsync(u => u.Auth0Id == userId);

                if (user == null)
                {
                    throw new KeyNotFoundException($"User not found with ID: {userId}");
                }

                var matches = await _matchRepository.Query()
                    .Include(m => m.Requester)
                        .ThenInclude(u => u.Profile)
                    .Include(m => m.Recipient)
                        .ThenInclude(u => u.Profile)
                    .Include(m => m.Sport)
                    .Where(m =>
                        m.RequesterId == user.UserId &&
                        m.Status == "Pending")
                    .ToListAsync();

                return _mapper.Map<IEnumerable<MatchDto>>(matches);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error getting sent requests for user {userId}");
                throw;
            }
        }

        public async Task<IEnumerable<MatchDto>> GetReceivedRequests(string userId)
        {
            try
            {
                var user = await _userRepository.Query()
                    .FirstOrDefaultAsync(u => u.Auth0Id == userId);

                if (user == null)
                {
                    throw new KeyNotFoundException($"User not found with ID: {userId}");
                }

                var matches = await _matchRepository.Query()
                    .Include(m => m.Requester)
                        .ThenInclude(u => u.Profile)
                    .Include(m => m.Recipient)
                        .ThenInclude(u => u.Profile)
                    .Include(m => m.Sport)
                    .Where(m =>
                        m.RecipientId == user.UserId &&
                        m.Status == "Pending")
                    .ToListAsync();

                return _mapper.Map<IEnumerable<MatchDto>>(matches);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error getting received requests for user {userId}");
                throw;
            }
        }

        public async Task<MatchDto> SendMatchRequest(string userId, MatchRequestDto requestDto)
        {
            try
            {
                var requester = await _userRepository.Query()
                    .FirstOrDefaultAsync(u => u.Auth0Id == userId);

                if (requester == null)
                {
                    throw new KeyNotFoundException($"User not found with ID: {userId}");
                }

                var recipient = await _userRepository.Query()
                    .FirstOrDefaultAsync(u => u.UserId == requestDto.RecipientId);

                if (recipient == null)
                {
                    throw new KeyNotFoundException($"Recipient not found with ID: {requestDto.RecipientId}");
                }

                // Verify sport exists
                var sport = await _sportRepository.GetByIdAsync(requestDto.SportId);
                if (sport == null)
                {
                    throw new KeyNotFoundException($"Sport not found with ID: {requestDto.SportId}");
                }

                // Check if user has this sport
                var userSport = await _userSportRepository.Query()
                    .FirstOrDefaultAsync(us =>
                        us.UserId == requester.UserId &&
                        us.SportId == requestDto.SportId);

                if (userSport == null)
                {
                    throw new InvalidOperationException($"User does not have sport with ID: {requestDto.SportId}");
                }

                // Check recipient has this sport
                var recipientSport = await _userSportRepository.Query()
                    .FirstOrDefaultAsync(us =>
                        us.UserId == recipient.UserId &&
                        us.SportId == requestDto.SportId);

                if (recipientSport == null)
                {
                    throw new InvalidOperationException($"Recipient does not have sport with ID: {requestDto.SportId}");
                }

                // Check if already matched
                var existingMatch = await _matchRepository.Query()
                    .FirstOrDefaultAsync(m =>
                        ((m.RequesterId == requester.UserId && m.RecipientId == recipient.UserId) ||
                         (m.RequesterId == recipient.UserId && m.RecipientId == requester.UserId)) &&
                        m.SportId == requestDto.SportId &&
                        m.Status != "Rejected");

                if (existingMatch != null)
                {
                    throw new InvalidOperationException("A match request already exists between these users for this sport");
                }

                // Create new match request
                var match = new Match
                {
                    MatchId = Guid.NewGuid(),
                    RequesterId = requester.UserId,
                    RecipientId = recipient.UserId,
                    SportId = requestDto.SportId,
                    Status = "Pending",
                    RequestedAt = DateTime.UtcNow
                };

                await _matchRepository.AddAsync(match);
                await _matchRepository.SaveChangesAsync();

                // Load complete match with navigation properties
                match = await _matchRepository.Query()
                    .Include(m => m.Requester)
                        .ThenInclude(u => u.Profile)
                    .Include(m => m.Recipient)
                        .ThenInclude(u => u.Profile)
                    .Include(m => m.Sport)
                    .FirstOrDefaultAsync(m => m.MatchId == match.MatchId);

                return _mapper.Map<MatchDto>(match);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error sending match request from user {userId}");
                throw;
            }
        }

        public async Task<MatchDto> RespondToMatchRequest(string userId, Guid matchId, MatchResponseDto responseDto)
        {
            try
            {
                var user = await _userRepository.Query()
                    .FirstOrDefaultAsync(u => u.Auth0Id == userId);

                if (user == null)
                {
                    throw new KeyNotFoundException($"User not found with ID: {userId}");
                }

                var match = await _matchRepository.Query()
                    .Include(m => m.Requester)
                        .ThenInclude(u => u.Profile)
                    .Include(m => m.Recipient)
                        .ThenInclude(u => u.Profile)
                    .Include(m => m.Sport)
                    .FirstOrDefaultAsync(m => m.MatchId == matchId);

                if (match == null)
                {
                    throw new KeyNotFoundException($"Match not found with ID: {matchId}");
                }

                // Verify this user is the recipient
                if (match.RecipientId != user.UserId)
                {
                    throw new UnauthorizedAccessException("You are not authorized to respond to this match request");
                }

                // Verify the match is pending
                if (match.Status != "Pending")
                {
                    throw new InvalidOperationException("This match request has already been processed");
                }

                // Update match status
                match.Status = responseDto.Accept ? "Accepted" : "Rejected";
                match.RespondedAt = DateTime.UtcNow;

                await _matchRepository.UpdateAsync(match);
                await _matchRepository.SaveChangesAsync();

                return _mapper.Map<MatchDto>(match);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error responding to match request {matchId} by user {userId}");
                throw;
            }
        }

        public async Task CancelMatch(string userId, Guid matchId)
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

                // Verify this user is involved in the match
                if (match.RequesterId != user.UserId && match.RecipientId != user.UserId)
                {
                    throw new UnauthorizedAccessException("You are not authorized to cancel this match");
                }

                // If this is a pending request from the user, set to Canceled
                // If this is an accepted match, set to Canceled
                // If this is a pending request to the user, can't cancel (must reject)
                if (match.RequesterId == user.UserId && match.Status == "Pending")
                {
                    match.Status = "Canceled";
                    match.RespondedAt = DateTime.UtcNow;
                    await _matchRepository.UpdateAsync(match);
                    await _matchRepository.SaveChangesAsync();
                }
                else if (match.Status == "Accepted")
                {
                    match.Status = "Canceled";
                    match.RespondedAt = DateTime.UtcNow;
                    await _matchRepository.UpdateAsync(match);
                    await _matchRepository.SaveChangesAsync();
                }
                else if (match.RecipientId == user.UserId && match.Status == "Pending")
                {
                    throw new InvalidOperationException("You can't cancel a request sent to you. Use the respond endpoint to reject it.");
                }
                else
                {
                    throw new InvalidOperationException("This match cannot be canceled due to its current status.");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error canceling match {matchId} by user {userId}");
                throw;
            }
        }

        public async Task<MatchDto> GetMatchById(Guid matchId)
        {
            try
            {
                var match = await _matchRepository.Query()
                    .Include(m => m.Requester)
                        .ThenInclude(u => u.Profile)
                    .Include(m => m.Recipient)
                        .ThenInclude(u => u.Profile)
                    .Include(m => m.Sport)
                    .FirstOrDefaultAsync(m => m.MatchId == matchId);

                if (match == null)
                {
                    throw new KeyNotFoundException($"Match not found with ID: {matchId}");
                }

                return _mapper.Map<MatchDto>(match);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error getting match with ID {matchId}");
                throw;
            }
        }
    }
}