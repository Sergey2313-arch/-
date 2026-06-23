namespace ReviMarket.Web.Models;

public static class UserRoles
{
    public const string Customer = "Customer";
    public const string Creator = "Creator";
    public const string Admin = "Admin";
    public const string Manager = "Manager";
    public const string Owner = "Owner";
    public const string CoOwner = "CoOwner";
    public const string Moderator = "Moderator";
    public const string Lawyer = "Lawyer";
    public const string SupportAgent = "SupportAgent";

    public static readonly string[] All =
    {
        Customer,
        Creator,
        Admin,
        Manager,
        Owner,
        CoOwner,
        Moderator,
        Lawyer,
        SupportAgent
    };
}
