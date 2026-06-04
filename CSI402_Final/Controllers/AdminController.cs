using Microsoft.AspNetCore.Mvc;
using CSI402_Final.Models.DB;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Http;

public class AdminController : Controller
{
    private readonly Csi402FinalContext _db;
    private readonly IWebHostEnvironment _env;

    public AdminController(Csi402FinalContext db, IWebHostEnvironment env)
    {
        _db = db;
        _env = env;
    }

    private bool IsAdmin() => HttpContext.Session.GetString("Role") == "admin";
    private IActionResult NotAdmin() => RedirectToAction("Login", "Auth");


    private async Task<string?> SaveProductImageAsync(IFormFile? file, string? existingPath = null)
    {
        if (file == null || file.Length == 0) return existingPath;

        var allowed = new[] { ".jpg", ".jpeg", ".png", ".webp", ".gif" };
        var ext = Path.GetExtension(file.FileName).ToLowerInvariant();
        if (!allowed.Contains(ext)) return existingPath;

        var uploadDir = Path.Combine(_env.WebRootPath, "uploads", "products");
        Directory.CreateDirectory(uploadDir);

        if (!string.IsNullOrEmpty(existingPath))
        {
            var oldFile = Path.Combine(_env.WebRootPath, existingPath.TrimStart('/').Replace('/', Path.DirectorySeparatorChar));
            if (System.IO.File.Exists(oldFile)) System.IO.File.Delete(oldFile);
        }

        var fileName = Guid.NewGuid().ToString() + ext;
        var fullPath = Path.Combine(uploadDir, fileName);

        using (var stream = new FileStream(fullPath, FileMode.Create))
            await file.CopyToAsync(stream);

        return $"/uploads/products/{fileName}";
    }


    public IActionResult Dashboard()
    {
        if (!IsAdmin()) return NotAdmin();

        ViewBag.TotalUsers = _db.Users.Count();
        ViewBag.TotalProducts = _db.Products.Count();
        ViewBag.TotalOrders = _db.Orders.Count();
        ViewBag.TotalRevenue = _db.Orders
            .Where(o => o.Status == "COMPLETED")
            .Sum(o => (decimal?)o.TotalPrice) ?? 0;

        ViewBag.RecentOrders = _db.Orders
            .Include(o => o.User)
            .OrderByDescending(o => o.CreatedAt)
            .Take(6)
            .Select(o => new
            {
                o.OrderId,
                UserEmail = o.User != null ? o.User.Email : "—",
                o.Status,
                o.TotalPrice
            })
            .ToList();

        ViewBag.LowStock = _db.ProductOptions
            .Include(po => po.Product)
            .Where(po => po.ProductStock <= 10)
            .OrderBy(po => po.ProductStock)
            .Take(8)
            .Select(po => new
            {
                ProductName = po.Product != null ? po.Product.ProductName : "—",
                Unit = po.ProductUnit,
                Stock = po.ProductStock ?? 0
            })
            .ToList();

        return View();
    }


    public IActionResult Products()
    {
        if (!IsAdmin()) return NotAdmin();

        var products = _db.Products
            .Include(p => p.Category)
            .Include(p => p.ProductOptions)
            .OrderBy(p => p.ProductName)
            .ToList();

        ViewBag.Categories = _db.ProductCategories.OrderBy(c => c.CategoryName).ToList();
        return View(products);
    }

    public IActionResult ProductDetail(string id)
    {
        if (!IsAdmin()) return NotAdmin();

        var product = _db.Products
            .Include(p => p.Category)
            .Include(p => p.ProductOptions)
            .Include(p => p.ProductCoffeeDetail)
            .Include(p => p.ProductMatchaDetail)
            .Include(p => p.ProductEquipmentDetail)
            .FirstOrDefault(p => p.ProductId == id);

        if (product == null) return NotFound();
        return View(product);
    }

