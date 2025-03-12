using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using BuddyUp.API.Models.DTOs;
using BuddyUp.API.Models.Responses;

namespace BuddyUp.API.Services.Interfaces
{
    public interface IAdminService
    {
        /// <summary>
        /// Get dashboard statistics
        /// </summary>
        Task<DashboardStatsDto> GetDashboardStats();
        
        /// <summary>
        /// Get all users with optional filtering
        /// </summary>
        Task<PaginatedResponse<UserListItemDto>> GetUsers(UserSearchDto searchDto);
        
        /// <summary>
        /// Update a user's status (active/inactive)
        /// </summary>
        Task<UserDto> UpdateUserStatus(Guid userId, bool active);
        
        /// <summary>
        /// Set a user's admin privileges
        /// </summary>
        Task<UserDto> SetAdminStatus(Guid userId, bool isAdmin);
        
        /// <summary>
        /// Get all sports
        /// </summary>
        Task<IEnumerable<SportDto>> GetSports();
        
        /// <summary>
        /// Add a new sport
        /// </summary>
        Task<SportDto> AddSport(string userId, SportCreateDto sportDto);
        
        /// <summary>
        /// Update an existing sport
        /// </summary>
        Task<SportDto> UpdateSport(Guid sportId, SportUpdateDto sportDto);
        
        /// <summary>
        /// Get all locations with pagination
        /// </summary>
        Task<PaginatedResponse<LocationDto>> GetLocations(int page = 1, int pageSize = 10);
        
        /// <summary>
        /// Add a new location
        /// </summary>
        Task<LocationDto> AddLocation(string userId, LocationCreateDto locationDto);
        
        /// <summary>
        /// Update an existing location
        /// </summary>
        Task<LocationDto> UpdateLocation(Guid locationId, LocationUpdateDto locationDto);
        
        /// <summary>
        /// Get user reports with optional filter by status
        /// </summary>
        Task<PaginatedResponse<UserReportDto>> GetUserReports(string status = null, int page = 1, int pageSize = 10);
        
        /// <summary>
        /// Handle a user report (review, dismiss, take action)
        /// </summary>
        Task<UserReportDto> HandleUserReport(Guid reportId, string adminUserId, UserReportUpdateDto updateDto);
        
        /// <summary>
        /// Delete content (admin function)
        /// </summary>
        Task DeleteContent(string contentType, Guid contentId, string adminUserId, string reason);
    }
}