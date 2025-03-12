using System.Collections.Generic;

namespace BuddyUp.API.Models.Responses
{
    public class ApiResponse
    {
        public bool Success { get; set; }
        
        public string Message { get; set; }
        
        public object Data { get; set; }
        
        public Dictionary<string, string[]> Errors { get; set; }
    }
    
    public class PaginatedResponse<T> : ApiResponse
    {
        public IEnumerable<T> Items { get; set; }
        
        public int Page { get; set; }
        
        public int PageSize { get; set; }
        
        public int TotalItems { get; set; }
        
        public int TotalPages { get; set; }
    }
}