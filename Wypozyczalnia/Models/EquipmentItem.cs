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
        public string Season { get; set; } = string.Empty;

        [Required, Range(0, 1000)]
        public int Quantity { get; set; }

        [Required]
        public int CategoryId { get; set; }
        public Category? Category { get; set; }

        public ICollection<Reservation> Reservations { get; set; } = new List<Reservation>();
    }
}
