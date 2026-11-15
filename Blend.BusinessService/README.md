# Blend.BusinessService

This project is the modern business/application logic layer. It references `Blend.DataService` and exposes business-facing contracts to the Blazor app. The legacy `Blend.BuisnessService` project is unchanged.

The initial example is the Broker service:

- `IBrokerService` is the application-facing contract.
- `BrokerService` coordinates with `IBrokerDataService` and maps data records to business models.
- Write operations return `BusinessOperationResult` for a reported unsuccessful operation; reads return `null` when the requested broker does not exist.
- Unexpected data-layer exceptions are not caught or converted into success-shaped results. They propagate to the Blazor application's exception boundary, which logs the exception and presents a safe error page with a request ID.
- The Blazor app registers `IBrokerService` and the underlying Dapper data service in dependency injection.

Broker operations require `ConnectionStrings:BlendData`, separate from `ConnectionStrings:BlendIdentity`. Configure the data connection in User Secrets before calling broker operations. The application can start without this setting; requesting the Broker service reports a configuration error.
