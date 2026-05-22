using Microsoft.AspNetCore.Localization;
using Microsoft.AspNetCore.Mvc;

namespace UM_Project.Controllers;

public class CultureController : Controller
{
    [HttpGet]
    public IActionResult Set(string culture, string? returnUrl)
    {
        if (culture is not ("en" or "sq"))
            culture = "en";

        Response.Cookies.Append(
            CookieRequestCultureProvider.DefaultCookieName,
            CookieRequestCultureProvider.MakeCookieValue(new RequestCulture(culture)),
            new CookieOptions
            {
                Expires = DateTimeOffset.UtcNow.AddYears(1),
                IsEssential = true,
                Path = "/",
                SameSite = SameSiteMode.Lax,
                HttpOnly = false
            });

        if (string.IsNullOrWhiteSpace(returnUrl) || returnUrl == "/")
            return RedirectToAction("Index", "Home");

        if (Url.IsLocalUrl(returnUrl))
            return LocalRedirect(returnUrl);

        return RedirectToAction("Index", "Home");
    }
}
