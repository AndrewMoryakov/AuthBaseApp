# AuthBaseApp

## Architecture Overview / Обзор архитектуры

### English
AuthBaseApp is an ASP.NET Core Web API that follows a layered architecture:

- **Controllers** (`WebApplication1/Controllers`): handle endpoints for authentication and user, country, and province resources.
- **Data Layer** (`WebApplication1/Data`): contains `ApplicationDbContext`, entity models, and repository/Unit of Work patterns for persistence.
- **Security** (`WebApplication1/Security`): provides JWT-based authentication through `AuthenticationService` and `TokenFactory`.
- **Filters** (`WebApplication1/Filters`): global `ExceptionFilter` centralizes error handling.

### Русский
AuthBaseApp — это веб‑API на ASP.NET Core, построенное по многоуровневой архитектуре:

- **Controllers** (`WebApplication1/Controllers`): реализуют конечные точки для аутентификации и управления пользователями, странами и провинциями.
- **Data** (`WebApplication1/Data`): содержит `ApplicationDbContext`, модели сущностей и реализацию шаблонов репозиторий и Unit of Work.
- **Security** (`WebApplication1/Security`): обеспечивает аутентификацию на базе JWT через `AuthenticationService` и `TokenFactory`.
- **Filters** (`WebApplication1/Filters`): глобальный фильтр исключений `ExceptionFilter` обрабатывает ошибки в одном месте.