    [HttpPost]
    public async Task<IActionResult> AddProduct(
        string ProductName, string ProductType, string CategoryId,
        string? ProductDescription,
        IFormFile? ProductImageFile,
        string OptionUnit, decimal OptionPrice, int OptionStock, int OptionAmount,
        string? CoffeeOrigin, string? RoastLevel, string? FlavorNote, string? CoffeeProcess,
        string? MatchaGrade, string? MatchaOrigin, string? ColorLevel,
        string? Brand, string? EquipmentType, string? Material)
    {
        if (!IsAdmin()) return NotAdmin();

        string? imagePath = await SaveProductImageAsync(ProductImageFile);

        var product = new Product
        {
            ProductId = Guid.NewGuid().ToString(),
            ProductName = ProductName,
            ProductType = ProductType,
            CategoryId = CategoryId,
            ProductDescription = ProductDescription,
            ProductImage = imagePath
        };

        _db.Products.Add(product);

        switch (ProductType)
        {
            case "coffee":
                _db.ProductCoffeeDetails.Add(new ProductCoffeeDetail
                {
                    ProductId = product.ProductId,
                    Origin = CoffeeOrigin,
                    RoastLevel = RoastLevel,
                    FlavorNote = FlavorNote,
                    Process = CoffeeProcess
                });
                break;
            case "matcha":
                _db.ProductMatchaDetails.Add(new ProductMatchaDetail
                {
                    ProductId = product.ProductId,
                    Grade = MatchaGrade,
                    Origin = MatchaOrigin,
                    ColorLevel = ColorLevel
                });
                break;
            case "equipment":
                _db.ProductEquipmentDetails.Add(new ProductEquipmentDetail
                {
                    ProductId = product.ProductId,
                    Brand = Brand,
                    EquipmentType = EquipmentType,
                    Material = Material
                });
                break;
        }

        _db.ProductOptions.Add(new ProductOption
        {
            ProductOptionId = Guid.NewGuid().ToString(),
            ProductId = product.ProductId,
            ProductUnit = OptionUnit,
            ProductPrice = OptionPrice,
            ProductStock = OptionStock,
            ProductAmount = OptionAmount
        });

        _db.SaveChanges();
        TempData["Success"] = $"Product '{ProductName}' added successfully.";
        return RedirectToAction("Products");
    }

    [HttpPost]
    public async Task<IActionResult> EditProduct(
        string ProductId,
        string ProductName,
        string? ProductDescription,
        IFormFile? ProductImageFile,
        string ProductType,
        string? CoffeeOrigin, string? RoastLevel, string? FlavorNote, string? CoffeeProcess,
        string? MatchaGrade, string? MatchaOrigin, string? ColorLevel,
        string? Brand, string? EquipmentType, string? Material)
    {
        if (!IsAdmin()) return NotAdmin();

        var p = _db.Products
            .Include(x => x.ProductCoffeeDetail)
            .Include(x => x.ProductMatchaDetail)
            .Include(x => x.ProductEquipmentDetail)
            .FirstOrDefault(x => x.ProductId == ProductId);

        if (p == null) { TempData["Error"] = "Product not found."; return RedirectToAction("Products"); }

        p.ProductName = ProductName;
        p.ProductDescription = ProductDescription;
        p.ProductImage = await SaveProductImageAsync(ProductImageFile, p.ProductImage);

        switch (ProductType)
        {
            case "coffee":
                if (p.ProductCoffeeDetail != null)
                {
                    p.ProductCoffeeDetail.Origin = CoffeeOrigin;
                    p.ProductCoffeeDetail.RoastLevel = RoastLevel;
                    p.ProductCoffeeDetail.FlavorNote = FlavorNote;
                    p.ProductCoffeeDetail.Process = CoffeeProcess;
                }
                else
                    _db.ProductCoffeeDetails.Add(new ProductCoffeeDetail { ProductId = ProductId, Origin = CoffeeOrigin, RoastLevel = RoastLevel, FlavorNote = FlavorNote, Process = CoffeeProcess });
                break;
            case "matcha":
                if (p.ProductMatchaDetail != null)
                {
                    p.ProductMatchaDetail.Grade = MatchaGrade;
                    p.ProductMatchaDetail.Origin = MatchaOrigin;
                    p.ProductMatchaDetail.ColorLevel = ColorLevel;
                }
                else
                    _db.ProductMatchaDetails.Add(new ProductMatchaDetail { ProductId = ProductId, Grade = MatchaGrade, Origin = MatchaOrigin, ColorLevel = ColorLevel });
                break;
            case "equipment":
                if (p.ProductEquipmentDetail != null)
                {
                    p.ProductEquipmentDetail.Brand = Brand;
                    p.ProductEquipmentDetail.EquipmentType = EquipmentType;
                    p.ProductEquipmentDetail.Material = Material;
                }
                else
                    _db.ProductEquipmentDetails.Add(new ProductEquipmentDetail { ProductId = ProductId, Brand = Brand, EquipmentType = EquipmentType, Material = Material });
                break;
        }

        _db.SaveChanges();
        TempData["Success"] = "Product updated.";
        return RedirectToAction("ProductDetail", new { id = ProductId });
    }

