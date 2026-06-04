using System;
using System.Collections.Generic;

namespace CSI402_Final.Models.DB;

public partial class ProductCategory
{
    public string CategoryId { get; set; } = null!;

    public string? CategoryName { get; set; }

    public virtual ICollection<Product> Products { get; set; } = new List<Product>();
}
