using System;
using System.Linq;
using System.Collections.Generic;
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

            // UserSport mappings - DTO to Domain
            CreateMap<UserSportDto, UserSport>()
                .ForMember(dest => dest.User, opt => opt.Ignore())
                .ForMember(dest => dest.Sport, opt => opt.Ignore())
                .ForMember(dest => dest.UpdatedAt, opt => opt.MapFrom(src => DateTime.UtcNow))
                .AfterMap((src, dest, context) =>
                {
                    // Ensure we have a UserSportId if creating a new entity
                    if (dest.UserSportId == Guid.Empty)
                    {
                        dest.UserSportId = src.UserSportId != Guid.Empty ? src.UserSportId : Guid.NewGuid();
                    }
                });

            // UserSport mappings
            CreateMap<UserSport, UserSportDto>()
                .ForMember(dest => dest.UserId, opt => opt.MapFrom(src => src.User.Auth0Id))
                .ForMember(dest => dest.SportName, opt => opt.MapFrom(src => src.Sport.Name))
                .ForMember(dest => dest.IconUrl, opt => opt.MapFrom(src => src.Sport.IconUrl));

            // Sport mappings
            CreateMap<Sport, SportDto>()
                .ForMember(dest => dest.CreatedBy, opt => opt.MapFrom(src =>
                    src.CreatedBy != null ? $"{src.CreatedBy.FirstName} {src.CreatedBy.LastName}" : null));

            // Sport Create DTO to Sport entity
            CreateMap<SportCreateDto, Sport>();

            // Sport Update DTO to Sport entity
            CreateMap<SportUpdateDto, Sport>();

            CreateMap<User, UserListItemDto>()
                .ForMember(dest => dest.Sports, opt => opt.MapFrom(src => src.Sports.Select(s => new SportBriefDto
                {
                    SportId = s.Sport.SportId,
                    Name = s.Sport.Name,
                    SkillLevel = s.SkillLevel
                })));
        }
    }
}