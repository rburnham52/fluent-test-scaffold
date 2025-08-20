using System.Net;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace FluentTestScaffold.Sample.WebApp.Exceptions.Filters;

public class InvalidOperationsExceptionFilter : IExceptionFilter
{
    public void OnException(ExceptionContext context)
    {
        if (context.Exception is InvalidOperationException exception)
        {
            context.Result = new ObjectResult(new ProblemDetails
            {
                Detail = exception.Message,
                Status = (int)HttpStatusCode.BadRequest
            })
            {
                StatusCode = (int)HttpStatusCode.BadRequest
            };
        }
    }
}
