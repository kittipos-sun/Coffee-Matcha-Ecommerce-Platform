using CSI402_Final.Models.DB;

namespace CSI402_Final.Models.ViewModels
{
    public class CheckoutViewModel
    {
        public const decimal FreeShippingThreshold = 1500m;
        public const decimal BaseShippingFee       = 100m;

        public List<CartItem> CartItems { get; set; } = new();

        public decimal Subtotal        { get; set; }
        public decimal ShippingFee     { get; set; }
        public decimal DiscountAmount  { get; set; }

        public decimal GrandTotal => Math.Max(0, Subtotal + ShippingFee - DiscountAmount);

        public string?  CouponCode    { get; set; }
        public bool     CouponValid   { get; set; }
        public string?  CouponMessage { get; set; }

        public bool IsFreeShipping { get; set; }

        public string FullName    { get; set; } = "";
        public string Phone       { get; set; } = "";
        public string PostalCode  { get; set; } = "";
        public string Province    { get; set; } = "";
        public string District    { get; set; } = "";
        public string AddressLine { get; set; } = "";
    }
}
