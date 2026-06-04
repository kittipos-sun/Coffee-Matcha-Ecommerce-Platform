using System;
using System.Collections.Generic;

namespace CSI402_Final.Models.DB;

public partial class DiscountCoupon
{
    public string DiscountId { get; set; } = null!;

    public string? DiscountName { get; set; }

    public string? DiscountCode { get; set; }

    public string? DiscountType { get; set; }

    public decimal? DiscountPrice { get; set; }

    public decimal? DiscountAmount { get; set; }

    public int? DiscountLeft { get; set; }

    public bool? IsActive { get; set; }

    public virtual ICollection<User> Users { get; set; } = new List<User>();
}
