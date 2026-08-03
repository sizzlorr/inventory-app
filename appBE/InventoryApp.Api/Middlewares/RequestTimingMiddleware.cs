using System.Diagnostics;

namespace InventoryApp.Api.Middlewares;

public class RequestTimingMiddleware(RequestDelegate next, ILogger<RequestTimingMiddleware> logger)
{
    public async Task InvokeAsync(HttpContext context)
    {
        // 1. ДІЯ ДО КОНТРОЛЕРА: Запускаємо секундомір
        var watch = Stopwatch.StartNew();

        // 2. ПЕРЕДАЄМО ЗАПИТ ДАЛІ: Чекаємо, поки інші шари та контролер зроблять свою роботу
        await next(context);

        // 3. ДІЯ ПІСЛЯ КОНТРОЛЕРА: Зупиняємо секундомір і логуємо результат
        watch.Stop();
        var ms = watch.ElapsedMilliseconds;

        // Логуємо тільки шлях запиту (наприклад, /api/inventory) і час
        logger.LogInformation($"[TIMING] Request to {context.Request.Path} was {ms} ms");
    }
    
}

