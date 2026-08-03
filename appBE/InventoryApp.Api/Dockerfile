# === ЕТАП 1: Збірка (Build) ===
# Беремо SDK для .NET 10
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

# Спочатку копіюємо тільки файл проекту, щоб кешувати залежності
COPY ["InventoryApp.Api.csproj", "./"]
RUN dotnet restore "./InventoryApp.Api.csproj"

# Тепер копіюємо весь інший код і збираємо реліз
COPY . .
RUN dotnet publish "InventoryApp.Api.csproj" -c Release -o /app/publish /p:UseAppHost=false

# === ЕТАП 2: Запуск (Run) ===
# Беремо середовище виконання для .NET 10
FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS final
WORKDIR /app
EXPOSE 8080

# Забираємо готові файли з етапу 'build'
COPY --from=build /app/publish .

# Запускаємо нашу .dll (переконайся, що ім'я збігається)
ENTRYPOINT ["dotnet", "InventoryApp.Api.dll"]