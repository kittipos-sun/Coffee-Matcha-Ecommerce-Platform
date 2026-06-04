using Microsoft.AspNetCore.Mvc;
using CSI402_Final.Models.DB;
using Microsoft.EntityFrameworkCore;

public class HomeController : Controller
{
    private readonly Csi402FinalContext _db;

    public HomeController(Csi402FinalContext db)
    {
        _db = db;
    }

    public IActionResult Index()
    {
        var products = _db.Products
            .Include(p => p.ProductOptions)
            .Take(4)
            .ToList();
        return View(products);
    }
}