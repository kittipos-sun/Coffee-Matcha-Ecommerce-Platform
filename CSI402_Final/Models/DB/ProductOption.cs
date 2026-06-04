using System;
using System.Collections.Generic;

namespace CSI402_Final.Models.DB;

public partial class ProductOption
{
    public string ProductOptionId { get; set; } = null!;

    public string? ProductId { get; set; }

    public decimal? ProductPrice { get; set; }

    public int? ProductAmount { get; set; }

    public string? ProductUnit { get; set; }

    public int? ProductStock { get; set; }

    public virtual ICollection<CartItem> CartItems { get; set; } = new List<CartItem>();

    public virtual ICollection<OrdersItem> OrdersItems { get; set; } = new List<OrdersItem>();

    public virtual Product? Product { get; set; }
}
