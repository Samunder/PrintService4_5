using System.Net;
using System.Net.Http;
using System.Web.Http.ExceptionHandling;
using System.Web.Http.Filters;
using System.Web.Http.Results;
using System.Web.Mvc;
using ExceptionContext = System.Web.Mvc.ExceptionContext;
using IExceptionFilter = System.Web.Mvc.IExceptionFilter;

namespace ButterflyPrint.Service45
{
    public class FilterConfig
    {
        public static void RegisterGlobalFilters(GlobalFilterCollection filters)
        {
            filters.Add(new CustomExceptionFilter());
        }
    }

    public class CustomExceptionFilter : IExceptionFilter
    {
        //public override void OnException(HttpActionExecutedContext actionExecutedContext)
        //{
        //    string exceptionMessage = string.Empty;
        //    if (actionExecutedContext.Exception.InnerException == null)
        //    {
        //        exceptionMessage = actionExecutedContext.Exception.Message;
        //    }
        //    else
        //    {
        //        exceptionMessage = actionExecutedContext.Exception.InnerException.Message;
        //    }
        //    //We can log this exception message to the file or database.
        //    var response = new HttpResponseMessage(HttpStatusCode.InternalServerError)
        //    {
        //        Content = new StringContent("An unhandled exception was thrown by service."),
        //        ReasonPhrase = "Internal Server Error.Please Contact your Administrator."
        //    };
        //    actionExecutedContext.Response = response;
        //}
        public void OnException(ExceptionContext filterContext)
        {
            string a = "";
        }
    }
    public class GlobalExceptionHandler : ExceptionHandler
    {
        //public async override TaskHandleAsync(ExceptionHandlerContext context, CancellationTokencancellationToken)
        //{
        //    // Access Exception using context.Exception;
        //    const string errorMessage = "An unexpected error occured";
        //    var response = context.Request.CreateResponse(HttpStatusCode.InternalServerError,
        //        new
        //        {
        //            Message = errorMessage
        //        });
        //    response.Headers.Add("X-Error", errorMessage);
        //    context.Result = new ResponseMessageResult(response);
        //}
    }
}