    [HttpPost]
    public IActionResult DeleteProduct(string productId)
    {
        if (!IsAdmin()) return NotAdmin();

        var p = _db.Products
            .Include(x => x.ProductOptions)
            .Include(x => x.ProductCoffeeDetail)
            .Include(x => x.ProductMatchaDetail)
            .Include(x => x.ProductEquipmentDetail)
            .FirstOrDefault(x => x.ProductId == productId);

        if (p != null)
        {
            if (!string.IsNullOrEmpty(p.ProductImage) && p.ProductImage.StartsWith("/uploads/"))
            {
                var filePath = Path.Combine(_env.WebRootPath, p.ProductImage.TrimStart('/').Replace('/', Path.DirectorySeparatorChar));
                if (System.IO.File.Exists(filePath)) System.IO.File.Delete(filePath);
            }

            if (p.ProductCoffeeDetail != null) _db.ProductCoffeeDetails.Remove(p.ProductCoffeeDetail);
            if (p.ProductMatchaDetail != null) _db.ProductMatchaDetails.Remove(p.ProductMatchaDetail);
            if (p.ProductEquipmentDetail != null) _db.ProductEquipmentDetails.Remove(p.ProductEquipmentDetail);

            var optionIds = p.ProductOptions.Select(o => o.ProductOptionId).ToList();
            var cartItems = _db.CartItems.Where(ci => optionIds.Contains(ci.ProductOptionId)).ToList();
            _db.CartItems.RemoveRange(cartItems);

            _db.ProductOptions.RemoveRange(p.ProductOptions);
            _db.Products.Remove(p);
            _db.SaveChanges();
            TempData["Success"] = $"Product '{p.ProductName}' deleted.";
        }
        return RedirectToAction("Products");
    }


    [HttpPost]
    public IActionResult AddOption(string productId, string ProductUnit, decimal ProductPrice, int ProductStock, int ProductAmount)
    {
        if (!IsAdmin()) return NotAdmin();

        _db.ProductOptions.Add(new ProductOption
        {
            ProductOptionId = Guid.NewGuid().ToString(),
            ProductId = productId,
            ProductUnit = ProductUnit,
            ProductPrice = ProductPrice,
            ProductStock = ProductStock,
            ProductAmount = ProductAmount
        });
        _db.SaveChanges();
        TempData["Success"] = "Option added.";
        return RedirectToAction("ProductDetail", new { id = productId });
    }

    [HttpPost]
    public IActionResult EditOption(string ProductOptionId, string ProductId, string ProductUnit, decimal ProductPrice, int ProductStock, int ProductAmount)
    {
        if (!IsAdmin()) return NotAdmin();

        var opt = _db.ProductOptions.Find(ProductOptionId);
        if (opt != null)
        {
            opt.ProductUnit = ProductUnit;
            opt.ProductPrice = ProductPrice;
            opt.ProductStock = ProductStock;
            opt.ProductAmount = ProductAmount;
            _db.SaveChanges();
            TempData["Success"] = "Option updated.";
        }
        return RedirectToAction("ProductDetail", new { id = ProductId });
    }

    [HttpPost]
    public IActionResult DeleteOption(string optionId, string productId)
    {
        if (!IsAdmin()) return NotAdmin();

        var opt = _db.ProductOptions.Find(optionId);
        if (opt != null) { _db.ProductOptions.Remove(opt); _db.SaveChanges(); TempData["Success"] = "Option deleted."; }
        return RedirectToAction("ProductDetail", new { id = productId });
    }


    public IActionResult Users()
    {
        if (!IsAdmin()) return NotAdmin();
        return View(_db.Users.OrderByDescending(u => u.CreatedAt).ToList());
    }

    [HttpPost]
    public IActionResult ChangeRole(string userId, string newRole)
    {
        if (!IsAdmin()) return NotAdmin();
        if (userId == HttpContext.Session.GetString("UserID"))
        {
            TempData["Error"] = "You cannot change your own role.";
            return RedirectToAction("Users");
        }
        var user = _db.Users.Find(userId);
        if (user != null) { user.Role = newRole; _db.SaveChanges(); TempData["Success"] = $"Role changed to '{newRole}'."; }
        return RedirectToAction("Users");
    }

