
using BuddyUp.API.Models.DTOs;


namespace BuddyUp.API.Services.Interfaces
{
    public interface IProfileService
    {
        /// <summary>
        /// Get a user profile by Auth0 user ID
        /// </summary>
        Task<ProfileDto> GetProfileByUserId(string userId);
        
        /// <summary>
        /// Create a new user profile
        /// </summary>
        Task<ProfileDto> CreateProfile(ProfileDto profileDto);
        
        /// <summary>
        /// Update an existing user profile
        /// </summary>
        Task<ProfileDto> UpdateProfile(ProfileDto profileDto);
        
        /// <summary>
        /// Get user sports by Auth0 user ID
        /// </summary>
        Task<IEnumerable<UserSportDto>> GetUserSports(string userId);
        
        /// <summary>
        /// Get a specific user sport by ID
        /// </summary>
        Task<UserSportDto> GetUserSportById(Guid userSportId);
        
        /// <summary>
        /// Add a sport to a user's profile
        /// </summary>
        Task<UserSportDto> AddUserSport(UserSportDto userSportDto);
        
        /// <summary>
        /// Update a user's sport
        /// </summary>
        Task<UserSportDto> UpdateUserSport(UserSportDto userSportDto);
        
        /// <summary>
        /// Remove a sport from a user's profile
        /// </summary>
        Task RemoveUserSport(Guid userSportId);
        
        /// <summary>
        /// Get the verification status of a user
        /// </summary>
        Task<VerificationDto> GetVerificationStatus(string userId);
        
        /// <summary>
        /// Initiate the verification process for a user
        /// </summary>
        Task<VerificationDto> InitiateVerification(VerificationRequestDto requestDto);
    }
}