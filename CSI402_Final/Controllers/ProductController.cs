using Microsoft.AspNetCore.Mvc;
using CSI402_Final.Models.DB;
using Microsoft.EntityFrameworkCore;
using System.Linq;

public class ProductController : Controller
{
    private readonly Csi402FinalContext _db;

    public ProductController(Csi402FinalContext db)
    {
        _db = db;
    }

    public IActionResult Index(string category = "all")
    {
        var products = _db.Products
            .Include(p => p.ProductOptions)
            .Include(p => p.Category)
            .Include(p => p.ProductCoffeeDetail)   
            .Include(p => p.ProductMatchaDetail)   
            .Include(p => p.ProductEquipmentDetail)  
            .AsQueryable();

        if (category != "all")
        {
            products = products.Where(p => p.Category.CategoryName == category);
        }

        var result = products.ToList();

    var coffeeRoast = _db.ProductCoffeeDetails
        .Select(x => x.RoastLevel)
        .Distinct()
        .ToList();

    var matchaGrade = _db.ProductMatchaDetails
        .Select(x => x.Grade)
        .Distinct()
        .ToList();

    var equipmentMaterial = _db.ProductEquipmentDetails
        .Select(x => x.Material)
        .Distinct()
        .ToList();

    ViewBag.FilterOptions = coffeeRoast
        .Concat(matchaGrade)
        .Concat(equipmentMaterial)
        .Where(x => !string.IsNullOrEmpty(x))
        .Distinct()
        .ToList();
        ViewBag.Category = category;

        return View(result);
    }

    public IActionResult Detail(string id)
    {
        var product = _db.Products
            .Include(p => p.ProductOptions)
            .Include(p => p.ProductCoffeeDetail)   
            .Include(p => p.ProductMatchaDetail)     
            .Include(p => p.ProductEquipmentDetail)   
            .FirstOrDefault(p => p.ProductId == id);

        if (product == null)
        {
            return NotFound();
        }

        return View(product);
    }
}