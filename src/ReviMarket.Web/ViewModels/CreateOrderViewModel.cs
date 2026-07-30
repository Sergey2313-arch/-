using System.ComponentModel.DataAnnotations;
using ReviMarket.Web.Models;

namespace ReviMarket.Web.ViewModels;

public class CreateOrderViewModel
{
    [Required(ErrorMessage = "Укажи название заказа")]
    [MaxLength(120)]
    public string Title { get; set; } = string.Empty;

    [Required(ErrorMessage = "Опиши задачу")]
    [MaxLength(600)]
    public string Description { get; set; } = string.Empty;

    [Range(100, 10_000_000, ErrorMessage = "Минимальный бюджет — 100 ₽")]
    public decimal Price { get; set; }

    [Required]
    [MaxLength(40)]
    public string Category { get; set; } = MarketCategories.Design;
}
