using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.Google;
using Microsoft.AspNetCore.Mvc;

namespace IdentityApi.Controllers;

[ApiController]
[Route("api/[controller]/v{version:apiVersion}")]
[Produces("application/json")]
[ApiExplorerSettings(GroupName = "v1")]
public class AccountController : Controller
{
    [HttpGet("login")]
    public async Task<IActionResult> Login()
    {
        AuthenticationProperties properties = new() { RedirectUri ="/metrics" };
        
        //TODO var result =
        //TODO     await HttpContext?.AuthenticateAsync(CookieAuthenticationDefaults
        //TODO         .AuthenticationScheme)!;
        //TODO 
        //TODO await HttpContext?.ChallengeAsync(GoogleDefaults.AuthenticationScheme, new AuthenticationProperties
        //TODO {
        //TODO     RedirectUri = "https://localhost:6556/metrics"
        //TODO })!;
        
        return Challenge(properties, GoogleDefaults.AuthenticationScheme);
    }

    [HttpGet("logout")]
    public async Task<IActionResult> Logout()
    {
        await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
        await HttpContext.SignOutAsync(GoogleDefaults.AuthenticationScheme);
        return RedirectToAction();
    }
}