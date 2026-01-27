# Config Server

Централизованный сервис управления конфигурациями для .NET приложений. Позволяет хранить и распространять параметры конфигурации между приложениями с поддержкой различных окружений (Development, Staging, Production) и профилей развёртывания.

## Возможности

- Централизованное хранилище конфигураций
- REST API для получения и установки параметров
- Интеграция с ASP.NET Core `IConfiguration`
- Периодическое обновление конфигураций на клиентской стороне
- Поддержка пагинации при получении большого количества параметров
- Автоматическая документация API через Swagger

## REST API

- `GET /configs/{project}/{profile}/{environment}` - получение всех конфигураций
- `GET /configs/{project}/{profile}/{environment}/{key}` - получение конифгурации по ключу
- `POST /configs/{project}/{profile}/{environment}/{key}` - добавление конфигурации по ключу
- `POST /configs/{project}/{profile}/{environment}` - батчевое добавление конфигурации

## Архитектура

Система построена на Clean Architecture с четырьмя слоями:

- **API слой** (`Config.Server.Api.Http`) - REST контроллеры и эндпоинты
- **Application слой** - бизнес-логика и контракты операций
- **Infrastructure слой** - работа с PostgreSQL через Npgsql
- **Client слой** (`Config.Server.Configuration`) - интеграция с ASP.NET Core

## Технологический стек

- .NET 10.0
- ASP.NET Core
- PostgreSQL
- Npgsql (асинхронный провайдер для PostgreSQL)
- Refit (декларативный REST-клиент)
- Swagger/OpenAPI

## Развертывание

### Серверная часть

Docker образ распространяется через GitHub Container Registry:

```yml
services:
    config-server:
        image: ghcr.io/kgn9/config-server:latest
    environment:
        CONNECTIONSTRING="PostgreSQL DB connection string"
```

Требуется PostgreSQL для хранилища конфигураций.

## Использование

### Клиентский пакет

```bash
dotnet add package Config.Server.Configuration --source https://nuget.pkg.github.com/kgn9/index.json
```

### Регистрация клиента

```csharp
services.AddConfigServer(builder, options =>
{
    options.Url = "https://config-server.example.com";
    options.Project = "MyApp";
    options.Profile = "Web";
    options.Environment = ConfigEnvironment.Production;
});

// Optional: add background configuration refreshing
services.AddBackgroundConfigRefreshing();
```

### Получение конфигураций

```csharp
var config = serviceProvider.GetRequiredService<IConfiguration>();
string setting = config["MySettingKey"];
```

---

**Версия:** 1.0.0 | **Фреймворк:** .NET 10.0