    [HttpPost]
    public IActionResult EditUser(string UserId, string? Email, string? Phone, string? NewPassword)
    {
        if (!IsAdmin()) return NotAdmin();

        var user = _db.Users.Find(UserId);
        if (user == null) { TempData["Error"] = "User not found."; return RedirectToAction("Users"); }

        if (!string.IsNullOrWhiteSpace(Email) && _db.Users.Any(u => u.Email == Email && u.UserId != UserId))
        {
            TempData["Error"] = "Email is already in use by another account.";
            return RedirectToAction("Users");
        }

        if (!string.IsNullOrWhiteSpace(Email)) user.Email = Email;
        if (!string.IsNullOrWhiteSpace(Phone)) user.Phone = Phone;
        if (!string.IsNullOrWhiteSpace(NewPassword)) user.Password = NewPassword;

        _db.SaveChanges();
        TempData["Success"] = $"User '{user.Email}' updated.";
        return RedirectToAction("Users");
    }

    [HttpPost]
    public IActionResult DeleteUser(string userId)
    {
        if (!IsAdmin()) return NotAdmin();
        if (userId == HttpContext.Session.GetString("UserID"))
        {
            TempData["Error"] = "You cannot delete your own account.";
            return RedirectToAction("Users");
        }
        var user = _db.Users.Find(userId);
        if (user != null) { _db.Users.Remove(user); _db.SaveChanges(); TempData["Success"] = "User deleted."; }
        return RedirectToAction("Users");
    }


    public IActionResult Orders(string? status)
    {
        if (!IsAdmin()) return NotAdmin();
        ViewBag.StatusFilter = status;

        var query = _db.Orders.Include(o => o.User).AsQueryable();
        if (!string.IsNullOrEmpty(status))
            query = query.Where(o => o.Status == status);

        return View(query.OrderByDescending(o => o.CreatedAt).ToList());
    }


    public IActionResult OrderDetail(string id)
    {
        if (!IsAdmin()) return NotAdmin();

        var order = _db.Orders
            .Include(o => o.User)
            .Include(o => o.OrdersItems)
                .ThenInclude(oi => oi.ProductOption)
                    .ThenInclude(po => po!.Product)
                        .ThenInclude(p => p!.Category)
            .FirstOrDefault(o => o.OrderId == id);

        if (order == null) return NotFound();
        return View(order);
    }


    [HttpPost]
    public IActionResult UpdateOrderStatus(string orderId, string status)
    {
        if (!IsAdmin()) return NotAdmin();
        var order = _db.Orders.Find(orderId);
        if (order != null) { order.Status = status; _db.SaveChanges(); TempData["Success"] = $"Order status updated to '{status}'."; }
        return RedirectToAction("OrderDetail", new { id = orderId });
    }


    public IActionResult Stock()
    {
        if (!IsAdmin()) return NotAdmin();
        return View(_db.ProductOptions
            .Include(o => o.Product)
            .OrderBy(o => o.ProductStock)
            .ToList());
    }

    [HttpPost]
    public IActionResult UpdateStock(string optionId, int stock)
    {
        if (!IsAdmin()) return NotAdmin();
        var opt = _db.ProductOptions.Include(o => o.Product).FirstOrDefault(o => o.ProductOptionId == optionId);
        if (opt != null)
        {
            opt.ProductStock = stock;
            _db.SaveChanges();
            TempData["Success"] = $"Stock for '{opt.Product?.ProductName} ({opt.ProductUnit})' updated to {stock}.";
        }
        return RedirectToAction("Stock");
    }


    public IActionResult Coupons()
    {
        if (!IsAdmin()) return NotAdmin();

        var coupons = _db.DiscountCoupons
            .Include(c => c.Users)
            .OrderByDescending(c => c.IsActive)
            .ThenBy(c => c.DiscountCode)
            .ToList();

        return View(coupons);
    }


