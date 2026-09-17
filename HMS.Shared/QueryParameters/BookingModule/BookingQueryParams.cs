namespace HMS.Shared.QueryParameters.BookingModule
{
    public class BookingQueryParams
    {
        public DateTime? CheckInDate { get; set; }
        public DateTime? CheckOutDate { get; set; }
        public string? BookingStatus { get; set; }
    }
}
