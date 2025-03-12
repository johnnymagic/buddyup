using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using AutoMapper;
using BuddyUp.API.Data.Repositories;
using BuddyUp.API.Models.Domain;
using BuddyUp.API.Models.DTOs;
using BuddyUp.API.Models.Responses;
using BuddyUp.API.Services.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace BuddyUp.API.Services.Implementations
{
    public class VerificationService : IVerificationService
    {
        private readonly IRepository<User> _userRepository;
        private readonly IRepository<UserProfile> _profileRepository;
        private readonly IRepository<Verification> _verificationRepository;
        private readonly ILogger<VerificationService> _logger;
        private readonly IMapper _mapper;
        private readonly VerificationSettings _verificationSettings;

        public VerificationService(
            IRepository<User> userRepository,
            IRepository<UserProfile> profileRepository,
            IRepository<Verification> verificationRepository,
            ILogger<VerificationService> logger,
            IMapper mapper,
            IOptions<VerificationSettings> verificationSettings)
        {
            _userRepository = userRepository ?? throw new ArgumentNullException(nameof(userRepository));
            _profileRepository = profileRepository ?? throw new ArgumentNullException(nameof(profileRepository));
            _verificationRepository = verificationRepository ?? throw new ArgumentNullException(nameof(verificationRepository));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
            _verificationSettings = verificationSettings?.Value ?? throw new ArgumentNullException(nameof(verificationSettings));
        }

        public async Task<VerificationDto> GetVerificationStatus(string userId)
        {
            try
            {
                var user = await _userRepository.Query()
                    .FirstOrDefaultAsync(u => u.Auth0Id == userId);

                if (user == null)
                {
                    throw new KeyNotFoundException($"User not found with ID: {userId}");
                }

                // Get most recent verification
                var verification = await _verificationRepository.Query()
                    .Where(v => v.UserId == user.UserId)
                    .OrderByDescending(v => v.InitiatedAt)
                    .FirstOrDefaultAsync();

                if (verification == null)
                {
                    // Return a default verification status
                    return new VerificationDto
                    {
                        UserId = userId,
                        Status = "Unverified"
                    };
                }

                return _mapper.Map<VerificationDto>(verification);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error getting verification status for user {userId}");
                throw;
            }
        }

        public async Task<VerificationDto> InitiateVerification(VerificationRequestDto requestDto)
        {
            try
            {
                var user = await _userRepository.Query()
                    .Include(u => u.Profile)
                    .FirstOrDefaultAsync(u => u.Auth0Id == requestDto.UserId);

                if (user == null)
                {
                    throw new KeyNotFoundException($"User not found with ID: {requestDto.UserId}");
                }

                // Check if there's already a pending verification
                var pendingVerification = await _verificationRepository.Query()
                    .Where(v => v.UserId == user.UserId && v.Status == "Pending")
                    .OrderByDescending(v => v.InitiatedAt)
                    .FirstOrDefaultAsync();

                if (pendingVerification != null)
                {
                    // Return the existing pending verification
                    return _mapper.Map<VerificationDto>(pendingVerification);
                }

                // Validate verification provider
                if (!string.IsNullOrEmpty(requestDto.VerificationProvider))
                {
                    var providerExists = _verificationSettings.Providers.ContainsKey(requestDto.VerificationProvider);
                    if (!providerExists)
                    {
                        throw new InvalidOperationException($"Verification provider not supported: {requestDto.VerificationProvider}");
                    }
                }

                // Create new verification request
                var verification = new Verification
                {
                    VerificationId = Guid.NewGuid(),
                    UserId = user.UserId,
                    VerificationType = requestDto.VerificationType,
                    VerificationProvider = requestDto.VerificationProvider,
                    Status = "Pending",
                    InitiatedAt = DateTime.UtcNow,
                    ExpiresAt = DateTime.UtcNow.AddDays(7) // Default to 7 days expiration
                };

                // Store additional data if provided
                if (requestDto.VerificationData != null)
                {
                    verification.VerificationData = JsonSerializer.Serialize(requestDto.VerificationData);
                }

                await _verificationRepository.AddAsync(verification);
                await _verificationRepository.SaveChangesAsync();

                // Update user profile verification status
                if (user.Profile != null)
                {
                    user.Profile.VerificationStatus = "Pending";
                    user.Profile.UpdatedAt = DateTime.UtcNow;
                    await _profileRepository.UpdateAsync(user.Profile);
                    await _profileRepository.SaveChangesAsync();
                }

                return _mapper.Map<VerificationDto>(verification);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error initiating verification for user {requestDto.UserId}");
                throw;
            }
        }

        public async Task<VerificationDto> ProcessVerificationCallback(string provider, object callbackData)
        {
            try
            {
                // Validate provider
                if (!_verificationSettings.Providers.ContainsKey(provider))
                {
                    throw new InvalidOperationException($"Verification provider not supported: {provider}");
                }

                // Parse the callback data to get user and verification information
                // This will be specific to each provider's API
                var verificationId = GetVerificationIdFromCallback(provider, callbackData);
                var isApproved = GetVerificationResultFromCallback(provider, callbackData);

                if (verificationId == Guid.Empty)
                {
                    throw new InvalidOperationException("Could not determine verification ID from callback data");
                }

                // Get the verification record
                var verification = await _verificationRepository.GetByIdAsync(verificationId);
                if (verification == null)
                {
                    throw new KeyNotFoundException($"Verification not found with ID: {verificationId}");
                }

                // Update verification status
                verification.Status = isApproved ? "Completed" : "Failed";
                verification.CompletedAt = DateTime.UtcNow;

                // Store callback data
                verification.VerificationData = JsonSerializer.Serialize(callbackData);

                await _verificationRepository.UpdateAsync(verification);
                await _verificationRepository.SaveChangesAsync();

                // Update user and profile if verification was successful
                if (isApproved)
                {
                    var user = await _userRepository.Query()
                        .Include(u => u.Profile)
                        .FirstOrDefaultAsync(u => u.UserId == verification.UserId);

                    if (user != null)
                    {
                        user.IsVerified = true;

                        if (user.Profile != null)
                        {
                            user.Profile.VerificationStatus = "Verified";
                            user.Profile.VerificationCompletedAt = DateTime.UtcNow;
                            user.Profile.UpdatedAt = DateTime.UtcNow;
                            await _profileRepository.UpdateAsync(user.Profile);
                        }

                        await _userRepository.UpdateAsync(user);
                        await _userRepository.SaveChangesAsync();
                    }
                }

                return _mapper.Map<VerificationDto>(verification);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error processing verification callback from provider {provider}");
                throw;
            }
        }

        public async Task<VerificationDto> CompleteVerification(Guid verificationId, bool approved, string adminUserId, string notes = null)
        {
            try
            {
                var adminUser = await _userRepository.Query()
                    .FirstOrDefaultAsync(u => u.Auth0Id == adminUserId);

                if (adminUser == null)
                {
                    throw new KeyNotFoundException($"Admin user not found with ID: {adminUserId}");
                }

                if (!adminUser.IsAdmin)
                {
                    throw new UnauthorizedAccessException("Only administrators can manually complete verifications");
                }

                var verification = await _verificationRepository.GetByIdAsync(verificationId);
                if (verification == null)
                {
                    throw new KeyNotFoundException($"Verification not found with ID: {verificationId}");
                }

                // Update verification status
                verification.Status = approved ? "Completed" : "Failed";
                verification.CompletedAt = DateTime.UtcNow;

                // Store admin notes if provided
                if (!string.IsNullOrEmpty(notes))
                {
                    var notesObject = new { AdminNotes = notes, AdminId = adminUser.UserId };
                    var existingData = string.IsNullOrEmpty(verification.VerificationData) 
                        ? new Dictionary<string, object>() 
                        : JsonSerializer.Deserialize<Dictionary<string, object>>(verification.VerificationData);
                    
                    existingData["adminAction"] = notesObject;
                    verification.VerificationData = JsonSerializer.Serialize(existingData);
                }

                await _verificationRepository.UpdateAsync(verification);
                await _verificationRepository.SaveChangesAsync();

                // Update user and profile if verification was successful
                if (approved)
                {
                    var user = await _userRepository.Query()
                        .Include(u => u.Profile)
                        .FirstOrDefaultAsync(u => u.UserId == verification.UserId);

                    if (user != null)
                    {
                        user.IsVerified = true;

                        if (user.Profile != null)
                        {
                            user.Profile.VerificationStatus = "Verified";
                            user.Profile.VerificationCompletedAt = DateTime.UtcNow;
                            user.Profile.UpdatedAt = DateTime.UtcNow;
                            await _profileRepository.UpdateAsync(user.Profile);
                        }

                        await _userRepository.UpdateAsync(user);
                        await _userRepository.SaveChangesAsync();
                    }
                }

                return _mapper.Map<VerificationDto>(verification);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error completing verification {verificationId} by admin {adminUserId}");
                throw;
            }
        }

        public async Task<PaginatedResponse<AdminVerificationDto>> GetPendingVerifications(int page = 1, int pageSize = 10)
        {
            try
            {
                var query = _verificationRepository.Query()
                    .Include(v => v.User)
                    .Where(v => v.Status == "Pending")
                    .OrderByDescending(v => v.InitiatedAt);

                var totalItems = await query.CountAsync();

                var verifications = await query
                    .Skip((page - 1) * pageSize)
                    .Take(pageSize)
                    .ToListAsync();

                var verificationDtos = _mapper.Map<List<AdminVerificationDto>>(verifications);

                return new PaginatedResponse<AdminVerificationDto>
                {
                    Success = true,
                    Items = verificationDtos,
                    Page = page,
                    PageSize = pageSize,
                    TotalItems = totalItems,
                    TotalPages = (int)Math.Ceiling(totalItems / (double)pageSize)
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting pending verifications");
                throw;
            }
        }

        public async Task<PaginatedResponse<AdminVerificationDto>> GetVerifications(string status, int page = 1, int pageSize = 10)
        {
            try
            {
                var query = _verificationRepository.Query()
                    .Include(v => v.User)
                    .AsQueryable();

                // Filter by status if provided
                if (!string.IsNullOrEmpty(status))
                {
                    query = query.Where(v => v.Status == status);
                }

                query = query.OrderByDescending(v => v.InitiatedAt);

                var totalItems = await query.CountAsync();

                var verifications = await query
                    .Skip((page - 1) * pageSize)
                    .Take(pageSize)
                    .ToListAsync();

                var verificationDtos = _mapper.Map<List<AdminVerificationDto>>(verifications);

                return new PaginatedResponse<AdminVerificationDto>
                {
                    Success = true,
                    Items = verificationDtos,
                    Page = page,
                    PageSize = pageSize,
                    TotalItems = totalItems,
                    TotalPages = (int)Math.Ceiling(totalItems / (double)pageSize)
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error getting verifications with status {status}");
                throw;
            }
        }

        public async Task<AdminVerificationDto> GetVerificationById(Guid verificationId)
        {
            try
            {
                var verification = await _verificationRepository.Query()
                    .Include(v => v.User)
                    .FirstOrDefaultAsync(v => v.VerificationId == verificationId);

                if (verification == null)
                {
                    throw new KeyNotFoundException($"Verification not found with ID: {verificationId}");
                }

                return _mapper.Map<AdminVerificationDto>(verification);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error getting verification with ID {verificationId}");
                throw;
            }
        }

        // Helper methods to parse callback data from providers
        private Guid GetVerificationIdFromCallback(string provider, object callbackData)
        {
            // This would be implemented based on each provider's API
            // For example, for CLEAR:
            if (provider == "Clear" && callbackData is JsonElement element)
            {
                if (element.TryGetProperty("verification_id", out var idElement) && 
                    idElement.ValueKind == JsonValueKind.String &&
                    Guid.TryParse(idElement.GetString(), out var innerId))
                {
                    return innerId;
                }
            }
            
            // For demo purposes, you might extract from a string or dictionary
            if (callbackData is Dictionary<string, object> dict && 
                dict.TryGetValue("verificationId", out var idObj) &&
                idObj is string idStr &&
                Guid.TryParse(idStr, out var id))
            {
                return id;
            }
            
            return Guid.Empty;
        }

        private bool GetVerificationResultFromCallback(string provider, object callbackData)
        {
            // This would be implemented based on each provider's API
            // For example, for CLEAR:
            if (provider == "Clear" && callbackData is JsonElement element)
            {
                if (element.TryGetProperty("result", out var resultElement) && 
                    resultElement.ValueKind == JsonValueKind.String)
                {
                    return resultElement.GetString() == "approved";
                }
            }
            
            // For demo purposes
            if (callbackData is Dictionary<string, object> dict && 
                dict.TryGetValue("result", out var resultObj) &&
                resultObj is string resultStr)
            {
                return resultStr.ToLower() == "approved" || resultStr.ToLower() == "true";
            }
            
            return false;
        }
    }

    // Class to hold verification settings from configuration
    public class VerificationSettings
    {
        public Dictionary<string, ProviderSettings> Providers { get; set; } = new Dictionary<string, ProviderSettings>();
    }

    public class ProviderSettings
    {
        public string ApiKey { get; set; }
        public string ApiUrl { get; set; }
    }
}