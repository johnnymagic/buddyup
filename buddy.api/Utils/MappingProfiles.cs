using System;
using System.Linq;
using AutoMapper;
using BuddyUp.API.Models.Domain;
using BuddyUp.API.Models.DTOs;
using NetTopologySuite.Geometries;

namespace BuddyUp.API.Utils
{
    /// <summary>
    /// AutoMapper profiles for mapping between domain entities and DTOs
    /// </summary>
    public class MappingProfiles : Profile
    {
        public MappingProfiles()
        {
            // User mappings
            CreateMap<User, UserDto>()
                .ForMember(dest => dest.ProfilePictureUrl, opt => opt.MapFrom(src => src.Profile != null ? src.Profile.ProfilePictureUrl : null))
                .ForMember(dest => dest.Bio, opt => opt.MapFrom(src => src.Profile != null ? src.Profile.Bio : null))
                .ForMember(dest => dest.Sports, opt => opt.MapFrom(src => src.Sports));

            CreateMap<UserDto, User>()
                .ForMember(dest => dest.Profile, opt => opt.Ignore())
                .ForMember(dest => dest.Sports, opt => opt.Ignore())
                .ForMember(dest => dest.SentMatchRequests, opt => opt.Ignore())
                .ForMember(dest => dest.ReceivedMatchRequests, opt => opt.Ignore())
                .ForMember(dest => dest.SentMessages, opt => opt.Ignore())
                .ForMember(dest => dest.ReportsSubmitted, opt => opt.Ignore())
                .ForMember(dest => dest.ReportsReceived, opt => opt.Ignore());

            CreateMap<User, UserListItemDto>()
                .ForMember(dest => dest.ProfilePictureUrl, opt => opt.MapFrom(src => src.Profile != null ? src.Profile.ProfilePictureUrl : null))
                .ForMember(dest => dest.Sports, opt => opt.MapFrom(src => src.Sports.Select(s => new SportBriefDto
                {
                    SportId = s.SportId,
                    Name = s.Sport.Name,
                    SkillLevel = s.SkillLevel
                })));

            // Profile mappings
            CreateMap<UserProfile, ProfileDto>()
                .ForMember(dest => dest.UserId, opt => opt.MapFrom(src => src.User.Auth0Id))
                .ForMember(dest => dest.FirstName, opt => opt.MapFrom(src => src.User.FirstName))
                .ForMember(dest => dest.LastName, opt => opt.MapFrom(src => src.User.LastName))
                .ForMember(dest => dest.Email, opt => opt.MapFrom(src => src.User.Email))
                .ForMember(dest => dest.Latitude, opt => opt.MapFrom(src => src.PreferredLocation != null ? src.PreferredLocation.Y : (double?)null))
                .ForMember(dest => dest.Longitude, opt => opt.MapFrom(src => src.PreferredLocation != null ? src.PreferredLocation.X : (double?)null))
               .ForMember(dest => dest.PreferredDays, opt => opt.MapFrom(src =>
                    src.PreferredDays ?? new List<string>()))
                .ForMember(dest => dest.PreferredTimes, opt => opt.MapFrom(src =>
                    src.PreferredTimes ?? new List<string>()));

            CreateMap<ProfileDto, UserProfile>()
                .ForMember(dest => dest.User, opt => opt.Ignore())
                .ForMember(dest => dest.UserId, opt => opt.Ignore()) // This should be set manually
                .ForMember(dest => dest.PreferredLocation, opt => opt.MapFrom(src =>
                    src.Latitude.HasValue && src.Longitude.HasValue
                        ? CreatePoint(src.Longitude.Value, src.Latitude.Value)
                        : null))
                .ForMember(dest => dest.PreferredDays, opt => opt.MapFrom(src =>
                    src.PreferredDays != null && src.PreferredDays.Any()
                        ? string.Join(",", src.PreferredDays)
                        : null))
                .ForMember(dest => dest.PreferredTimes, opt => opt.MapFrom(src =>
                    src.PreferredTimes != null && src.PreferredTimes.Any()
                        ? string.Join(",", src.PreferredTimes)
                        : null));

            // UserSport mappings
            CreateMap<UserSport, UserSportDto>()
                .ForMember(dest => dest.UserId, opt => opt.MapFrom(src => src.User.Auth0Id))
                .ForMember(dest => dest.SportName, opt => opt.MapFrom(src => src.Sport.Name))
                .ForMember(dest => dest.IconUrl, opt => opt.MapFrom(src => src.Sport.IconUrl));

            CreateMap<UserSportDto, UserSport>()
                .ForMember(dest => dest.User, opt => opt.Ignore())
                .ForMember(dest => dest.Sport, opt => opt.Ignore());

            // Sport mappings
            CreateMap<Sport, SportDto>()
                .ForMember(dest => dest.CreatedBy, opt => opt.MapFrom(src => src.CreatedBy != null ? src.CreatedBy.FirstName : null));

            CreateMap<SportDto, Sport>()
                .ForMember(dest => dest.CreatedBy, opt => opt.Ignore())
                .ForMember(dest => dest.UserSports, opt => opt.Ignore());

            CreateMap<SportCreateDto, Sport>()
                .ForMember(dest => dest.SportId, opt => opt.Ignore())
                .ForMember(dest => dest.CreatedAt, opt => opt.Ignore())
                .ForMember(dest => dest.CreatedByUserId, opt => opt.Ignore())
                .ForMember(dest => dest.CreatedBy, opt => opt.Ignore())
                .ForMember(dest => dest.UserSports, opt => opt.Ignore())
                .ForMember(dest => dest.IsActive, opt => opt.MapFrom(_ => true));

            // Location mappings
            CreateMap<Models.Domain.Location, LocationDto>()
                .ForMember(dest => dest.Latitude, opt => opt.MapFrom(src => src.Coordinates.Y))
                .ForMember(dest => dest.Longitude, opt => opt.MapFrom(src => src.Coordinates.X))
                .ForMember(dest => dest.CreatedBy, opt => opt.MapFrom(src => src.CreatedBy != null ? src.CreatedBy.FirstName : null));

            CreateMap<LocationDto, Models.Domain.Location>()
                .ForMember(dest => dest.Coordinates, opt => opt.MapFrom(src => CreatePoint(src.Longitude, src.Latitude)))
                .ForMember(dest => dest.CreatedBy, opt => opt.Ignore());

            CreateMap<LocationCreateDto, Models.Domain.Location>()
                .ForMember(dest => dest.LocationId, opt => opt.Ignore())
                .ForMember(dest => dest.Coordinates, opt => opt.MapFrom(src => CreatePoint(src.Longitude, src.Latitude)))
                .ForMember(dest => dest.CreatedAt, opt => opt.Ignore())
                .ForMember(dest => dest.CreatedByUserId, opt => opt.Ignore())
                .ForMember(dest => dest.CreatedBy, opt => opt.Ignore())
                .ForMember(dest => dest.IsActive, opt => opt.MapFrom(_ => true));

            // Match mappings
            CreateMap<Match, MatchDto>()
                .ForMember(dest => dest.RequesterFirstName, opt => opt.MapFrom(src => src.Requester.FirstName))
                .ForMember(dest => dest.RequesterProfilePictureUrl, opt => opt.MapFrom(src => src.Requester.Profile != null ? src.Requester.Profile.ProfilePictureUrl : null))
                .ForMember(dest => dest.RecipientFirstName, opt => opt.MapFrom(src => src.Recipient.FirstName))
                .ForMember(dest => dest.RecipientProfilePictureUrl, opt => opt.MapFrom(src => src.Recipient.Profile != null ? src.Recipient.Profile.ProfilePictureUrl : null))
                .ForMember(dest => dest.SportName, opt => opt.MapFrom(src => src.Sport != null ? src.Sport.Name : null));

            // Verification mappings
            CreateMap<Verification, VerificationDto>()
                .ForMember(dest => dest.UserId, opt => opt.MapFrom(src => src.User.Auth0Id));

            CreateMap<VerificationDto, Verification>()
                .ForMember(dest => dest.User, opt => opt.Ignore())
                .ForMember(dest => dest.VerificationData, opt => opt.Ignore());

            // Activity mappings
            CreateMap<Activity, ActivityDto>()
                .ForMember(dest => dest.CreatedBy, opt => opt.MapFrom(src => src.CreatedBy != null ? src.CreatedBy.FirstName : null));

            // Message mappings
            CreateMap<Message, MessageDto>();

            CreateMap<Conversation, ConversationDto>()
                .ForMember(dest => dest.OtherUserId, opt => opt.Ignore())
                .ForMember(dest => dest.OtherUser, opt => opt.Ignore())
                .ForMember(dest => dest.Sport, opt => opt.MapFrom(src => src.Match != null && src.Match.Sport != null ? src.Match.Sport.Name : null))
                .ForMember(dest => dest.LastMessage, opt => opt.MapFrom(src => src.Messages.OrderByDescending(m => m.SentAt).FirstOrDefault().Content))
                .ForMember(dest => dest.UnreadCount, opt => opt.Ignore());
        }

        /// <summary>
        /// Create a Point geometry from longitude and latitude
        /// </summary>
        private static Point CreatePoint(double longitude, double latitude)
        {
            return new Point(longitude, latitude) { SRID = 4326 };
        }
    }
}