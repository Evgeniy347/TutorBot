# TutorBot — AGENTS.md

## Структура проекта
- Решение: `TutorBot.API.slnx` (формат `.slnx`, а не `.sln`)
- Точка входа: `src/TutorBot.App/Program.cs` — Blazor Interactive Server + Telegram-бот.
- 9 проектов в папке `src/`. Ключевые:
  - `TutorBot.App` — хост, настройка запуска, компоненты Blazor
  - `TutorBot.Core` — EF Core DbContext, миграции, интеграция с GigaChat
  - `TutorBot.TelegramService` — логика Telegram-бота, действия бота, модель диалога
  - `TutorBot.Frontend` — регистрация Blazor UI, компоненты Radzen
  - `TutorBot.Authentication` — вспомогательные классы для аутентификации
  - `TutorBot.Abstractions` — интерфейсы (`IApplication`, `IChatService` и т.д.)
  - `TutorBot.Primitives` — утилиты (`Check`, `StringExtensions` и т.д.)
  - `TutorBot.ServiceDefaults` — настройки OpenTelemetry, стандарты обнаружения сервисов

## Фреймворк и инструментарий
- .NET 10.0 (`net10.0`), включены nullable reference types, implicit usings
- Включён `TreatWarningsAsErrors`. Исключения: `NU1900`, `NU1603`, `NU1608`
- Централизованное управление пакетами через `Directory.Packages.props`
- Источник NuGet: только `nuget.org` (все остальные источники очищены в `NuGet.Config`)
- Docker: только образы `arm64`. Сборка через `scripts/build-push.sh` или:
  ```bash
  docker buildx build --platform linux/arm64 -t tutorbot:<tag> --load .
  ```
- База данных: PostgreSQL через Npgsql + EF Core. Миграции в `src/TutorBot.Core/Migrations/`

## Тестирование
- xunit v3 (пакет `xunit.v3`). **Не** xunit v2.
- Юнит-тесты: `src/TutorBot.Test/`. Запуск: `dotnet test src/TutorBot.Test`
- Интеграционные тесты: `src/TutorBot.IntegrationTest/`. Требуют Docker (PostgreSQL Testcontainers с `postgres:17.4`). Запуск: `dotnet test src/TutorBot.IntegrationTest`
- Атрибуты категорий: `[Trait("Category", "Unit")]` для юнит-тестов, `[Trait("Category", "Integration")]` для интеграционных
- Базовый класс интеграционных тестов: наследовать `IntegrationTestsBase` (реализует `IClassFixture<CustomAppFactory>`). Тесты используют `[Collection("TestContainerCollection")]` — запуски сериализованы (`DisableParallelization = true`)
- Группы снапшотов БД: аннотировать класс теста атрибутом `[DatabaseSnapshotGroup]` для группировки тестов с общим снапшотом БД; `[TestPriority(n)]` управляет порядком внутри группы
- Покрытие: два файла настроек — `coverlet.runsettings` (с исключениями) и `coverlet.nofilter.runsettings` (без исключений). Цели: `TutorBot.TelegramService`, `TutorBot.Core`, `TutorBot.Primitives`, `TutorBot.Abstractions`, `TutorBot.Authentication`. Запуск: `dotnet test --settings coverlet.runsettings`
- Утверждения: Shouldly (fluent assertions), Moq (моки)
- Фейки (интеграционные тесты): `TelegramBotFake` эмулирует API Telegram-бота; `TestBotFactory` предоставляет его

## Конфигурация
- Настройки приложения загружаются из: `appsettings.json` → `appsettings.private.json`. Отключается через AppContext-переключатель `DisableLoadConfig` (используется в интеграционных тестах)
- Обязательные секции конфигурации:
  - `TelegramService:Token`, `TelegramService:EvaluateKey`, `TelegramService:DialogModelPath`
  - `GigaChat:SecretKey`
  - `ConnectionStrings:DefaultConnection`
- Приватные файлы игнорируются git (`*.private.*`): `appsettings.private.json`, `SystemPromtBot.private.txt`, `SystemPromtGroupBot.private.txt`

## Ключевые соглашения
- `InternalsVisibleTo` предоставлен сборкам `TutorBot.Test` и `TutorBot.IntegrationTest` из `TutorBot.Core`, `TutorBot.App` и `TutorBot.TelegramService`
- В `AddApplicationCore()` включено устаревшее поведение таймстампов Npgsql: `Npgsql.EnableLegacyTimestampBehavior` и `DisableDateTimeInfinityConversions`
- Модель диалога бота — это JSON-файл (`DialogDiagram.json`), загружаемый через `DialogModelLoader`
- В `.github/workflows/` нет CI/CD пайплайнов (папка пустая)

---

✅ **Зафиксированные правила работы:**
1. 💭 **Мысль и ответы:** исключительно на русском языке.
2. 💻 **Код:** названия классов, методов, переменных, интерфейсов и вся архитектура — строго на английском.
3. 📝 **Комментарии:** только на русском языке.
4. 🔍 **Где комментировать:** только публичный API и неочевидную/сложную логику. Очевидные или приватные внутренние участки оставляем без комментариев.
 