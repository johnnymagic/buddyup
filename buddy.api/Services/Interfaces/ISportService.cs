
using BuddyUp.API.Models.DTOs;

namespace BuddyUp.API.Services.Interfaces
{
    public interface ISportService
    {
        /// <summary>
        /// Get a user profile by db user ID
        /// </summary>
        Task<SportDto> GetSportById(Guid sportId);

    }
}