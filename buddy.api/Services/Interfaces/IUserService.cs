
using BuddyUp.API.Models.DTOs;
using BuddyUp.API.Models.Responses;

namespace BuddyUp.API.Services.Interfaces
{
    public interface IUserService
    {
        /// <summary>
        /// Get a user by Auth0 ID
        /// </summary>
        Task<UserDto> GetUserByAuth0Id(string auth0Id);
        
        /// <summary>
        /// Get a user by internal user ID
        /// </summary>
        Task<UserDto> GetUserById(Guid userId);
        
        /// <summary>
        /// Create a new user
        /// </summary>
        Task<UserDto> CreateUser(UserDto userDto);
        
        /// <summary>
        /// Update a user's basic information
        /// </summary>
        Task<UserDto> UpdateUser(UserDto userDto);
        
        /// <summary>
        /// Set a user's login timestamp
        /// </summary>
        Task UpdateLastLogin(string auth0Id);
        
        /// <summary>
        /// Toggle a user's active status
        /// </summary>
        Task<UserDto> UpdateUserStatus(Guid userId, bool active);
        
        /// <summary>
        /// Get a paginated list of users with optional filtering
        /// </summary>
        Task<PaginatedResponse<UserListItemDto>> GetUsers(UserSearchDto searchDto);
        
        /// <summary>
        /// Set a user's admin status
        /// </summary>
        Task<UserDto> SetAdminStatus(Guid userId, bool isAdmin);
        
        /// <summary>
        /// Set a user's verification status
        /// </summary>
        Task<UserDto> SetVerificationStatus(Guid userId, bool isVerified);
    }
}