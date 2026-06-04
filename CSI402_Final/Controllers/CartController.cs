using CSI402_Final.Models.DB;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace CSI402_Final.Controllers
{
    public class CartController : Controller
    {
        private readonly Csi402FinalContext _db;

        public CartController(Csi402FinalContext db)
        {
            _db = db;
        }

        private string? GetUserId() => HttpContext.Session.GetString("UserID");

        private async Task<Cart> GetOrCreateCartAsync(string userId)
        {
            var cart = await _db.Carts
                .FirstOrDefaultAsync(c => c.UserId == userId && c.CartStatus == "ACTIVE");

            if (cart == null)
            {
                cart = new Cart
                {
                    CartId     = Guid.NewGuid().ToString(),
                    UserId     = userId,
                    CartStatus = "ACTIVE",
                    CreatedAt  = DateTime.Now
                };
                _db.Carts.Add(cart);
                await _db.SaveChangesAsync();
            }

            return cart;
        }

        public async Task<IActionResult> Index()
        {
            var userId = GetUserId();
            if (userId == null) return RedirectToAction("Login", "Auth");

            var cart = await _db.Carts
                .FirstOrDefaultAsync(c => c.UserId == userId && c.CartStatus == "ACTIVE");

            if (cart == null)
                return View(new List<CartItem>());

            var items = await _db.CartItems
                .Include(ci => ci.ProductOption)
                    .ThenInclude(o => o.Product)
                .Where(ci => ci.CartId == cart.CartId)
                .ToListAsync();

            return View(items);
        }

        [HttpPost]
        public async Task<IActionResult> AddToCart(string productOptionId, int quantity = 1)
        {
            var userId = GetUserId();
            if (userId == null) return RedirectToAction("Login", "Auth");

            if (quantity < 1) quantity = 1;

            var cart = await GetOrCreateCartAsync(userId);

            var existing = await _db.CartItems
                .FirstOrDefaultAsync(ci => ci.CartId == cart.CartId
                                        && ci.ProductOptionId == productOptionId);

            if (existing != null)
            {
                existing.CartItemAmount += quantity;
            }
            else
            {
                _db.CartItems.Add(new CartItem
                {
                    CartItemId      = Guid.NewGuid().ToString(),
                    CartId          = cart.CartId,
                    ProductOptionId = productOptionId,
                    CartItemAmount  = quantity
                });
            }

            await _db.SaveChangesAsync();
            return RedirectToAction("Index");
        }


        [HttpPost]
        public async Task<IActionResult> UpdateQuantity(string cartItemId, int quantity)
        {
            var userId = GetUserId();
            if (userId == null) return RedirectToAction("Login", "Auth");

            var cart = await _db.Carts
                .FirstOrDefaultAsync(c => c.UserId == userId && c.CartStatus == "ACTIVE");
            if (cart == null) return NotFound();

            var item = await _db.CartItems
                .FirstOrDefaultAsync(ci => ci.CartItemId == cartItemId
                                        && ci.CartId == cart.CartId);
            if (item == null) return NotFound();

            if (quantity < 1)
                _db.CartItems.Remove(item);
            else
                item.CartItemAmount = quantity;

            await _db.SaveChangesAsync();
            return RedirectToAction("Index");
        }

        [HttpPost]
        public async Task<IActionResult> Remove(string cartItemId)
        {
            var userId = GetUserId();
            if (userId == null) return RedirectToAction("Login", "Auth");

            var cart = await _db.Carts
                .FirstOrDefaultAsync(c => c.UserId == userId && c.CartStatus == "ACTIVE");
            if (cart == null) return NotFound();

            var item = await _db.CartItems
                .FirstOrDefaultAsync(ci => ci.CartItemId == cartItemId
                                        && ci.CartId == cart.CartId);
            if (item != null)
            {
                _db.CartItems.Remove(item);
                await _db.SaveChangesAsync();
            }

            return RedirectToAction("Index");
        }

        [HttpPost]
        public async Task<IActionResult> Clear()
        {
            var userId = GetUserId();
            if (userId == null) return RedirectToAction("Login", "Auth");

            var cart = await _db.Carts
                .FirstOrDefaultAsync(c => c.UserId == userId && c.CartStatus == "ACTIVE");
            if (cart == null) return RedirectToAction("Index");

            var items = _db.CartItems.Where(ci => ci.CartId == cart.CartId);
            _db.CartItems.RemoveRange(items);
            await _db.SaveChangesAsync();

            return RedirectToAction("Index");
        }
    }
}
