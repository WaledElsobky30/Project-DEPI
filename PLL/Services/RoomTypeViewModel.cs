namespace PLL.Services
{
    public class RoomTypeViewModel
    {
        public int RoomTypeId { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public decimal PricePerNight { get; set; } 
        public int Capacity { get; set; }
        public int Size { get; set; }
        public int AvailableRoomsCount { get; set; }
        public string ImageUrl { get; set; } = string.Empty;
    }
}