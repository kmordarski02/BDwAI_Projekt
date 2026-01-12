using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Wypozyczalnia.Models
{
    public class Category
    {
        public int Id { get; set; }

        [Required, StringLength(100)]
        public string Name { get; set; } = string.Empty;

        
        public ICollection<EquipmentItem> EquipmentItems { get; set; } = new List<EquipmentItem>();
    }
}
