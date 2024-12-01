namespace Foodie.Models
{
    public class PaymentEdit
    {
        public int PaymentId { get; set; }

        public string? Address { get; set; }

        public string? PaymentMode { get; set; }

        public decimal TotalAmount { get; set; }
        public int OrderId { get; set; }
        public string EncOrderId { get; set; } = null!;
    }
}
