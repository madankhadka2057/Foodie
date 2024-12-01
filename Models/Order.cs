using System;
using System.Collections.Generic;

namespace Foodie.Models;

public partial class Order
{
    public int? ProductId { get; set; }

    public int? UserId { get; set; }

    public string? Status { get; set; }

    public DateTime? OrderDate { get; set; }

    public int OrderId { get; set; }

    public string Address { get; set; } = null!;

    public virtual ICollection<OrderDetail> OrderDetails { get; set; } = new List<OrderDetail>();

    public virtual ICollection<Payment> Payments { get; set; } = new List<Payment>();

    public virtual Product? Product { get; set; }

    public virtual User? User { get; set; }
}
