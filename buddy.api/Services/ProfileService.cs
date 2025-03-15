
using AutoMapper;
using BuddyUp.API.Data.Repositories;
using BuddyUp.API.Models.Domain;
using BuddyUp.API.Data.Repositories;
using BuddyUp.API.Models.DTOs;
using BuddyUp.API.Services.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using NetTopologySuite.Geometries;




namespace BuddyUp.API.Services.Implementations
{


    public class ProfileService : IProfileService
    {
        private readonly IRepository<UserProfile> _profileRepository;
        private readonly IRepository<User> _userRepository;
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
            IRepository<User> userRepository,
            ILogger<ProfileService> logger,
            IMapper mapper)
        {
            _profileRepository = profileRepository ?? throw new ArgumentNullException(nameof(profileRepository));
            _userSportRepository = userSportRepository ?? throw new ArgumentNullException(nameof(userSportRepository));
            _userRepository = userRepository ?? throw new ArgumentNullException(nameof(userRepository));
            _sportRepository = sportRepository ?? throw new ArgumentNullException(nameof(sportRepository));
            _verificationRepository = verificationRepository ?? throw new ArgumentNullException(nameof(verificationRepository));
            _verificationRepository = verificationRepository ?? throw new ArgumentNullException(nameof(verificationRepository));
            _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task<ProfileDto> GetProfileByUserId(Guid userId)
        {
            try
            {
                var profile = await _profileRepository.Query()
                    .Include(p => p.User)
                    .FirstOrDefaultAsync(p => p.User.UserId == userId);

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

        // Fix for the GetByAuth0Id method in ProfileService.cs

        public async Task<ProfileDto> GetByAuth0Id(string auth0UserId)
        {
            try
            {
                // First, find the User with this Auth0 ID
                var user = await _userRepository.Query()
                    .FirstOrDefaultAsync(u => u.Auth0Id == auth0UserId);

                if (user == null)
                {
                    _logger.LogInformation($"User not found for auth0UserId {auth0UserId}");
                    return null;
                }

                // Then, find the profile associated with this user
                var profile = await _profileRepository.Query()
                    .FirstOrDefaultAsync(p => p.UserId == user.UserId);

                if (profile == null)
                {
                    _logger.LogInformation($"Profile not found for user with UserId {user.UserId} (Auth0ID: {auth0UserId})");
                    return null;
                }

                // Manually map to ProfileDto instead of using AutoMapper
                var profileDto = new ProfileDto
                {
                    ProfileId = profile.ProfileId,
                    UserId = user.UserId,
                    Auth0UserId = user.Auth0Id,
                    Email = user.Email,
                    FirstName = user.FirstName,
                    LastName = user.LastName,
                    Bio = profile.Bio,
                    ProfilePictureUrl = profile.ProfilePictureUrl,
                    MaxTravelDistance = profile.MaxTravelDistance,
                    PreferredDays = profile.PreferredDays ?? new List<string>(),
                    PreferredTimes = profile.PreferredTimes ?? new List<string>(),
                    Latitude = profile.PreferredLocation?.Y,
                    Longitude = profile.PreferredLocation?.X,
                    VerificationStatus = profile.VerificationStatus,
                    PublicProfile = profile.PublicProfile
                };

                return profileDto;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error retrieving profile for auth0UserId {auth0UserId}");
                throw;
            }
        }

        // Fix for the CreateProfile method in ProfileService.cs

        public async Task<ProfileDto> CreateProfile(ProfileDto profileDto)
        {
            try
            {
                // Find the user by Auth0 ID
                var user = await _userRepository.Query()
                    .FirstOrDefaultAsync(u => u.Auth0Id == profileDto.Auth0UserId);

                if (user == null)
                {
                    // We need to create a user first
                    _logger.LogInformation($"Creating new user with Auth0 ID {profileDto.Auth0UserId}");

                    user = new User
                    {
                        UserId = Guid.NewGuid(),
                        Auth0Id = profileDto.Auth0UserId,
                        Email = profileDto.Email,
                        FirstName = profileDto.FirstName,
                        LastName = profileDto.LastName,
                        CreatedAt = DateTime.UtcNow,
                        Active = true
                    };

                    await _userRepository.AddAsync(user);
                    await _userRepository.SaveChangesAsync();

                    _logger.LogInformation($"Created new user with ID {user.UserId} for Auth0 ID {profileDto.Auth0UserId}");
                }

                // Create profile directly without using AutoMapper to avoid mapping issues
                var profile = new UserProfile
                {
                    ProfileId = Guid.NewGuid(),
                    UserId = user.UserId,
                    Bio = profileDto.Bio ?? string.Empty,
                    ProfilePictureUrl = profileDto.ProfilePictureUrl ?? string.Empty,
                    MaxTravelDistance = profileDto.MaxTravelDistance,
                    PreferredDays = profileDto.PreferredDays ?? new List<string>(),
                    PreferredTimes = profileDto.PreferredTimes ?? new List<string>(),
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow,
                    VerificationStatus = "Unverified",
                    PublicProfile = profileDto.PublicProfile
                };

                // Create a default point at (0,0) if no location provided
                if (profileDto.Latitude.HasValue && profileDto.Longitude.HasValue)
                {
                    var geometryFactory = new GeometryFactory();
                    profile.PreferredLocation = geometryFactory.CreatePoint(
                        new Coordinate(profileDto.Longitude.Value, profileDto.Latitude.Value));
                    profile.PreferredLocation.SRID = 4326;
                }
                else
                {
                    var geometryFactory = new GeometryFactory();
                    profile.PreferredLocation = geometryFactory.CreatePoint(new Coordinate(0, 0));
                    profile.PreferredLocation.SRID = 4326;
                }

                await _profileRepository.AddAsync(profile);
                await _profileRepository.SaveChangesAsync();

                // Map data back to DTO manually to avoid mapping issues
                var resultDto = new ProfileDto
                {
                    ProfileId = profile.ProfileId,
                    UserId = user.UserId,
                    Auth0UserId = user.Auth0Id,
                    Email = user.Email,
                    FirstName = user.FirstName,
                    LastName = user.LastName,
                    Bio = profile.Bio,
                    ProfilePictureUrl = profile.ProfilePictureUrl,
                    MaxTravelDistance = profile.MaxTravelDistance,
                    PreferredDays = profile.PreferredDays ?? new List<string>(),
                    PreferredTimes = profile.PreferredTimes ?? new List<string>(),
                    Latitude = profile.PreferredLocation?.Y,
                    Longitude = profile.PreferredLocation?.X,
                    VerificationStatus = profile.VerificationStatus,
                    PublicProfile = profile.PublicProfile
                };

                _logger.LogInformation($"Successfully created profile {profile.ProfileId} for user with Auth0 ID {profileDto.Auth0UserId}");
                return resultDto;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error creating profile for user with Auth0 ID {profileDto.Auth0UserId}");
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

                // Get the user associated with this profile
                var user = await _userRepository.GetByIdAsync(existingProfile.UserId);
                if (user == null)
                {
                    _logger.LogWarning($"User not found for profile with ID: {profileDto.ProfileId}");
                    throw new KeyNotFoundException($"User not found for profile with ID {profileDto.ProfileId}");
                }

                // Update User table properties
                if (!string.IsNullOrEmpty(profileDto.FirstName))
                    user.FirstName = profileDto.FirstName;

                if (!string.IsNullOrEmpty(profileDto.LastName))
                    user.LastName = profileDto.LastName;

                if (!string.IsNullOrEmpty(profileDto.Email))
                    user.Email = profileDto.Email;

                await _userRepository.UpdateAsync(user);

                // Update UserProfile table properties
                existingProfile.Bio = profileDto.Bio ?? existingProfile.Bio;
                existingProfile.ProfilePictureUrl = profileDto.ProfilePictureUrl ?? existingProfile.ProfilePictureUrl;
                existingProfile.MaxTravelDistance = profileDto.MaxTravelDistance;
                existingProfile.PreferredDays = profileDto.PreferredDays ?? existingProfile.PreferredDays;
                existingProfile.PreferredTimes = profileDto.PreferredTimes ?? existingProfile.PreferredTimes;
                existingProfile.PublicProfile = profileDto.PublicProfile;
                existingProfile.UpdatedAt = DateTime.UtcNow;

                // Update coordinates if provided
                if (profileDto.Latitude.HasValue && profileDto.Longitude.HasValue)
                {
                    existingProfile.PreferredLocation =
                        new Point(profileDto.Longitude.Value, profileDto.Latitude.Value) { SRID = 4326 };
                }

                await _profileRepository.UpdateAsync(existingProfile);
                await _profileRepository.SaveChangesAsync();

                // Manually map the updated profile to DTO
                var resultDto = new ProfileDto
                {
                    ProfileId = existingProfile.ProfileId,
                    UserId = user.UserId,
                    Auth0UserId = user.Auth0Id,
                    Email = user.Email,
                    FirstName = user.FirstName,
                    LastName = user.LastName,
                    Bio = existingProfile.Bio,
                    ProfilePictureUrl = existingProfile.ProfilePictureUrl,
                    MaxTravelDistance = existingProfile.MaxTravelDistance,
                    PreferredDays = existingProfile.PreferredDays ?? new List<string>(),
                    PreferredTimes = existingProfile.PreferredTimes ?? new List<string>(),
                    Latitude = existingProfile.PreferredLocation?.Y,
                    Longitude = existingProfile.PreferredLocation?.X,
                    VerificationStatus = existingProfile.VerificationStatus,
                    PublicProfile = existingProfile.PublicProfile
                };

                return resultDto;
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
                    : new VerificationDto
                    {
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