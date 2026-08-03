# === ЕТАП 1: Збірка (Build) ===
# Беремо SDK для .NET 10
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

# Копіюємо файл конфігурації
COPY ["InventoryApp.Api.csproj", "./"]
RUN dotnet restore "InventoryApp.Api.csproj"

# Копіюємо весь інший код
COPY . .
# Публікуємо проект
RUN dotnet publish "InventoryApp.Api.csproj" -c Release -o /app/publish

# === ЕТАП 2: Запуск (Run) ===
# Беремо середовище виконання для .NET 10
FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS final
WORKDIR /app

EXPOSE 8080
ENV ASPNETCORE_HTTP_PORTS=8080

# Копіюємо готовий результат з Етапу 1
COPY --from=build /app/publish .

# Запускаємо нашу .dll (переконайся, що ім'я збігається)
ENTRYPOINT ["dotnet", "InventoryApp.Api.dll"]