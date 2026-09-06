namespace HMS.Shared.QueryParameters.RoomModule
{
    public class RoomQueryParameters
    {
        public string? RoomType { get; set; }
        public decimal? MinPrice { get; set; }
        public decimal? MaxPrice { get; set; }
        public string? Sort { get; set; }
    }
}
