using Microsoft.AspNetCore.Mvc;
using CSI402_Final.Models.DB;
using CSI402_Final.Models.ViewModels;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;

public class AuthController : Controller
{
    private readonly Csi402FinalContext _db;

    public AuthController(Csi402FinalContext db)
    {
        _db = db;
    }


    [HttpGet]
    public IActionResult Login()
    {
        return View();
    }

    [HttpPost]
    public IActionResult Login(LoginViewModel model)
    {
        if (string.IsNullOrEmpty(model.Email) || string.IsNullOrEmpty(model.Password))
        {
            ViewBag.Error = "Please fill all fields";
            return View();
        }

        var user = _db.Users
            .FirstOrDefault(u => u.Email == model.Email && u.Password == model.Password);

        if (user == null)
        {
            ViewBag.Error = "Email or Password incorrect";
            return View();
        }

        HttpContext.Session.SetString("UserID", user.UserId ?? "");
        HttpContext.Session.SetString("Email", user.Email ?? "");
        HttpContext.Session.SetString("Role", user.Role ?? "");

        return RedirectToAction("Index", "Home");
    }


    [HttpGet]
    public IActionResult Register()
    {
        return View();
    }

    [HttpPost]
    public IActionResult Register(User model)
    {
        model.UserId = Guid.NewGuid().ToString();
        model.Role = "member";
        model.CreatedAt = DateTime.Now;

        ModelState.Remove("UserId");
        ModelState.Remove("Role");
        ModelState.Remove("CreatedAt");

        if (!ModelState.IsValid)
            return View(model);

        if (_db.Users.Any(u => u.Email == model.Email))
        {
            ModelState.AddModelError("Email", "Email already exists");
            return View(model);
        }

        try
        {
            _db.Users.Add(model);
            _db.SaveChanges();

            HttpContext.Session.SetString("UserID", model.UserId);
            HttpContext.Session.SetString("Email", model.Email ?? "");
            HttpContext.Session.SetString("Role", model.Role ?? "");

            return RedirectToAction("Index", "Home");
        }
        catch (DbUpdateException ex)
        {
            if (ex.InnerException?.Message.Contains("Duplicate entry") == true)
                ModelState.AddModelError("Email", "Email already exists");
            else
                ModelState.AddModelError("", "Database error occurred");
        }
        catch (Exception)
        {
            ModelState.AddModelError("", "Something went wrong");
        }

        return View(model);
    }


    public IActionResult Logout()
    {
        HttpContext.Session.Clear();
        return RedirectToAction("Login");
    }
}