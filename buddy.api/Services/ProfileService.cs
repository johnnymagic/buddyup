
using AutoMapper;
using BuddyUp.API.Data.Repositories;
using BuddyUp.API.Models.Domain;
using BuddyUp.API.Data.Repositories;
using BuddyUp.API.Models.DTOs;
using BuddyUp.API.Services.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;




namespace BuddyUp.API.Services.Implementations
{

    
    public class ProfileService : IProfileService
    {
        private readonly IRepository<UserProfile> _profileRepository;
        private readonly IRepository<UserSport> _userSportRepository;
        private readonly IRepository<Sport> _sportRepository;
        private readonly IRepository<Verification> _verificationRepository;
        private readonly ILogger<ProfileService> _logger;
        private readonly IMapper _mapper;

        public ProfileService(
            IRepository<UserProfile> profileRepository,
            IRepository<UserSport> userSportRepository,
            IRepository<Sport> sportRepository,
            IRepository<Verification> verificationRepository,
            ILogger<ProfileService> logger,
            IMapper mapper)
        {
            _profileRepository = profileRepository ?? throw new ArgumentNullException(nameof(profileRepository));
            _userSportRepository = userSportRepository ?? throw new ArgumentNullException(nameof(userSportRepository));
            _sportRepository = sportRepository ?? throw new ArgumentNullException(nameof(sportRepository));
            _verificationRepository = verificationRepository ?? throw new ArgumentNullException(nameof(verificationRepository));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
        }

        public async Task<ProfileDto> GetProfileByUserId(string userId)
        {
            try
            {
                var profile = await _profileRepository.Query()
                    .Include(p => p.User)
                    .FirstOrDefaultAsync(p => p.User.Auth0Id == userId);

                if (profile == null)
                {
                    _logger.LogInformation($"Profile not found for user {userId}");
                    return null;
                }

                return _mapper.Map<ProfileDto>(profile);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error retrieving profile for user {userId}");
                throw;
            }
        }

        public async Task<ProfileDto> CreateProfile(ProfileDto profileDto)
        {
            try
            {
                var profile = _mapper.Map<UserProfile>(profileDto);
                
                // Set default values
                profile.ProfileId = Guid.NewGuid();
                profile.CreatedAt = DateTime.UtcNow;
                profile.UpdatedAt = DateTime.UtcNow;
                profile.VerificationStatus = "Unverified";
                
                await _profileRepository.AddAsync(profile);
                await _profileRepository.SaveChangesAsync();
                
                return _mapper.Map<ProfileDto>(profile);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error creating profile for user {profileDto.UserId}");
                throw;
            }
        }
        
        public async Task<ProfileDto> UpdateProfile(ProfileDto profileDto)
        {
            try
            {
                var existingProfile = await _profileRepository.GetByIdAsync(profileDto.ProfileId);
                if (existingProfile == null)
                {
                    _logger.LogWarning($"Profile not found for update: {profileDto.ProfileId}");
                    throw new KeyNotFoundException($"Profile with ID {profileDto.ProfileId} not found");
                }
                
                // Update properties (only what's allowed to be updated)
                existingProfile.Bio = profileDto.Bio;
                existingProfile.ProfilePictureUrl = profileDto.ProfilePictureUrl;
                existingProfile.MaxTravelDistance = profileDto.MaxTravelDistance;
                existingProfile.PreferredDays = profileDto.PreferredDays;
                existingProfile.PreferredTimes = profileDto.PreferredTimes;
                existingProfile.UpdatedAt = DateTime.UtcNow;
                
                // Update coordinates if provided
                if (profileDto.Latitude.HasValue && profileDto.Longitude.HasValue)
                {
                    existingProfile.PreferredLocation = 
                        NetTopologySuite.Geometries.GeometryFactory.Default.CreatePoint(
                            new NetTopologySuite.Geometries.Coordinate(
                                profileDto.Longitude.Value, 
                                profileDto.Latitude.Value
                            )
                        );
                }
                
                await _profileRepository.UpdateAsync(existingProfile);
                await _profileRepository.SaveChangesAsync();
                
                return _mapper.Map<ProfileDto>(existingProfile);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error updating profile {profileDto.ProfileId}");
                throw;
            }
        }
        
        public async Task<IEnumerable<UserSportDto>> GetUserSports(string userId)
        {
            try
            {
                var userSports = await _userSportRepository.Query()
                    .Include(us => us.Sport)
                    .Include(us => us.User)
                    .Where(us => us.User.Auth0Id == userId)
                    .ToListAsync();
                    
                return _mapper.Map<IEnumerable<UserSportDto>>(userSports);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error retrieving sports for user {userId}");
                throw;
            }
        }
        
