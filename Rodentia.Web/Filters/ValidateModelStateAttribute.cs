using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace Rodentia.Web.Filters;

public class ValidateModelStateAttribute : ActionFilterAttribute
{
    public override void OnActionExecuting(ActionExecutingContext context)
    {
        if (!context.ModelState.IsValid)
        {
            var isAjax = context.HttpContext.Request.Headers["X-Requested-With"] == "XMLHttpRequest";

            if (isAjax)
            {
                var errors = context.ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage);
                context.Result = new BadRequestObjectResult(new { message = "Перевір поля форми.", errors });
            }
            else
            {
                var controller = context.Controller as Controller;
                var model = context.ActionArguments.Values.FirstOrDefault();
                
                if (controller != null)
                {
                    controller.ViewData.Model = model;
                    context.Result = new ViewResult 
                    { 
                        ViewData = controller.ViewData, 
                        TempData = controller.TempData 
                    };
                }
            }
        }
    }
}