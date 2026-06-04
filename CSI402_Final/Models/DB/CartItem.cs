using System;
using System.Collections.Generic;

namespace CSI402_Final.Models.DB;

public partial class CartItem
{
    public string CartItemId { get; set; } = null!;

    public string? CartId { get; set; }

    public string? ProductOptionId { get; set; }

    public int? CartItemAmount { get; set; }

    public virtual Cart? Cart { get; set; }

    public virtual ProductOption? ProductOption { get; set; }
}
