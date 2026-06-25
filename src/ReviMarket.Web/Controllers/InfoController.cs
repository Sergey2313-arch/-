using Microsoft.AspNetCore.Mvc;

namespace ReviMarket.Web.Controllers;

public class InfoController : Controller
{
    public IActionResult Privacy() => View();
    public IActionResult Rules() => View();
    public IActionResult Help() => View();
    public IActionResult About() => View();
    public IActionResult Contacts() => View();
}
