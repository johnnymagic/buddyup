using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace BuddyUp.API.Models.DTOs
{
    public class DashboardStatsDto
    {
        public int TotalUsers { get; set; }
        
        public int NewUsersToday { get; set; }
        
        public int TotalSports { get; set; }
        
        public int ActiveSports { get; set; }
        
        public MatchStatsDto MatchStats { get; set; }
        
        public VerificationStatsDto VerificationStats { get; set; }
        
        public List<PeriodDataDto> UserGrowth { get; set; }
        
        public List<SportPopularityDto> SportPopularity { get; set; }
        
        public List<RecentActivityDto> RecentActivity { get; set; }
    }
    
    public class MatchStatsDto
    {
        public int TotalMatches { get; set; }
        
        public int PendingMatches { get; set; }
        
        public int AcceptedMatches { get; set; }
        
        public int RejectedMatches { get; set; }
    }
    
    public class VerificationStatsDto
    {
        public int TotalVerified { get; set; }
        
        public int PendingVerifications { get; set; }
        
        public double VerificationRate { get; set; }
    }
    
    public class PeriodDataDto
    {
        public string Period { get; set; }
        
        public int Count { get; set; }
    }
    
    public class SportPopularityDto
    {
        public string Name { get; set; }
        
        public int Count { get; set; }
    }
    
    public class RecentActivityDto
    {
        public string Type { get; set; } // user, match, verification
        
        public string Description { get; set; }
        
        public string Time { get; set; }
    }
    
    public class UserReportDto
    {
        public Guid ReportId { get; set; }
        
        public Guid ReportingUserId { get; set; }
        
        public string ReportingUserName { get; set; }
        
        public Guid ReportedUserId { get; set; }
        
        public string ReportedUserName { get; set; }
        
        public string Reason { get; set; }
        
        public string Status { get; set; }
        
        public DateTime CreatedAt { get; set; }
        
        public DateTime? ReviewedAt { get; set; }
        
        public string ReviewedBy { get; set; }
    }
    
    public class UserReportCreateDto
    {
        [Required]
        public Guid ReportedUserId { get; set; }
        
        [Required]
        public string Reason { get; set; }
    }
    
    public class UserReportUpdateDto
    {
        [Required]
        public string Status { get; set; }
        
        public string Notes { get; set; }
    }
    
    public class AdminVerificationDto : VerificationDto
    {
        public string UserEmail { get; set; }
        
        public string UserFirstName { get; set; }
        
        public string UserLastName { get; set; }
    }
    
    public class VerificationHandleDto
    {
        [Required]
        public bool Approved { get; set; }
        
        public string Notes { get; set; }
    }
    

}