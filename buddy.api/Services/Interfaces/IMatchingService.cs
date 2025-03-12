using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using BuddyUp.API.Models.DTOs;

namespace BuddyUp.API.Services.Interfaces
{
    public interface IMatchingService
    {
        /// <summary>
        /// Get potential workout buddies based on filter criteria
        /// </summary>
        Task<IEnumerable<PotentialMatchDto>> GetPotentialMatches(string userId, MatchFilterDto filter);
        
        /// <summary>
        /// Get current matches (accepted buddy requests)
        /// </summary>
        Task<IEnumerable<MatchDto>> GetCurrentMatches(string userId);
        
        /// <summary>
        /// Get match requests sent by the user
        /// </summary>
        Task<IEnumerable<MatchDto>> GetSentRequests(string userId);
        
        /// <summary>
        /// Get match requests received by the user
        /// </summary>
        Task<IEnumerable<MatchDto>> GetReceivedRequests(string userId);
        
        /// <summary>
        /// Send a match request to another user
        /// </summary>
        Task<MatchDto> SendMatchRequest(string userId, MatchRequestDto requestDto);
        
        /// <summary>
        /// Respond to a match request (accept or reject)
        /// </summary>
        Task<MatchDto> RespondToMatchRequest(string userId, Guid matchId, MatchResponseDto responseDto);
        
        /// <summary>
        /// Cancel a match request or unmatch from a buddy
        /// </summary>
        Task CancelMatch(string userId, Guid matchId);
        
        /// <summary>
        /// Get a specific match by ID
        /// </summary>
        Task<MatchDto> GetMatchById(Guid matchId);
    }
}