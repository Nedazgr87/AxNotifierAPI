using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace AxNotifierAPI
{

    public class RequestFilterAttribute : ActionFilterAttribute
    {
        public override void OnActionExecuting(ActionExecutingContext actionContext)
        {
            string key = actionContext.HttpContext.Request.Headers["key"];
            if (!Statics.CheckKey(key))
            {
                actionContext.Result = new OkObjectResult(new ApiResult("Invalid key", false, TypeEnum.Error));
                return;
            }

            base.OnActionExecuting(actionContext);
        }
    }

}
