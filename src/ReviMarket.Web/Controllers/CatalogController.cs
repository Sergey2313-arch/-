using Microsoft.AspNetCore.Mvc;

namespace ReviMarket.Web.Controllers;

public class CatalogController : Controller
{
    public IActionResult Index() => RedirectToAction("Index", "Orders");
    public IActionResult Details(int id) => RedirectToAction("Details", "Orders", new { id });
    public IActionResult Create() => RedirectToAction("Create", "Orders");
}
