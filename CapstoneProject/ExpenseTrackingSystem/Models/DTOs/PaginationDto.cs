namespace ExpenseTrackingSystem.Models.DTOs
{
    public class PaginationQuery
    {
        public int PageNumber { get; set; } = 1; // default to page 1
        public int PageSize { get; set; } = 10;  // default page size
    }  
}

