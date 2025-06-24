using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

[Authorize]
public class ChatController : Controller
{
    private readonly UserManager<IdentityUser> _userManager;

    public ChatController(UserManager<IdentityUser> userManager)
    {
        _userManager = userManager;
    }

    public IActionResult Chat(string receiverId)
    {
        ViewBag.ReceiverId = receiverId;
        return View();
    }

}
