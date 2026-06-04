using System;
using System.Collections.Generic;

namespace CSI402_Final.Models.DB;

public partial class OrdersItem
{
    public string OrderItemId { get; set; } = null!;

    public string? OrderId { get; set; }

    public string? ProductOptionId { get; set; }

    public int? Amount { get; set; }

    public decimal? Price { get; set; }

    public virtual Order? Order { get; set; }

    public virtual ProductOption? ProductOption { get; set; }
}
