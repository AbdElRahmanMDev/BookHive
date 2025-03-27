using Microsoft.AspNetCore.Mvc.Abstractions;
using Microsoft.AspNetCore.Mvc.ActionConstraints;

namespace BookHive.Web.Filters
{
    public class AjaxOnly : ActionMethodSelectorAttribute
    {
        public override bool IsValidForRequest(RouteContext routeContext, ActionDescriptor action)
        {
            var request=routeContext.HttpContext.Request;
            var ajax = request.Headers["x-requested-with"] =="XMLHttpRequest";
            return ajax;
        }
    }
}
