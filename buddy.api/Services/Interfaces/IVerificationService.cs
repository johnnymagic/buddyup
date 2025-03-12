using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using BuddyUp.API.Models.DTOs;
using BuddyUp.API.Models.Responses;

namespace BuddyUp.API.Services.Interfaces
{
    public interface IVerificationService
    {
        /// <summary>
        /// Get verification status for a user
        /// </summary>
        Task<VerificationDto> GetVerificationStatus(string userId);
        
        /// <summary>
        /// Initiate the verification process
        /// </summary>
        Task<VerificationDto> InitiateVerification(VerificationRequestDto requestDto);
        
        /// <summary>
        /// Process verification callback from external provider
        /// </summary>
        Task<VerificationDto> ProcessVerificationCallback(string provider, object callbackData);
        
        /// <summary>
        /// Complete the verification process manually (admin function)
        /// </summary>
        Task<VerificationDto> CompleteVerification(Guid verificationId, bool approved, string adminUserId, string notes = null);
        
        /// <summary>
        /// Get pending verification requests (admin function)
        /// </summary>
        Task<PaginatedResponse<AdminVerificationDto>> GetPendingVerifications(int page = 1, int pageSize = 10);
        
        /// <summary>
        /// Get all verifications by status (admin function)
        /// </summary>
        Task<PaginatedResponse<AdminVerificationDto>> GetVerifications(string status, int page = 1, int pageSize = 10);
        
        /// <summary>
        /// Get verification by ID (admin function)
        /// </summary>
        Task<AdminVerificationDto> GetVerificationById(Guid verificationId);
    }
}