using CSI402_Final.Models.DB;
using CSI402_Final.Models.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace CSI402_Final.Controllers
{
    public class OrderController : Controller
    {
        private readonly Csi402FinalContext _db;

        public OrderController(Csi402FinalContext db)
        {
            _db = db;
        }

        private string? GetUserId() => HttpContext.Session.GetString("UserID");

        private async Task<List<CartItem>> GetActiveCartItemsAsync(string userId)
        {
            var cart = await _db.Carts
                .FirstOrDefaultAsync(c => c.UserId == userId && c.CartStatus == "ACTIVE");

            if (cart == null) return new List<CartItem>();

            return await _db.CartItems
                .Include(ci => ci.ProductOption)
                    .ThenInclude(o => o!.Product)
                .Where(ci => ci.CartId == cart.CartId)
                .ToListAsync();
        }

        private (decimal subtotal, decimal shipping) CalcTotals(List<CartItem> items)
        {
            decimal subtotal = items.Sum(i =>
                (i.ProductOption?.ProductPrice ?? 0) * (i.CartItemAmount ?? 0));
            decimal shipping = subtotal >= CheckoutViewModel.FreeShippingThreshold
                ? 0m : CheckoutViewModel.BaseShippingFee;
            return (subtotal, shipping);
        }

        private (decimal discountAmount, string message, bool freeShip) CalcCouponDiscount(
            DiscountCoupon coupon, List<CartItem> items, decimal subtotal, decimal shipping)
        {
            switch (coupon.DiscountType?.ToUpper())
            {
                case "PERCENT":
                {
                    var amt = Math.Round(subtotal * (coupon.DiscountAmount ?? 0) / 100, 2);
                    return (amt, $"ลด {coupon.DiscountAmount:0}% — คุณประหยัด {amt:N0} ฿", false);
                }
                case "FIXED":
                {
                    var amt = Math.Min(coupon.DiscountAmount ?? 0, subtotal);
                    return (amt, $"ลดราคา {amt:N0} ฿", false);
                }
                case "FREESHIP":
                {
                    var amt = shipping;
                    return (amt, amt > 0
                        ? $"ฟรีค่าจัดส่ง — ประหยัด {amt:N0} ฿"
                        : "ฟรีค่าจัดส่ง (ออเดอร์นี้ได้ฟรีอยู่แล้ว)", true);
                }
                case "BOGO":
                {
                    var cheapest = items
                        .OrderBy(i => i.ProductOption?.ProductPrice ?? 0)
                        .FirstOrDefault();
                    var amt = cheapest?.ProductOption?.ProductPrice ?? 0;
                    var name = cheapest?.ProductOption?.Product?.ProductName ?? "item";
                    return (amt, $"ซื้อ 1 แถม 1 — {name} ฟรี 1 ชิ้น (ประหยัด {amt:N0} ฿)", false);
                }
                case "MINSPEND":
                {
                    decimal minRequired = coupon.DiscountPrice ?? 0;
                    if (subtotal < minRequired)
                        return (0, $"ต้องซื้อครบ {minRequired:N0} ฿ (ยังขาดอยู่ {(minRequired - subtotal):N0} ฿)", false);
                    var amt = coupon.DiscountAmount ?? 0;
                    return (amt, $"ซื้อครบ {minRequired:N0} ฿ ลดทันที {amt:N0} ฿", false);
                }
                default:
                    return (0, "ประเภทคูปองไม่รู้จัก", false);
            }
        }

        public async Task<IActionResult> Checkout()
        {
            var userId = GetUserId();
            if (userId == null) return RedirectToAction("Login", "Auth");

            var items = await GetActiveCartItemsAsync(userId);
            if (!items.Any()) return RedirectToAction("Index", "Cart");

            var (subtotal, shipping) = CalcTotals(items);

            var savedAddr = await _db.Addresses.FirstOrDefaultAsync(a => a.UserId == userId);

            var vm = new CheckoutViewModel
            {
                CartItems   = items,
                Subtotal    = subtotal,
                ShippingFee = shipping,
                FullName    = savedAddr?.FullName    ?? "",
                Phone       = savedAddr?.Phone       ?? "",
                PostalCode  = savedAddr?.PostalCode  ?? "",
                Province    = savedAddr?.Province    ?? "",
                District    = savedAddr?.District    ?? "",
                AddressLine = savedAddr?.AddressLine ?? ""
            };

            return View(vm);
        }

        [HttpPost]
        public async Task<IActionResult> ApplyCoupon(
            string couponCode,
            string fullName, string phone,
            string postalCode, string province,
            string district, string addressLine)
        {
            var userId = GetUserId();
            if (userId == null) return RedirectToAction("Login", "Auth");

            var items = await GetActiveCartItemsAsync(userId);
            var (subtotal, shipping) = CalcTotals(items);

            var vm = new CheckoutViewModel
            {
                CartItems   = items,
                Subtotal    = subtotal,
                ShippingFee = shipping,
                CouponCode  = couponCode,
                FullName    = fullName,
                Phone       = phone,
                PostalCode  = postalCode,
                Province    = province,
                District    = district,
                AddressLine = addressLine
            };

            var coupon = await _db.DiscountCoupons
                .Include(c => c.Users)
                .FirstOrDefaultAsync(c =>
                    c.DiscountCode == couponCode.ToUpper() &&
                    c.DiscountLeft > 0 &&
                    c.IsActive == true);

            if (coupon == null)
            {
                vm.CouponValid   = false;
                vm.CouponMessage = "รหัสคูปองไม่ถูกต้องหรือหมดอายุแล้ว";
            }
            else if (coupon.Users.Any(u => u.UserId == userId))
            {
                vm.CouponValid   = false;
                vm.CouponMessage = "คุณเคยใช้คูปองนี้ไปแล้ว";
            }
            else
            {
                var (discAmt, msg, freeShip) = CalcCouponDiscount(coupon, items, subtotal, shipping);

                if (coupon.DiscountType?.ToUpper() == "MINSPEND" && discAmt == 0)
                {
                    vm.CouponValid   = false;
                    vm.CouponMessage = msg;
                }
                else
                {
                    vm.CouponValid      = true;
                    vm.DiscountAmount   = discAmt;
                    vm.IsFreeShipping   = freeShip;
                    vm.CouponMessage    = msg;

                    if (freeShip) vm.ShippingFee = 0;

                    TempData["CouponCode"]      = coupon.DiscountCode;
                    TempData["DiscountAmount"]  = vm.DiscountAmount.ToString();
                    TempData["IsFreeShipping"]  = freeShip.ToString();
                }
            }

            return View("Checkout", vm);
        }

        [HttpPost]
        public async Task<IActionResult> PlaceOrder(
            string fullName, string phone,
            string postalCode, string province,
            string district, string addressLine,
            string? couponCode, decimal discountAmount, bool isFreeShipping)
        {
            var userId = GetUserId();
            if (userId == null) return RedirectToAction("Login", "Auth");

            var items = await GetActiveCartItemsAsync(userId);
            if (!items.Any()) return RedirectToAction("Index", "Cart");

            var (subtotal, shipping) = CalcTotals(items);

            if (isFreeShipping) shipping = 0;

            decimal grandTotal = subtotal + shipping - discountAmount;
            if (grandTotal < 0) grandTotal = 0;

            var addr = await _db.Addresses.FirstOrDefaultAsync(a => a.UserId == userId);
            if (addr == null)
            {
                addr = new Address { AddressId = Guid.NewGuid().ToString(), UserId = userId };
                _db.Addresses.Add(addr);
            }
            addr.FullName    = fullName;
            addr.Phone       = phone;
            addr.PostalCode  = postalCode;
            addr.Province    = province;
            addr.District    = district;
            addr.AddressLine = addressLine;

            var shippingSnapshot = $"{addressLine}, {district}, {province} {postalCode}";

            var order = new Order
            {
                OrderId         = Guid.NewGuid().ToString(),
                UserId          = userId,
                Status          = "PENDING",
                TotalPrice      = grandTotal,
                CreatedAt       = DateTime.Now,
                AddressId       = addr.AddressId,
                ShippingName    = fullName,
                ShippingPhone   = phone,
                ShippingAddress = shippingSnapshot
            };
            _db.Orders.Add(order);

            foreach (var ci in items)
            {
                _db.OrdersItems.Add(new OrdersItem
                {
                    OrderItemId     = Guid.NewGuid().ToString(),
                    OrderId         = order.OrderId,
                    ProductOptionId = ci.ProductOptionId!,
                    Amount          = ci.CartItemAmount ?? 1,
                    Price           = (ci.ProductOption?.ProductPrice ?? 0) * (ci.CartItemAmount ?? 1)
                });
            }

            if (!string.IsNullOrEmpty(couponCode))
            {
                var coupon = await _db.DiscountCoupons
                    .Include(c => c.Users)
                    .FirstOrDefaultAsync(c => c.DiscountCode == couponCode);

                if (coupon != null && coupon.DiscountLeft > 0)
                {
                    coupon.DiscountLeft -= 1;
                    var user = await _db.Users.FindAsync(userId);
                    if (user != null) coupon.Users.Add(user);
                }
            }

            var cart = await _db.Carts
                .FirstOrDefaultAsync(c => c.UserId == userId && c.CartStatus == "ACTIVE");
            if (cart != null) cart.CartStatus = "COMPLETED";

            await _db.SaveChangesAsync();

            TempData["OrderId"] = order.OrderId;
            return RedirectToAction("OrderSuccess");
        }

        public async Task<IActionResult> OrderSuccess()
        {
            var orderId = TempData["OrderId"]?.ToString();
            if (string.IsNullOrEmpty(orderId)) return RedirectToAction("Index", "Home");

            var order = await _db.Orders
                .Include(o => o.OrdersItems)
                    .ThenInclude(oi => oi.ProductOption)
                        .ThenInclude(po => po!.Product)
                .FirstOrDefaultAsync(o => o.OrderId == orderId);

            if (order == null) return RedirectToAction("Index", "Home");
            return View(order);
        }

        public async Task<IActionResult> Track(string? orderId)
        {
            var userId = GetUserId();
            if (userId == null) return RedirectToAction("Login", "Auth");

            var query = _db.Orders
                .Include(o => o.OrdersItems)
                    .ThenInclude(oi => oi.ProductOption)
                        .ThenInclude(po => po!.Product)
                .Where(o => o.UserId == userId)
                .OrderByDescending(o => o.CreatedAt);

            if (!string.IsNullOrEmpty(orderId))
            {
                query = query.Where(o => o.OrderId == orderId)
                             .OrderByDescending(o => o.CreatedAt);
            }

            return View(await query.ToListAsync());
        }
    }
}
