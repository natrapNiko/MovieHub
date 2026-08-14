using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using MovieHub.ViewModels;

namespace MovieHub.Controllers;

/// <summary>
/// Central error handling controller. Unhandled exceptions are routed here
/// via app.UseExceptionHandler("/Error/Index") and HTTP status codes such as
/// 404/403 via app.UseStatusCodePagesWithReExecute("/Error/{0}"). Neither
/// path exposes internal exception details to the end user.
/// </summary>
[Route("Error")]
public class ErrorController : Controller
{
    private readonly ILogger<ErrorController> _logger;

    public ErrorController(ILogger<ErrorController> logger)
    {
        _logger = logger;
    }

    [Route("")]
    [Route("Index")]
    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Index()
    {
        var exceptionFeature = HttpContext.Features.Get<IExceptionHandlerPathFeature>();
        if (exceptionFeature is not null)
        {
            _logger.LogError(
                exceptionFeature.Error,
                "Unhandled exception while processing {Path}.",
                exceptionFeature.Path);
        }

        var model = new ErrorViewModel
        {
            RequestId = HttpContext.TraceIdentifier,
            StatusCode = 500,
            Title = "Something went wrong",
            Message = "An unexpected error occurred while processing your request. The issue has been logged and we're looking into it."
        };

        return View("Error", model);
    }

    [Route("{statusCode:int}")]
    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult HandleStatusCode(int statusCode)
    {
        var model = new ErrorViewModel
        {
            RequestId = HttpContext.TraceIdentifier,
            StatusCode = statusCode
        };

        switch (statusCode)
        {
            case 404:
                model.Title = "Page not found";
                model.Message = "The page you're looking for doesn't exist or may have been moved.";
                return View("NotFound", model);
            case 403:
                model.Title = "Access denied";
                model.Message = "You don't have permission to view this page.";
                return View("AccessDenied", model);
            default:
                model.Title = "Something went wrong";
                model.Message = "An unexpected error occurred while processing your request.";
                return View("Error", model);
        }
    }
}
