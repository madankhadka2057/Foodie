using System;
using System.Collections.Generic;

namespace Foodie.Models;

public partial class Category
{
    public int CategoryId { get; set; }

    public string? Name { get; set; }

    public DateTime? CreatedDate { get; set; }

    public virtual ICollection<Product> Products { get; set; } = new List<Product>();
}
