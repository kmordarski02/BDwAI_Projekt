using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Wypozyczalnia.Models
{
    public class EquipmentItem
    {
        public int Id { get; set; }

        [Required, StringLength(150)]
        [Display(Name = "Nazwa")]
        public string Name { get; set; } = string.Empty;

        [Required, StringLength(50)]
        [Display(Name = "Sezon")]
        public string Season { get; set; } = string.Empty; 

        [Required, Range(0, 1000)]
        [Display(Name = "Ilość")]
        public int Quantity { get; set; }

        [Required]
        [Display(Name = "Płeć / Przeznaczenie")]
        public TargetGender TargetGender { get; set; }

        [StringLength(50)]
        [Display(Name = "Rozmiar")]
        public string Size { get; set; } = string.Empty; 

        [Required, Range(0, 10000), DataType(DataType.Currency)]
        [Display(Name = "Cena za godzinę")]
        public decimal PricePerHour { get; set; }

        [Required]
        [Display(Name = "Kategoria")]
        public int CategoryId { get; set; }
        public Category? Category { get; set; }

        public ICollection<Reservation> Reservations { get; set; } = new List<Reservation>();
    }
}
