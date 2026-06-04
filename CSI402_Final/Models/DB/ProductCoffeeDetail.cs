using System;
using System.Collections.Generic;

namespace CSI402_Final.Models.DB;

public partial class ProductCoffeeDetail
{
    public string ProductId { get; set; } = null!;

    public string? RoastLevel { get; set; }

    public string? FlavorNote { get; set; }

    public string? Origin { get; set; }

    public string? Process { get; set; }

    public virtual Product Product { get; set; } = null!;
}
