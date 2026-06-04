using System;
using System.Collections.Generic;

namespace CSI402_Final.Models.DB;

public partial class User
{
    public string UserId { get; set; } = null!;

    public string? Email { get; set; }

    public string? Phone { get; set; }

    public string? Password { get; set; }

    public string? Role { get; set; }

    public DateTime? CreatedAt { get; set; }

    public virtual ICollection<Address> Addresses { get; set; } = new List<Address>();

    public virtual ICollection<Cart> Carts { get; set; } = new List<Cart>();

    public virtual ICollection<Order> Orders { get; set; } = new List<Order>();

    public virtual ICollection<DiscountCoupon> Discounts { get; set; } = new List<DiscountCoupon>();
}
