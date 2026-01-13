using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Wypozyczalnia.Models
{
    public class EquipmentItem
    {
        public int Id { get; set; }

        [Required, StringLength(150)]
        public string Name { get; set; } = string.Empty;

        [Required, StringLength(50)]
        public string Season { get; set; } = string.Empty; // "Summer", "Winter", "AllYear"

        [Required, Range(0, 1000)]
        public int Quantity { get; set; }

        [Required]
        public TargetGender TargetGender { get; set; }

        [StringLength(50)]
        public string Size { get; set; } = string.Empty; // "42", "L", "180cm"

        [Required, Range(0, 10000), DataType(DataType.Currency)]
        public decimal PricePerDay { get; set; }

        [Required]
        public int CategoryId { get; set; }
        public Category? Category { get; set; }

        public ICollection<Reservation> Reservations { get; set; } = new List<Reservation>();
    }
}
