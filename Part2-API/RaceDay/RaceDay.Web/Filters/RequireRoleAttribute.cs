using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace RaceDay.Web.Filters;

// Put [RequireRole] on a controller to make it login-only,
// or [RequireRole("Organiser")] to allow only one role.
public class RequireRoleAttribute : ActionFilterAttribute
{
    private readonly string? _role;

    public RequireRoleAttribute(string? role = null)
    {
        _role = role;
    }

    public override void OnActionExecuting(ActionExecutingContext context)
    {
        var userRole = context.HttpContext.Session.GetString("UserRole");

        // Not logged in: send to the login page
        if (string.IsNullOrEmpty(userRole))
        {
            context.Result = new RedirectToActionResult("Login", "Account", null);
            return;
        }

        // Logged in but the wrong role: send to the access denied page
        if (_role != null && !string.Equals(userRole, _role, StringComparison.OrdinalIgnoreCase))
        {
            context.Result = new RedirectToActionResult("AccessDenied", "Account", null);
        }
    }
}