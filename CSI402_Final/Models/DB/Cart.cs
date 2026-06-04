using System;
using System.Collections.Generic;

namespace CSI402_Final.Models.DB;

public partial class Cart
{
    public string CartId { get; set; } = null!;

    public string? UserId { get; set; }

    public string? CartStatus { get; set; }

    public DateTime? CreatedAt { get; set; }

    public virtual ICollection<CartItem> CartItems { get; set; } = new List<CartItem>();

    public virtual User? User { get; set; }
}
