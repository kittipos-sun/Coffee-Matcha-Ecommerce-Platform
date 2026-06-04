using System;
using System.Collections.Generic;

namespace CSI402_Final.Models.DB;

public partial class Address
{
    public string AddressId { get; set; } = null!;

    public string? UserId { get; set; }

    public string? FullName { get; set; }

    public string? Phone { get; set; }

    public string? AddressLine { get; set; }

    public string? District { get; set; }

    public string? Province { get; set; }

    public string? PostalCode { get; set; }

    public virtual ICollection<Order> Orders { get; set; } = new List<Order>();

    public virtual User? User { get; set; }
}
