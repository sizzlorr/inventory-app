using System.Net;
using System.Text.Json;

namespace InventoryApp.Api.Middlewares;

public class ExceptionHandlingMiddleware(RequestDelegate next, ILogger<ExceptionHandlingMiddleware> logger)
{
    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await next(context);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Unexpected error");

            await HandleExceptionAsync(context, ex);
        }
    }

    private static Task HandleExceptionAsync(HttpContext context, Exception exception)
    {
        // Кажемо клієнту, що повертаємо JSON і статус 500 (Internal Server Error)
        context.Response.ContentType = "application/json";
        context.Response.StatusCode = (int)HttpStatusCode.InternalServerError;

        // Створюємо стандартизований об'єкт помилки
        var errorResponse = new 
        {
            StatusCode = context.Response.StatusCode,
            Message = "Internal server error",
            // Увага: на реальному продакшені 'Detailed' зазвичай приховують з метою безпеки
            Detailed = exception.Message 
        };

        // Перетворюємо C# об'єкт у JSON рядок і відправляємо
        var result = JsonSerializer.Serialize(errorResponse);
        return context.Response.WriteAsync(result);
    }
}

