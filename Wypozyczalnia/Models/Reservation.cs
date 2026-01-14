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

        [Required]
        [Display(Name = "Od")]
        public DateTime From { get; set; }

        [Required]
        [Display(Name = "Do")]
        public DateTime To { get; set; }

        [DataType(DataType.Currency)]
        public decimal TotalPrice { get; set; }

        [Display(Name = "Użytkownik zaznacza, że jest studentem i akceptuje regulamin")]
        public bool IsStudent { get; set; }

        [EmailAddress]
        [Display(Name = "Mail studencki")]
        public string? StudentEmail { get; set; }
    }
}
