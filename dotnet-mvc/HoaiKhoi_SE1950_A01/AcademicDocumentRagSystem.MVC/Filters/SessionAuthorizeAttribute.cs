using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace AcademicDocumentRagSystem.MVC.Filters;

public class SessionAuthorizeAttribute : ActionFilterAttribute
{
    private readonly string[] _roles;

    public SessionAuthorizeAttribute(params string[] roles)
    {
        _roles = roles;
    }

    public override void OnActionExecuting(ActionExecutingContext context)
    {
        var session = context.HttpContext.Session;
        var email = session.GetString("Email");

        if (string.IsNullOrWhiteSpace(email))
        {
            context.Result = new RedirectToRouteResult("login", null);
            return;
        }

        if (_roles.Length == 0)
        {
            return;
        }

        var roleName = session.GetString("RoleName");

        if (!_roles.Contains(roleName))
        {
            context.Result = new RedirectToActionResult("AccessDenied", "Auth", null);
        }
    }
}
