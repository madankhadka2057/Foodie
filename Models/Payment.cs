using System;
using System.Collections.Generic;

namespace Foodie.Models;

public partial class Payment
{
    public int PaymentId { get; set; }

    public string? Address { get; set; }

    public string? PaymentMode { get; set; }

    public decimal TotalAmount { get; set; }

    public string? PaymentStatus { get; set; }

    public int? OrderId { get; set; }

    public virtual Order? Order { get; set; }
}
