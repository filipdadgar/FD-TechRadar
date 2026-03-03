using System.Text.Json;

namespace TechRadar.Api.Middleware;

public class GlobalErrorHandlingMiddleware(RequestDelegate next, ILogger<GlobalErrorHandlingMiddleware> logger)
{
    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await next(context);
        }
        catch (KeyNotFoundException ex)
        {
            logger.LogWarning(ex, "Resource not found");
            await WriteError(context, 404, ex.Message, null);
        }
        catch (InvalidOperationException ex) when (ex.Message.Contains("conflict", StringComparison.OrdinalIgnoreCase))
        {
            logger.LogWarning(ex, "Conflict error");
            await WriteError(context, 409, ex.Message, null);
        }
        catch (ArgumentException ex)
        {
            logger.LogWarning(ex, "Argument error");
            await WriteError(context, 400, ex.Message, null);
        }
        catch (UnauthorizedAccessException ex)
        {
            logger.LogWarning(ex, "Unauthorized");
            await WriteError(context, 401, "Unauthorized", null);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Unhandled exception");
            var detail = context.RequestServices
                .GetRequiredService<IWebHostEnvironment>().IsDevelopment()
                ? ex.ToString()
                : null;
            await WriteError(context, 500, "An unexpected error occurred.", detail);
        }
    }

    private static async Task WriteError(HttpContext context, int statusCode, string error, string? detail)
    {
        context.Response.StatusCode = statusCode;
        context.Response.ContentType = "application/json";
        var body = detail != null
            ? new { error, detail }
            : (object)new { error };
        await context.Response.WriteAsync(JsonSerializer.Serialize(body));
    }
}
