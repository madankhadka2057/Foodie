using Microsoft.AspNetCore.Mvc.Rendering;

namespace Foodie.Models
{
    //public class CategoryList
    //{
    //    public string Catagory { get; set; } = null!;
    //    public List<SelectListItem> selectCategories { get; set; } = null!;
    //}
    public class ProductEdit
    {
        public int ProductId { get; set; }

        public string Name { get; set; } = null!;

        public string Description { get; set; }= null!;

        public decimal Price { get; set; } 

        public int Quantity { get; set; }

        public string ImageUrl { get; set; } = null!;

        public int CategoryId { get; set; }

        public bool IsActive { get; set; }

        public DateTime CreatedDate { get; set; }
        public IFormFile foodImage {get; set; }=null!;
    }
}
