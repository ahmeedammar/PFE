using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace Expense_Tracker.Filters
{
    public class AuthorizeAdminAttribute : ActionFilterAttribute
    {
        public override void OnActionExecuting(ActionExecutingContext context)
        {
            if (!context.HttpContext.User.Identity.IsAuthenticated)
            {
                context.Result = new RedirectToActionResult("Login", "Account", null);
            }
            else
            {
                // Check if the user is an admin (optional, add your admin check logic here)
                bool isAdmin = true; // Replace with your admin check logic

                if (!isAdmin)
                {
                    context.Result = new ForbidResult();
                }
            }

            base.OnActionExecuting(context);
        }
    }
}
