# QuickReserve Backend API

Sistema de reservas de turnos para talleres mecánicos - Challenge técnico Tecnom.

## Arquitectura

Este proyecto implementa **Clean Architecture** con **DDD** y **CQRS** usando MediatR.

```
src/
├── QuickReserve.Domain/           # Entidades, Value Objects, Domain Events
├── QuickReserve.Application/      # Casos de uso, Commands, Queries, DTOs
├── QuickReserve.Infrastructure/   # EF Core, Redis, HTTP Clients
└── QuickReserve.API/              # Controllers, Middleware, Configuration

tests/
└── QuickReserve.Tests/            # Unit & Integration Tests
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

## Requisitos

- [.NET 10 SDK](https://dotnet.microsoft.com/download)
- [Docker Desktop](https://www.docker.com/products/docker-desktop)

## Inicio Rápido

### 1. Clonar el repositorio

```bash
git clone https://github.com/your-username/quickreserve-backend.git
cd quickreserve-backend
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

### 3. Ejecutar la API

```bash
dotnet run --project src/QuickReserve.API
```

La API estará disponible en:
- HTTP: http://localhost:5000
- HTTPS: https://localhost:5001
- Swagger: http://localhost:5000/swagger

### 4. Ejecutar Tests

```bash
dotnet test --collect:"XPlat Code Coverage"
```

## Endpoints Principales

| Método | Endpoint | Descripción |
|--------|----------|-------------|
| GET | `/api/workshops` | Lista talleres disponibles |
| GET | `/api/workshops/{id}/availability` | Disponibilidad de un taller |
| POST | `/api/appointments` | Crear reserva |
| GET | `/api/appointments/{id}` | Obtener reserva |
| DELETE | `/api/appointments/{id}` | Cancelar reserva |
| GET | `/health` | Health check |

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
| `ConnectionStrings__Redis` | Connection string Redis | localhost:6379 |
| `ExternalApi__BaseUrl` | URL API externa talleres | https://dev.tecnomcrm.com |
| `ExternalApi__Username` | Usuario API externa | - |
| `ExternalApi__Password` | Password API externa | - |

## Estructura de Carpetas

```
QuickReserveBackEnd/
├── src/
│   ├── QuickReserve.Domain/
│   │   ├── Entities/
│   │   ├── ValueObjects/
│   │   ├── Events/
│   │   ├── Exceptions/
│   │   └── Interfaces/
│   ├── QuickReserve.Application/
│   │   ├── Common/
│   │   │   ├── Behaviors/
│   │   │   ├── Interfaces/
│   │   │   └── Models/
│   │   └── Features/
│   │       ├── Workshops/
│   │       └── Appointments/
│   ├── QuickReserve.Infrastructure/
│   │   ├── Persistence/
│   │   ├── Caching/
│   │   └── ExternalServices/
│   └── QuickReserve.API/
│       ├── Controllers/
│       ├── Middleware/
│       └── Filters/
├── tests/
│   └── QuickReserve.Tests/
│       ├── Domain/
│       ├── Application/
│       ├── Infrastructure/
│       └── API/
├── docker/
│   └── docker-compose.yml
├── .github/
│   └── workflows/
├── Directory.Build.props
├── .editorconfig
├── sonar-project.properties
└── QuickReserve.sln
```

## Convenciones

- **Commits**: [Conventional Commits](https://www.conventionalcommits.org/)
- **Branching**: GitFlow
- **Code Style**: Configurado en `.editorconfig` y StyleCop

## Licencia

MIT License - Ver [LICENSE](LICENSE) para más detalles.
