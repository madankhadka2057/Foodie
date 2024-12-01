namespace Foodie.Models
{
    public class CartEdit
    {
        public int CartId { get; set; }

        public int ProductId { get; set; }

        public int? Quantity { get; set; } = null!;

        public int UserId { get; set; }

        public string ProductName { get; set; } = null!;
        public Decimal ProductPrice { get; set; }
        public string Description { get; set; } = null!;
        public string Category { get; set; } = null!;
        public decimal Price { get; set; }
        public string UserName {get; set ;}=null!;
        public string Address { get; set; } = null!;

        public string ImageUrl { get; set; }= null!;
    }
}
