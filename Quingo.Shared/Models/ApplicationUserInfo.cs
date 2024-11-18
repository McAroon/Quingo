namespace Quingo.Shared.Models;

public class ApplicationUserInfo(string userId, string userName)
{
    public string UserId { get; set; } = userId;
    public string UserName { get; set; } = userName;
}