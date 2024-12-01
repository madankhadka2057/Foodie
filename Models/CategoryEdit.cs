using System.ComponentModel.DataAnnotations;

namespace Foodie.Models
{
    public class CategoryEdit
    {
        public int CategoryId { get; set; }

        [Required(ErrorMessage ="Category name is required")]
        public string Name { get; set; } = null!;

        public DateTime? CreatedDate { get; set; }
    }
}
