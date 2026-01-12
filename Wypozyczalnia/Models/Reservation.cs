using System;
using System.ComponentModel.DataAnnotations;

namespace Wypozyczalnia.Models
{
    public class Reservation
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Wybierz sprzęt")]
        public int EquipmentItemId { get; set; }
        public EquipmentItem? EquipmentItem { get; set; }

        [Required]
        public string ApplicationUserId { get; set; } = string.Empty;
        public ApplicationUser? ApplicationUser { get; set; }

        [Required, DataType(DataType.Date)]
        public DateTime From { get; set; }

        [Required, DataType(DataType.Date)]
        public DateTime To { get; set; }
    }
}