        public async Task<UserSportDto> GetUserSportById(Guid userSportId)
        {
            try
            {
                var userSport = await _userSportRepository.Query()
                    .Include(us => us.Sport)
                    .Include(us => us.User)
                    .FirstOrDefaultAsync(us => us.UserSportId == userSportId);
                    
                if (userSport == null)
                {
                    return null;
                }
                
                return _mapper.Map<UserSportDto>(userSport);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error retrieving user sport with ID {userSportId}");
                throw;
            }
        }
        
        public async Task<UserSportDto> AddUserSport(UserSportDto userSportDto)
        {
            try
            {
                // Check if sport exists
                var sport = await _sportRepository.GetByIdAsync(userSportDto.SportId);
                if (sport == null)
                {
                    throw new KeyNotFoundException($"Sport with ID {userSportDto.SportId} not found");
                }
                
                // Check if user already has this sport
                var existingUserSport = await _userSportRepository.Query()
                    .Include(us => us.User)
                    .FirstOrDefaultAsync(us => 
                        us.SportId == userSportDto.SportId && 
                        us.User.Auth0Id == userSportDto.UserId);
                        
                if (existingUserSport != null)
                {
                    throw new InvalidOperationException($"User already has sport with ID {userSportDto.SportId}");
                }
                
                var userSport = _mapper.Map<UserSport>(userSportDto);
                userSport.UserSportId = Guid.NewGuid();
                userSport.CreatedAt = DateTime.UtcNow;
                userSport.UpdatedAt = DateTime.UtcNow;
                
                await _userSportRepository.AddAsync(userSport);
                await _userSportRepository.SaveChangesAsync();
                
                return _mapper.Map<UserSportDto>(userSport);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error adding sport {userSportDto.SportId} for user {userSportDto.UserId}");
                throw;
            }
        }
        
        public async Task<UserSportDto> UpdateUserSport(UserSportDto userSportDto)
        {
            try
            {
                var existingUserSport = await _userSportRepository.GetByIdAsync(userSportDto.UserSportId);
                if (existingUserSport == null)
                {
                    throw new KeyNotFoundException($"UserSport with ID {userSportDto.UserSportId} not found");
                }
                
                // Update only what's allowed
                existingUserSport.SkillLevel = userSportDto.SkillLevel;
                existingUserSport.YearsExperience = userSportDto.YearsExperience;
                existingUserSport.Notes = userSportDto.Notes;
                existingUserSport.IsPublic = userSportDto.IsPublic;
                existingUserSport.UpdatedAt = DateTime.UtcNow;
                
                await _userSportRepository.UpdateAsync(existingUserSport);
                await _userSportRepository.SaveChangesAsync();
                
                return _mapper.Map<UserSportDto>(existingUserSport);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error updating user sport {userSportDto.UserSportId}");
                throw;
            }
        }
        
        public async Task RemoveUserSport(Guid userSportId)
        {
            try
            {
                var userSport = await _userSportRepository.GetByIdAsync(userSportId);
                if (userSport == null)
                {
                    throw new KeyNotFoundException($"UserSport with ID {userSportId} not found");
                }
                
                await _userSportRepository.DeleteAsync(userSport);
                await _userSportRepository.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error removing user sport {userSportId}");
                throw;
            }
        }
        
        public async Task<VerificationDto> GetVerificationStatus(string userId)
        {
            try
            {
                var verification = await _verificationRepository.Query()
                    .Include(v => v.User)
                    .Where(v => v.User.Auth0Id == userId)
                    .OrderByDescending(v => v.InitiatedAt)
                    .FirstOrDefaultAsync();
                    
                return verification != null
                    ? _mapper.Map<VerificationDto>(verification) 
                    : new VerificationDto { 
                        Status = "Unverified", 
                        UserId = userId 
                      };
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
                // Check if there's an existing verification in progress
                var existingVerification = await _verificationRepository.Query()
                    .Include(v => v.User)
                    .Where(v => 
                        v.User.Auth0Id == requestDto.UserId && 
                        v.Status == "Pending")
                    .OrderByDescending(v => v.InitiatedAt)
                    .FirstOrDefaultAsync();
                
                if (existingVerification != null)
                {
                    throw new InvalidOperationException("There is already a verification in progress");
                }
                
                // Create new verification
                var verification = new Verification
                {
                    VerificationId = Guid.NewGuid(),
                    VerificationType = requestDto.VerificationType,
                    VerificationProvider = requestDto.VerificationProvider,
                    Status = "Pending",
                    InitiatedAt = DateTime.UtcNow,
                    ExpiresAt = DateTime.UtcNow.AddDays(7) // Typically expires in a week
                };
                
                // If additional data provided, store it
                if (requestDto.VerificationData != null)
                {
                    verification.VerificationData = System.Text.Json.JsonSerializer.Serialize(requestDto.VerificationData);
                }
                
                await _verificationRepository.AddAsync(verification);
                await _verificationRepository.SaveChangesAsync();
                
                return _mapper.Map<VerificationDto>(verification);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error initiating verification for user {requestDto.UserId}");
                throw;
            }
        }
    }
}