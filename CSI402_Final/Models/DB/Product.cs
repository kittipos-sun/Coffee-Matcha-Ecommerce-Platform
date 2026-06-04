using System;
using System.Collections.Generic;

namespace CSI402_Final.Models.DB;

public partial class Product
{
    public string ProductId { get; set; } = null!;

    public string? CategoryId { get; set; }

    public string? ProductImage { get; set; }

    public string? ProductType { get; set; }

    public string? ProductName { get; set; }

    public string? ProductDescription { get; set; }

    public virtual ProductCategory? Category { get; set; }

    public virtual ProductCoffeeDetail? ProductCoffeeDetail { get; set; }

    public virtual ProductEquipmentDetail? ProductEquipmentDetail { get; set; }

    public virtual ProductMatchaDetail? ProductMatchaDetail { get; set; }

    public virtual ICollection<ProductOption> ProductOptions { get; set; } = new List<ProductOption>();
}
