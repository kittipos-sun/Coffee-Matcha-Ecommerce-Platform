using System;
using System.Collections.Generic;

namespace CSI402_Final.Models.DB;

public partial class Order
{
    public string OrderId { get; set; } = null!;

    public string? UserId { get; set; }

    public string? AddressId { get; set; }

    public decimal? TotalPrice { get; set; }

    public string? Status { get; set; }

    public DateTime? CreatedAt { get; set; }

    public string? ShippingName { get; set; }

    public string? ShippingPhone { get; set; }

    public string? ShippingAddress { get; set; }

    public virtual Address? Address { get; set; }

    public virtual ICollection<OrdersItem> OrdersItems { get; set; } = new List<OrdersItem>();

    public virtual User? User { get; set; }
}
