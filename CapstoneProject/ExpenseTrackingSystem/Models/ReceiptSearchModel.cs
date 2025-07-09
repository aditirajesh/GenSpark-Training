namespace ExpenseTrackingSystem.Models
{
    public class ReceiptSearchModel
    {
        public string? ReceiptName { get; set; }
        public string? Category { get; set; }
        public DateRange? UploadDate { get; set; }
    }

    public class DateRange
    {
        public DateTimeOffset? MinDate { get; set; }
        public DateTimeOffset? MaxDate { get; set; }
    }
}
