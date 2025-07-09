namespace ExpenseTrackingSystem.Models
{
    public class ExpenseSearchModel
    {
        public string? Title { get; set; }
        public string? Category { get; set; }
        public Range<decimal>? amountRange { get; set; }
        public Range<DateTimeOffset>? dateRange { get; set; }
    }

    public class Range<T> where T : struct, IComparable<T>
    {
        public T? MinVal { get; set; }
        public T? MaxVal { get; set; }
    }
}
