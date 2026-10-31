using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;

namespace BookifyHotel.Model
{
    public class RoomType
    {
        private string? _description;
        private int? _capacity;

        public int RoomTypeId { get; set; }
        public string Name { get; set; } = string.Empty;
        public decimal PricePerNight { get; set; }

        [NotMapped]
        public string Description
        {
            get => !string.IsNullOrWhiteSpace(_description) ? _description : GetDefaultDescription(Name);
            set => _description = value;
        }

        [NotMapped]
        public int Capacity
        {
            get => _capacity ?? GetDefaultCapacity(Name);
            set => _capacity = value;
        }

        public virtual ICollection<Room> Rooms { get; set; } = new List<Room>();

        private static string GetDefaultDescription(string? name)
        {
            var lower = name?.ToLowerInvariant() ?? string.Empty;
            if (lower.Contains("royal") || lower.Contains("presidential"))
                return "Our signature top-floor sanctuary with a private panoramic terrace, soaking jacuzzi, bespoke lounge, and 24/7 butler service.";
            if (lower.Contains("family"))
                return "Generous multi-space retreat overlooking private gardens, featuring dual queen beds, lounge seating, and family-friendly amenities.";
            if (lower.Contains("suite") || lower.Contains("executive"))
                return "Refined luxury suite with a dedicated living parlor, artisanal minibar, Nespresso station, and sweeping skyline vistas.";
            if (lower.Contains("deluxe") || lower.Contains("ocean") || lower.Contains("sea"))
                return "Elevated coastal elegance featuring a private balcony, plush king bedding, Italian marble bath, and curated lighting.";
            return "Thoughtfully crafted haven with bespoke furnishings, soundproof floor-to-ceiling windows, rainfall shower, and ultra-fast Wi-Fi.";
        }

        private static int GetDefaultCapacity(string? name)
        {
            var lower = name?.ToLowerInvariant() ?? string.Empty;
            if (lower.Contains("family") || lower.Contains("presidential")) return 4;
            if (lower.Contains("royal") || lower.Contains("suite") || lower.Contains("executive")) return 3;
            if (lower.Contains("single")) return 1;
            return 2;
        }
    }
}
