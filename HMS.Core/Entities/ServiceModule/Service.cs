namespace HMS.Core.Entities.ServiceModule
{
    public class Service : BaseEntity<int>
    {
        public string Name { get; set; } = default!;
        public string Description { get; set; } = default!;
        public decimal Price { get; set; } = default!;
    }
}
