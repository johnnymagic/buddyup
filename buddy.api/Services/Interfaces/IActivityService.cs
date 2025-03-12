using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using BuddyUp.API.Models.DTOs;
using BuddyUp.API.Models.Responses;

namespace BuddyUp.API.Services.Interfaces
{
    public interface IActivityService
    {
        /// <summary>
        /// Get activities based on search criteria
        /// </summary>
        Task<IEnumerable<ActivityDto>> GetActivities(string userId, ActivitySearchDto searchDto);
        
        /// <summary>
        /// Get a specific activity by ID
        /// </summary>
        Task<ActivityDto> GetActivityById(Guid activityId, string userId = null);
        
        /// <summary>
        /// Create a new activity
        /// </summary>
        Task<ActivityDto> CreateActivity(string userId, ActivityCreateDto activityDto);
        
        /// <summary>
        /// Update an existing activity
        /// </summary>
        Task<ActivityDto> UpdateActivity(Guid activityId, string userId, ActivityUpdateDto activityDto);
        
        /// <summary>
        /// Delete an activity
        /// </summary>
        Task DeleteActivity(Guid activityId, string userId);
        
        /// <summary>
        /// Get activities created by a specific user
        /// </summary>
        Task<IEnumerable<ActivityDto>> GetUserActivities(string userId);
        
        /// <summary>
        /// Get activities near a specific location
        /// </summary>
        Task<PaginatedResponse<ActivityDto>> GetNearbyActivities(double latitude, double longitude, 
            double distance = 10, int page = 1, int pageSize = 10);
            
        /// <summary>
        /// Get recommended activities for a user based on their preferences
        /// </summary>
        Task<IEnumerable<ActivityDto>> GetRecommendedActivities(string userId);
    }
}