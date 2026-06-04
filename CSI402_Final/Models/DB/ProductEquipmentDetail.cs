using System;
using System.Collections.Generic;

namespace CSI402_Final.Models.DB;

public partial class ProductEquipmentDetail
{
    public string ProductId { get; set; } = null!;

    public string? Material { get; set; }

    public string? EquipmentType { get; set; }

    public string? Brand { get; set; }

    public virtual Product Product { get; set; } = null!;
}
