namespace HMS.Core.Entities.RoomModuleEntities
{
    public class RoomImage : BaseEntity<int>
    {
        public string ImageUrl { get; set; } = default!;
        public int RoomId { get; set; }
    }
}
