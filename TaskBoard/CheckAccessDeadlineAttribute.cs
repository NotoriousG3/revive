using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace TaskBoard;

public class AccessDeadlineMetException : Exception
{
}

public class CheckAccessDeadlineAttribute : IAsyncActionFilter
{
    private const string _errorMessage = "Access revoked due to meeting the set deadline";
    protected readonly AppSettingsLoader _loader;

    public CheckAccessDeadlineAttribute(AppSettingsLoader loader)
    {
        _loader = loader;
    }

    public virtual async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
    {
        var settings = await _loader.Load();
        if (!ValidateAccess(context, settings.AccessDeadline))
        {
            return;
        }
        
        await next();
    }

    protected bool ValidateAccess(ActionExecutingContext context, DateTime? limitDate)
    {
        if (DateTime.UtcNow >= limitDate)
        {
            context.Result = new RedirectToRouteResult(new RouteValueDictionary()
            {
                {"controller", "NoAccess"},
                {"action", "Index"}
            });
            return false;
        }

        return true;
    }
}

public class CheckLateAccessDeadlineAttribute : CheckAccessDeadlineAttribute
{
    public CheckLateAccessDeadlineAttribute(AppSettingsLoader loader) : base(loader)
    {
    }

    public override async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
    {
        var settings = await _loader.Load();
        if (!ValidateAccess(context, settings.AccessDeadline.Value.AddDays(1)))
        {
            return;
        }

        await next();
    }
}