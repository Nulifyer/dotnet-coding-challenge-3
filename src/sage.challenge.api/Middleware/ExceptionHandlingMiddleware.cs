using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;

namespace sage.challenge.api;

public class ExceptionHandlingMiddleware
{
    private readonly RequestDelegate _next;

    public ExceptionHandlingMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public async Task Invoke(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (Exception ex)
        {
            await HandleExceptionAsync(context, ex);
        }
    }

    private static Task HandleExceptionAsync(HttpContext context, Exception exception)
    {
        context.Response.ContentType = System.Net.Mime.MediaTypeNames.Application.Json;
        var response = new
        {
            error = exception.Message,
        };
        return context.Response.WriteAsJsonAsync(response);
    }
}
