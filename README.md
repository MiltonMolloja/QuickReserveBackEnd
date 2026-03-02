# QuickReserve Backend API

Sistema de reservas de turnos para talleres mecánicos - Challenge técnico Tecnom.

> **IMPORTANTE:** Los documentos de análisis y planificación utilizados para el desarrollo de este proyecto se encuentran en:
>
> - [Plan de Arquitectura y Diseño](https://github.com/MiltonMolloja/QuickReserveBackEnd/blob/master/Docs/QuickReserve-Backend-Plan.md)
> - [Guía de Implementación (paso a paso)](https://github.com/MiltonMolloja/QuickReserveBackEnd/blob/master/Docs/QuickReserve-Backend-Implementacion.md)

## Arquitectura

Este proyecto implementa **Clean Architecture** con **DDD** y **CQRS** usando MediatR.

```
src/
├── QuickReserve.Domain/           # Entidades, Value Objects, Domain Events
├── QuickReserve.Application/      # Casos de uso, Commands, Queries, DTOs
├── QuickReserve.Infrastructure/   # EF Core, Redis, HTTP Clients
└── QuickReserve.API/              # Controllers, Middleware, Configuration

tests/
└── QuickReserve.Tests/            # Unit, Integration & Architecture Tests
```

## Stack Tecnológico

- **.NET 10** - Framework principal
- **Entity Framework Core InMemory** - Persistencia (desarrollo)
- **MediatR** - CQRS pattern
- **FluentValidation** - Validaciones
- **Mapster** - Object mapping
- **Polly** - Resilience & retry policies
- **Redis** - Caching distribuido
- **Serilog + ELK** - Logging estructurado
- **SonarQube** - Análisis de código
- **xUnit + FluentAssertions** - Testing
- **NetArchTest** - Architecture tests

## Requisitos

- [.NET 10 SDK](https://dotnet.microsoft.com/download)
- [Docker Desktop](https://www.docker.com/products/docker-desktop)

## Inicio Rápido

### 1. Clonar el repositorio

```bash
git clone https://github.com/MiltonMolloja/QuickReserveBackEnd.git
cd QuickReserveBackEnd
```

### 2. Levantar servicios con Docker

```bash
cd docker
docker-compose up -d
```

Esto levanta:
- **Redis** - localhost:6379
- **Elasticsearch** - localhost:9200
- **Kibana** - localhost:5601
- **SonarQube** - localhost:9000

### 3. Configurar credenciales (User Secrets)

```bash
cd src/QuickReserve.API
dotnet user-secrets set "TecnomApi:Username" "<tu_usuario>"
dotnet user-secrets set "TecnomApi:Password" "<tu_password>"
dotnet user-secrets set "ConnectionStrings:Redis" "localhost:6379"
```

### 4. Ejecutar la API

```bash
dotnet run --project src/QuickReserve.API
```

La API estará disponible en:
- HTTP: http://localhost:5000
- Swagger: http://localhost:5000/swagger

> Al iniciar en modo Development, se precargan **80 turnos de ejemplo** (4 turnos/día x 5 días hábiles x 4 talleres).

### 5. Ejecutar Tests

```bash
dotnet test --collect:"XPlat Code Coverage"
```

## Endpoints

| Método | Endpoint | Descripción |
|--------|----------|-------------|
| GET | `/api/workshops` | Lista talleres activos (desde Tecnom CRM) |
| GET | `/api/appointments` | Lista todas las reservas |
| POST | `/api/appointments` | Crear una nueva reserva |
| GET | `/health` | Health check |

## Reglas de Negocio

- Los turnos solo pueden agendarse en horarios fijos: **09:00, 10:00, 11:00, 12:00, 13:00, 14:00, 15:00, 16:00, 17:00** (hora Argentina, UTC-3)
- Solo se atiende de **lunes a viernes** (sábado y domingo no hay turnos)
- Un taller solo puede atender **un turno a la vez**
- Solo se pueden crear turnos en talleres **activos** (validado contra la API de Tecnom CRM)

## Análisis de Código con SonarQube

### Configuración inicial

1. Acceder a SonarQube: http://localhost:9000
2. Login: admin/admin (cambiar en primer acceso)
3. Crear proyecto "quickreserve-backend"
4. Generar token de autenticación

### Ejecutar análisis

```bash
# Instalar scanner (una vez)
dotnet tool install --global dotnet-sonarscanner

# Ejecutar análisis
dotnet sonarscanner begin /k:"quickreserve-backend" /d:sonar.host.url="http://localhost:9000" /d:sonar.token="YOUR_TOKEN"
dotnet build
dotnet test --collect:"XPlat Code Coverage"
dotnet sonarscanner end /d:sonar.token="YOUR_TOKEN"
```

## Variables de Entorno

| Variable | Descripción | Default |
|----------|-------------|---------|
| `ASPNETCORE_ENVIRONMENT` | Entorno de ejecución | Development |
| `ConnectionStrings__Redis` | Connection string Redis | (User Secrets) |
| `TecnomApi__BaseUrl` | URL API externa talleres | https://dev.tecnomcrm.com/api/v1/ |
| `TecnomApi__Username` | Usuario API externa | (User Secrets) |
| `TecnomApi__Password` | Password API externa | (User Secrets) |

## Estructura de Carpetas

```
QuickReserveBackEnd/
├── src/
│   ├── QuickReserve.Domain/
│   │   ├── Entities/
│   │   ├── ValueObjects/
│   │   ├── Exceptions/
│   │   ├── Interfaces/
│   │   └── Services/
│   ├── QuickReserve.Application/
│   │   ├── Common/
│   │   │   └── Behaviors/       # Logging, Validation pipelines
│   │   ├── DTOs/
│   │   │   ├── Requests/
│   │   │   └── Responses/
│   │   ├── Features/
│   │   │   ├── Workshops/
│   │   │   └── Appointments/
│   │   ├── Mappings/
│   │   └── Validators/
│   ├── QuickReserve.Infrastructure/
│   │   ├── Persistence/
│   │   │   ├── Configurations/
│   │   │   └── Repositories/
│   │   ├── ExternalServices/
│   │   │   └── Models/
│   │   └── Configuration/
│   └── QuickReserve.API/
│       ├── Controllers/
│       └── Middleware/
├── tests/
│   └── QuickReserve.Tests/
│       ├── Domain/
│       ├── Application/
│       ├── Architecture/
│       ├── Infrastructure/
│       └── Integration/
├── Docs/                          # Documentación de análisis y planificación
├── docker/
│   ├── Dockerfile
│   └── docker-compose.yml
├── .github/
│   └── workflows/
├── Directory.Build.props
├── .editorconfig
└── QuickReserve.sln
```

## Convenciones

- **Commits**: [Conventional Commits](https://www.conventionalcommits.org/)
- **Code Style**: Configurado en `.editorconfig` y StyleCop
- **Architecture**: Clean Architecture con CQRS (MediatR)

## Licencia

MIT License - Ver [LICENSE](LICENSE) para más detalles.
