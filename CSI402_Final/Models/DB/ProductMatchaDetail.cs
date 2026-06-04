using System;
using System.Collections.Generic;

namespace CSI402_Final.Models.DB;

public partial class ProductMatchaDetail
{
    public string ProductId { get; set; } = null!;

    public string? Grade { get; set; }

    public string? Origin { get; set; }

    public string? ColorLevel { get; set; }

    public virtual Product Product { get; set; } = null!;
}