    [HttpPost]
    public IActionResult AddCoupon(
        string DiscountName,
        string DiscountCode,
        string DiscountType,
        decimal? DiscountPrice,
        decimal? DiscountAmount,
        int DiscountLeft,
        bool IsActive)
    {
        if (!IsAdmin()) return NotAdmin();

        if (_db.DiscountCoupons.Any(c => c.DiscountCode == DiscountCode.ToUpper()))
        {
            TempData["Error"] = $"รหัสคูปอง '{DiscountCode.ToUpper()}' ถูกใช้แล้ว";
            return RedirectToAction("Coupons");
        }

        string? err = ValidateCouponFields(DiscountType, DiscountPrice, DiscountAmount);
        if (err != null) { TempData["Error"] = err; return RedirectToAction("Coupons"); }

        _db.DiscountCoupons.Add(new DiscountCoupon
        {
            DiscountId = Guid.NewGuid().ToString(),
            DiscountName = DiscountName,
            DiscountCode = DiscountCode.ToUpper().Trim(),
            DiscountType = DiscountType.ToUpper(),
            DiscountPrice = DiscountPrice,
            DiscountAmount = DiscountAmount,
            DiscountLeft = DiscountLeft,
            IsActive = IsActive
        });

        _db.SaveChanges();
        TempData["Success"] = $"คูปอง '{DiscountCode.ToUpper()}' ถูกสร้างแล้ว";
        return RedirectToAction("Coupons");
    }


    [HttpPost]
    public IActionResult EditCoupon(
        string DiscountId,
        string DiscountName,
        string DiscountCode,
        string DiscountType,
        decimal? DiscountPrice,
        decimal? DiscountAmount,
        int DiscountLeft,
        bool IsActive)
    {
        if (!IsAdmin()) return NotAdmin();

        var coupon = _db.DiscountCoupons.Find(DiscountId);
        if (coupon == null) { TempData["Error"] = "ไม่พบคูปอง"; return RedirectToAction("Coupons"); }

        if (_db.DiscountCoupons.Any(c => c.DiscountCode == DiscountCode.ToUpper() && c.DiscountId != DiscountId))
        {
            TempData["Error"] = $"รหัสคูปอง '{DiscountCode.ToUpper()}' ถูกใช้แล้ว";
            return RedirectToAction("Coupons");
        }

        string? err = ValidateCouponFields(DiscountType, DiscountPrice, DiscountAmount);
        if (err != null) { TempData["Error"] = err; return RedirectToAction("Coupons"); }

        coupon.DiscountName = DiscountName;
        coupon.DiscountCode = DiscountCode.ToUpper().Trim();
        coupon.DiscountType = DiscountType.ToUpper();
        coupon.DiscountPrice = DiscountPrice;
        coupon.DiscountAmount = DiscountAmount;
        coupon.DiscountLeft = DiscountLeft;
        coupon.IsActive = IsActive;

        _db.SaveChanges();
        TempData["Success"] = $"อัปเดตคูปอง '{coupon.DiscountCode}' แล้ว";
        return RedirectToAction("Coupons");
    }


    [HttpPost]
    public IActionResult ToggleCoupon(string discountId)
    {
        if (!IsAdmin()) return NotAdmin();

        var coupon = _db.DiscountCoupons.Find(discountId);
        if (coupon != null)
        {
            coupon.IsActive = !(coupon.IsActive ?? false);
            _db.SaveChanges();
            TempData["Success"] = $"คูปอง '{coupon.DiscountCode}' {(coupon.IsActive == true ? "เปิด" : "ปิด")}ใช้งานแล้ว";
        }
        return RedirectToAction("Coupons");
    }


    [HttpPost]
    public IActionResult DeleteCoupon(string discountId)
    {
        if (!IsAdmin()) return NotAdmin();

        var coupon = _db.DiscountCoupons
            .Include(c => c.Users)
            .FirstOrDefault(c => c.DiscountId == discountId);

        if (coupon != null)
        {
            coupon.Users.Clear();
            _db.SaveChanges();

            _db.DiscountCoupons.Remove(coupon);
            _db.SaveChanges();
            TempData["Success"] = $"ลบคูปอง '{coupon.DiscountCode}' แล้ว";
        }
        return RedirectToAction("Coupons");
    }

    private static string? ValidateCouponFields(string type, decimal? price, decimal? amount)
    {
        switch (type.ToUpper())
        {
            case "PERCENT" when amount is null || amount <= 0 || amount > 100:
                return "PERCENT ต้องระบุ % ระหว่าง 1–100";
            case "FIXED" when amount is null || amount <= 0:
                return "FIXED ต้องระบุจำนวน ฿ ที่ลด";
            case "MINSPEND" when price is null || price <= 0:
                return "MINSPEND ต้องระบุยอดขั้นต่ำ";
            case "MINSPEND" when amount is null || amount <= 0:
                return "MINSPEND ต้องระบุจำนวน ฿ ที่ลด";
            default:
                return null;
        }
    }
}
