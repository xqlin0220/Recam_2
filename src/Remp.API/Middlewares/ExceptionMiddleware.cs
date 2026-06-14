using System.Net;
using System.Text.Json;
using System.Text.Json.Serialization;
using Azure;
using Microsoft.AspNetCore.Http.Json;
using Remp.Common.Utilities;
using Remp.Common.Exceptions;

namespace Remp.API.Middlewares;
public class ExceptionMiddleware
{   
    private readonly RequestDelegate _next;
    private readonly ILogger<ExceptionMiddleware> _loger;

    public ExceptionMiddleware(RequestDelegate next, ILogger<ExceptionMiddleware> loger)
    {
        _next = next;
        _loger = loger;
    }

    public async Task InvokeAsync (HttpContext context) 
    {
        try
        {
            await _next(context);
        }
        catch (Exception ex)
        {
            _loger.LogError(ex, "An exception occurred: {Message} | Path: {Path}",
                ex.Message,
                context.Request.Path);
            await HandleExceptionAsync(context, ex); 
        }
    }

    private static async Task HandleExceptionAsync(HttpContext context, Exception exception)
    {
        context.Response.ContentType = "application/json";

        var(statusCode, response) = exception switch
        {
            NotFoundException ex => (
                HttpStatusCode.NotFound,
                ApiResponse<object>.NotFound(ex.Message)
            ),

            ValidationException ex =>(
                HttpStatusCode.BadRequest,
                ApiResponse<object>. BadRequest(ex.Message, ex.Errors)
            ),
            UnauthorizedException ex =>(
                HttpStatusCode.Unauthorized,
                ApiResponse<object>. Unauthorized(ex.Message)
            ),
            ForbiddenException ex =>(
                HttpStatusCode.Forbidden,
                ApiResponse<object>. Forbidden(ex.Message)
            ),
            AppException ex =>(
                (HttpStatusCode) ex.StatusCode,
                ApiResponse<object>.ServerError(ex.Message)
            ),

            _ =>(
                HttpStatusCode.InternalServerError,
                ApiResponse<object>.ServerError("An unexpected error occurred. Please try again later.")
            )
        };
    
        context.Response.StatusCode = (int)statusCode;
        var jsonOptions = new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        };
        var jsonResponse = JsonSerializer.Serialize(response, jsonOptions);
        await context.Response.WriteAsync(jsonResponse);
    }
}
