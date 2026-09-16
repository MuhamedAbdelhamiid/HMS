namespace HMS.Core.Entities.RoomModule
{
    public class RoomImage : BaseEntity<int>
    {
        public string ImageUrl { get; set; } = default!;
        public int RoomId { get; set; }
    }
}
