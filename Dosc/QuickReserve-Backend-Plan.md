# QuickReserve Backend - Plan de Proyecto

> **Challenge Tecnico Tecnom** - Full-Stack DEV (Angular + .NET)
> Fecha: 2026-02-26
> Version: 3.0 (Clean Architecture + DDD + CQRS + Shift-Left Quality)

---

## Resumen del Challenge

Desarrollar una aplicacion Full-Stack que permita la **reserva y visualizacion de turnos en talleres**, replicando un caso de uso real del producto "Boxes" de Tecnom.

**Enfoque de este documento:** Backend en .NET

---

## 1. Analisis de Requisitos Funcionales

### RF-01: Gestion de Turnos (Appointments)

| ID      | Requisito                                                                                | Prioridad | Endpoint                 |
| ------- | ---------------------------------------------------------------------------------------- | --------- | ------------------------ |
| RF-01.1 | Crear un turno con datos de servicio, contacto y opcionalmente vehiculo                  | Alta      | `POST /api/appointments` |
| RF-01.2 | Validar campos requeridos: `name`, `email`, `appointment_at`, `service_type`, `place_id` | Alta      | `POST /api/appointments` |
| RF-01.3 | Validar que el `place_id` corresponda a un taller activo (validacion externa)            | Alta      | `POST /api/appointments` |
| RF-01.4 | Listar todos los turnos creados con informacion principal                                | Alta      | `GET /api/appointments`  |
| RF-01.5 | Los datos del vehiculo (`make`, `model`, `year`, `license_plate`) son opcionales         | Alta      | `POST /api/appointments` |
| RF-01.6 | Devolver errores claros cuando los datos no cumplen con lo requerido                     | Alta      | `POST /api/appointments` |

### RF-02: Consulta de Talleres (Workshops)

| ID      | Requisito                                                          | Prioridad | Endpoint             |
| ------- | ------------------------------------------------------------------ | --------- | -------------------- |
| RF-02.1 | Obtener listado de talleres activos desde la API externa de Tecnom | Alta      | `GET /api/workshops` |
| RF-02.2 | Autenticarse contra la API externa con Basic Auth                  | Alta      | Interno              |
| RF-02.3 | Devolver datos del taller: nombre, direccion, email, whatsapp      | Alta      | `GET /api/workshops` |
| RF-02.4 | Implementar caching para evitar consultas repetitivas              | Media     | Interno              |

---

## 2. Requisitos No Funcionales

| ID     | Requisito                                                            | Categoria      |
| ------ | -------------------------------------------------------------------- | -------------- |
| RNF-01 | Almacenamiento en memoria (no se requiere BD persistente)            | Persistencia   |
| RNF-02 | API RESTful con respuestas HTTP semanticas (200, 201, 400, 404, 500) | Estandar       |
| RNF-03 | Codigo limpio, bien estructurado y mantenible                        | Calidad        |
| RNF-04 | Documentacion de decisiones clave                                    | Documentacion  |
| RNF-05 | CORS habilitado para el frontend Angular                             | Seguridad      |
| RNF-06 | Resiliencia ante fallos de la API externa (Polly)                    | Disponibilidad |
| RNF-07 | Monitoreo basico de salud (Health Checks)                            | Operabilidad   |
| RNF-08 | Analisis estatico de calidad (SonarQube + StyleCop) desde el inicio  | Calidad        |
| RNF-09 | Contenerizacion (Docker)                                             | Despliegue     |
| RNF-10 | Pipeline CI/CD (GitHub Actions)                                      | Automatizacion |
| RNF-11 | Logging centralizado y observable (ELK Stack)                        | Observabilidad |

---

## 3. Modelo de Datos

### Entidad: Appointment (Aggregate Root)

```
Appointment
├── Id              : Guid (generado automaticamente)
├── PlaceId         : int (requerido, debe ser taller activo)
├── AppointmentAt   : DateTime (requerido)
├── ServiceType     : string (requerido)
├── CreatedAt       : DateTime (generado automaticamente)
├── Contact         : Contact (requerido)
│   ├── Name        : string (requerido)
│   ├── Email       : string (requerido, formato valido)
│   └── Phone       : string (requerido)
└── Vehicle         : Vehicle? (opcional)
    ├── Make         : string
    ├── Model        : string
    ├── Year         : int
    └── LicensePlate : string
```

### Entidad Externa: Workshop (solo lectura, desde API Tecnom)

```
Workshop
├── Id              : int
├── Name            : string
├── Address         : string
├── Email           : string
├── Whatsapp        : string
└── Active          : bool
```

---

## 4. Reglas de Negocio

| ID    | Regla                                                                                    | Tipo       |
| ----- | ---------------------------------------------------------------------------------------- | ---------- |
| RN-01 | No se puede crear un turno sin `name`, `email`, `appointment_at`, `service_type`, `place_id` | Validacion |
| RN-02 | El `email` debe tener formato valido                                                     | Validacion |
| RN-03 | El `appointment_at` no debe ser una fecha pasada                                         | Validacion |
| RN-04 | El `place_id` debe corresponder a un taller que exista Y este activo en la API de Tecnom | Negocio    |
| RN-05 | Si se envia `vehicle`, los campos internos son libres (todos opcionales entre si)        | Validacion |
| RN-06 | Si se envia `license_plate`, debe tener formato valido (ej: formato patente argentina)   | Validacion |
| RN-07 | Solo se devuelven talleres activos en el endpoint GET de workshops                       | Filtrado   |

---

## 5. Arquitectura CQRS

### ¿Por que CQRS?

**Command Query Responsibility Segregation** separa las operaciones de lectura (Queries) de las de escritura (Commands), permitiendo:

| Beneficio | Descripcion |
|-----------|-------------|
| **Escalabilidad** | Lecturas y escrituras pueden escalar independientemente |
| **Single Responsibility** | Cada handler hace una sola cosa |
| **Testeable** | Handlers aislados, faciles de testear |
| **Mantenibilidad** | Agregar features sin tocar codigo existente |
| **Pipeline Behaviors** | Validacion, logging, caching como cross-cutting concerns |

### Flujo CQRS con MediatR

```
┌─────────────────────────────────────────────────────────────────┐
│                         Controller                               │
│                  (recibe HTTP request)                          │
└──────────────────────────┬──────────────────────────────────────┘
                           │
                           ▼
┌─────────────────────────────────────────────────────────────────┐
│                      MediatR.Send()                              │
│              (dispatcher de commands/queries)                    │
└──────────────────────────┬──────────────────────────────────────┘
                           │
                           ▼
┌─────────────────────────────────────────────────────────────────┐
│                   Pipeline Behaviors                             │
│         (ValidationBehavior -> LoggingBehavior -> ...)          │
└──────────────────────────┬──────────────────────────────────────┘
                           │
              ┌────────────┴────────────┐
              ▼                         ▼
┌─────────────────────────┐   ┌─────────────────────────┐
│    Command Handler      │   │     Query Handler       │
│   (modifica estado)     │   │    (solo lectura)       │
├─────────────────────────┤   ├─────────────────────────┤
│ - Valida con Domain     │   │ - Lee de DB/Cache       │
│ - Ejecuta logica        │   │ - Mapea a DTO           │
│ - Persiste cambios      │   │ - Retorna datos         │
└─────────────────────────┘   └─────────────────────────┘
```

### Commands vs Queries

| Tipo | Proposito | Ejemplo | Retorna |
|------|-----------|---------|---------|
| **Command** | Modificar estado | `CreateAppointmentCommand` | `ApiResponse<AppointmentDto>` |
| **Query** | Leer datos | `GetAppointmentsQuery` | `ApiResponse<List<AppointmentDto>>` |

### Pipeline Behaviors (Cross-Cutting Concerns)

| Behavior | Funcion |
|----------|---------|
| `ValidationBehavior` | Ejecuta FluentValidation antes del handler |
| `LoggingBehavior` | Loguea entrada/salida de cada request |
| `PerformanceBehavior` | Alerta si un request tarda mas de X ms |

---

### CU-01: Crear Turno (Command)

```
Actor: Cliente (Frontend Angular)
Precondiciones: Ninguna

Flujo principal:
  1. Cliente envia POST /api/appointments con datos del turno
  2. Sistema valida campos requeridos (RN-01, RN-02, RN-03)
  3. Sistema consulta API externa para verificar que place_id es taller activo (RN-04)
  4. Sistema guarda turno en memoria
  5. Sistema responde 201 Created con el turno creado

Flujos alternativos:
  3a. place_id no existe o no esta activo -> 400 Bad Request con mensaje claro
  2a. Campos requeridos faltantes -> 400 Bad Request con detalle de errores
  2b. Email con formato invalido -> 400 Bad Request
  2c. Fecha en el pasado -> 400 Bad Request
```

### CU-02: Listar Turnos (Query)

```
Actor: Cliente (Frontend Angular)
Precondiciones: Ninguna

Flujo principal:
  1. Cliente envia GET /api/appointments
  2. Sistema recupera todos los turnos de memoria
  3. Sistema responde 200 OK con coleccion de turnos

Flujo alternativo:
  2a. No hay turnos -> 200 OK con coleccion vacia
```

### CU-03: Obtener Talleres (Query)

```
Actor: Cliente (Frontend Angular)
Precondiciones: Ninguna

Flujo principal:
  1. Cliente envia GET /api/workshops
  2. Sistema verifica cache (Redis)
  3. Si no hay cache, consulta API externa con Basic Auth
  4. Sistema filtra solo talleres activos (RN-07)
  5. Sistema guarda en cache el resultado
  6. Sistema responde 200 OK con coleccion de talleres

Flujos alternativos:
  2a. Cache disponible -> Retorna desde cache (sin llamar API externa)
  3a. API externa no disponible -> Retry (Polly), si falla -> 503 Service Unavailable
  3b. Credenciales invalidas -> 502 Bad Gateway
```

---

## 6. Contrato de API

### `POST /api/appointments`

**Request:**
```json
{
  "place_id": 2222,
  "appointment_at": "2025-10-01T10:00:00Z",
  "service_type": "Mantenimiento",
  "contact": {
    "name": "Juan Perez",
    "email": "juan@email.com",
    "phone": "+5491155551234"
  },
  "vehicle": {
    "make": "Toyota",
    "model": "Corolla",
    "year": 2022,
    "license_plate": "AB123CD"
  }
}
```

**Response 201 Created:**
```json
{
  "success": true,
  "data": {
    "id": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
    "place_id": 2222,
    "appointment_at": "2025-10-01T10:00:00Z",
    "service_type": "Mantenimiento",
    "contact": { "name": "Juan Perez", "email": "juan@email.com", "phone": "+5491155551234" },
    "vehicle": { "make": "Toyota", "model": "Corolla", "year": 2022, "license_plate": "AB123CD" },
    "created_at": "2026-02-26T15:30:00Z"
  },
  "errors": null
}
```

**Response 400 Bad Request:**
```json
{
  "success": false,
  "data": null,
  "errors": [
    "El campo 'name' es requerido",
    "El place_id no corresponde a un taller activo"
  ]
}
```

### `GET /api/appointments`

**Response 200 OK:**
```json
{
  "success": true,
  "data": [
    {
      "id": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
      "place_id": 2222,
      "appointment_at": "2025-10-01T10:00:00Z",
      "service_type": "Mantenimiento",
      "contact": { "name": "Juan Perez", "email": "juan@email.com", "phone": "+5491155551234" },
      "vehicle": { "make": "Toyota", "model": "Corolla", "year": 2022, "license_plate": "AB123CD" },
      "created_at": "2026-02-26T15:30:00Z"
    }
  ],
  "errors": null
}
```

### `GET /api/workshops`

**Response 200 OK:**
```json
{
  "success": true,
  "data": [
    {
      "id": 2222,
      "name": "Taller Norte",
      "address": "Av. Siempreviva 742",
      "email": "contacto@tallernorte.com",
      "whatsapp": "+5491155559999"
    }
  ],
  "errors": null
}
```

### `GET /health`

**Response 200 OK:**
```json
{
  "status": "Healthy",
  "checks": {
    "self": "Healthy",
    "redis": "Healthy",
    "elasticsearch": "Healthy",
    "tecnom-api": "Healthy"
  }
}
```

---

## 7. Stack Tecnologico

| Area               | Tecnologia                                                             |
| ------------------ | ---------------------------------------------------------------------- |
| **Runtime**        | .NET 10, C# 14                                                         |
| **API**            | ASP.NET Core Web API (Controllers)                                     |
| **Arquitectura**   | Clean Architecture + DDD + CQRS (MediatR)                              |
| **Validacion**     | FluentValidation + Value Objects                                       |
| **Mapping**        | Mapster                                                                |
| **Persistencia**   | EF Core InMemory                                                       |
| **HTTP Client**    | IHttpClientFactory (Typed Client)                                      |
| **Resiliencia**    | Polly (Retry, Circuit Breaker, Timeout)                                |
| **Caching**        | Redis (contenedor Docker) con IDistributedCache                        |
| **Docs API**       | Swagger/OpenAPI                                                        |
| **Logging**        | Serilog + ELK Stack (Elasticsearch, Kibana)                            |
| **Tests**          | xUnit + Moq + FluentAssertions + Coverlet                              |
| **Calidad Codigo** | SonarQube (Docker) + StyleCop.Analyzers + .editorconfig                |
| **Proyecto**       | Directory.Build.props (config centralizada)                            |
| **Monitoreo**      | Health Checks (API, Redis, Elasticsearch, API externa Tecnom)          |
| **Errores**        | Global Exception Middleware + Domain Exceptions                        |
| **Config**         | Options Pattern (appsettings.json)                                     |
| **Contenedores**   | Dockerfile + docker-compose (API + Redis + ELK + SonarQube + Postgres) |
| **CI/CD**          | GitHub Actions (build, test, analisis)                                 |
| **Git**            | Conventional Commits, .gitattributes, .gitignore                       |

---

## 8. Estructura del Proyecto (Clean Architecture + DDD + CQRS)

```
QuickReserveBackEnd/
│
├── src/
│   ├── QuickReserve.Domain/              # Corazon del dominio (sin dependencias externas)
│   │   ├── Entities/
│   │   │   ├── Appointment.cs            # Aggregate Root
│   │   │   ├── Contact.cs                # Entity
│   │   │   └── Vehicle.cs                # Entity
│   │   ├── ValueObjects/
│   │   │   ├── Email.cs
│   │   │   ├── Phone.cs
│   │   │   ├── LicensePlate.cs
│   │   │   └── ServiceType.cs
│   │   ├── Exceptions/
│   │   │   ├── DomainException.cs
│   │   │   └── InvalidWorkshopException.cs
│   │   ├── Interfaces/
│   │   │   ├── IAppointmentRepository.cs
│   │   │   └── IWorkshopService.cs       # Puerto hacia infraestructura
│   │   └── Services/
│   │       └── AppointmentDomainService.cs
│   │
│   ├── QuickReserve.Application/         # CQRS con MediatR
│   │   ├── Common/
│   │   │   ├── Interfaces/
│   │   │   │   └── IApplicationDbContext.cs
│   │   │   ├── Behaviors/                # MediatR Pipeline Behaviors
│   │   │   │   ├── ValidationBehavior.cs
│   │   │   │   ├── LoggingBehavior.cs
│   │   │   │   └── PerformanceBehavior.cs
│   │   │   ├── Exceptions/
│   │   │   │   └── ValidationException.cs
│   │   │   └── Models/
│   │   │       └── ApiResponse.cs        # Wrapper generico
│   │   │
│   │   ├── Features/                     # Vertical Slices por Feature
│   │   │   ├── Appointments/
│   │   │   │   ├── Commands/
│   │   │   │   │   └── CreateAppointment/
│   │   │   │   │       ├── CreateAppointmentCommand.cs
│   │   │   │   │       ├── CreateAppointmentCommandHandler.cs
│   │   │   │   │       └── CreateAppointmentCommandValidator.cs
│   │   │   │   └── Queries/
│   │   │   │       ├── GetAppointments/
│   │   │   │       │   ├── GetAppointmentsQuery.cs
│   │   │   │       │   ├── GetAppointmentsQueryHandler.cs
│   │   │   │       │   └── AppointmentDto.cs
│   │   │   │       └── GetAppointmentById/
│   │   │   │           ├── GetAppointmentByIdQuery.cs
│   │   │   │           └── GetAppointmentByIdQueryHandler.cs
│   │   │   │
│   │   │   └── Workshops/
│   │   │       └── Queries/
│   │   │           └── GetWorkshops/
│   │   │               ├── GetWorkshopsQuery.cs
│   │   │               ├── GetWorkshopsQueryHandler.cs
│   │   │               └── WorkshopDto.cs
│   │   │
│   │   ├── Mappings/
│   │   │   └── MappingConfig.cs
│   │   │
│   │   └── DependencyInjection.cs
│   │
│   ├── QuickReserve.Infrastructure/      # Implementaciones concretas
│   │   ├── Persistence/
│   │   │   ├── AppDbContext.cs
│   │   │   ├── Configurations/           # EF Core Fluent Config
│   │   │   │   └── AppointmentConfiguration.cs
│   │   │   └── Repositories/
│   │   │       └── AppointmentRepository.cs
│   │   ├── ExternalServices/
│   │   │   ├── TecnomApiClient.cs        # Typed HttpClient
│   │   │   └── Models/
│   │   │       └── TecnomWorkshopDto.cs  # Modelo de la API externa
│   │   ├── Caching/
│   │   │   └── CachedWorkshopService.cs  # Decorator con Redis
│   │   └── Configuration/
│   │       └── TecnomApiSettings.cs      # Options Pattern
│   │
│   └── QuickReserve.API/                 # Capa de presentacion
│       ├── Controllers/
│       │   ├── AppointmentsController.cs
│       │   └── WorkshopsController.cs
│       ├── Middleware/
│       │   └── GlobalExceptionMiddleware.cs
│       ├── Extensions/
│       │   └── ServiceCollectionExtensions.cs  # DI registration
│       ├── Program.cs
│       └── appsettings.json
│
├── tests/
│   └── QuickReserve.Tests/
│       ├── Domain/
│       │   ├── ValueObjects/
│       │   └── Entities/
│       ├── Application/
│       │   ├── Validators/
│       │   └── Services/
│       └── Integration/
│
├── docker/
│   ├── docker-compose.yml
│   ├── docker-compose.override.yml
│   └── sonar/
│       └── sonar-project.properties
│
├── .github/
│   └── workflows/
│       ├── build-and-test.yml
│       └── sonar-analysis.yml
│
├── Dockerfile
├── Directory.Build.props
├── .editorconfig
├── .gitignore
├── .gitattributes
├── README.md
└── QuickReserve.sln
```

---

## 9. Plan de Trabajo - 10 Fases (Shift-Left Quality)

> **Principio Shift-Left:** La calidad se integra desde el inicio, no al final.
> SonarQube, Docker y herramientas de calidad se configuran en la Fase 1.

### Fase 1 - Setup + Infraestructura de Calidad (Fundacion)

| Item | Descripcion |
|------|-------------|
| 1.1 | Crear solution y 5 proyectos (Domain, Application, Infrastructure, API, Tests) |
| 1.2 | Configurar `Directory.Build.props` (StyleCop.Analyzers, versiones, analizadores) |
| 1.3 | Crear `.editorconfig` con reglas de estilo |
| 1.4 | Crear `.gitignore`, `.gitattributes` |
| 1.5 | Establecer referencias entre proyectos |
| 1.6 | Instalar paquetes NuGet base |
| 1.7 | **Crear `docker-compose.yml`** con SonarQube + PostgreSQL + Redis + ELK |
| 1.8 | **Configurar `sonar-project.properties`** |
| 1.9 | **Ejecutar primer analisis SonarQube** (baseline con 0 codigo) |
| 1.10 | Inicializar Git + primer commit con Conventional Commits |
| 1.11 | Crear README.md basico |

**Prioridad:** Alta
**Entregable:** Proyecto compilable con infraestructura de calidad funcionando

---

### Fase 2 - Capa Domain (DDD)

| Item | Descripcion |
|------|-------------|
| 2.1 | Value Objects: `Email`, `Phone`, `LicensePlate`, `ServiceType` |
| 2.2 | Domain Exceptions: `DomainException`, `InvalidWorkshopException`, etc. |
| 2.3 | Entities: `Contact`, `Vehicle` |
| 2.4 | Aggregate Root: `Appointment` |
| 2.5 | Interfaces/Puertos: `IAppointmentRepository`, `IWorkshopService` |
| 2.6 | Domain Service: `AppointmentDomainService` |
| 2.7 | **Tests unitarios de Value Objects y Entities** |
| 2.8 | Ejecutar analisis SonarQube |

**Prioridad:** Alta
**Entregable:** Dominio completo con tests

---

### Fase 3 - Capa Application

| Item | Descripcion |
|------|-------------|
| 3.1 | DTOs Request: `CreateAppointmentRequest`, `ContactRequest`, `VehicleRequest` |
| 3.2 | DTOs Response: `ApiResponse<T>`, `AppointmentResponse`, `WorkshopResponse` |
| 3.3 | FluentValidation: `CreateAppointmentValidator`, `ContactRequestValidator` |
| 3.4 | Mapster configuration |
| 3.5 | Application Services: `AppointmentAppService`, `WorkshopAppService` |
| 3.6 | `DependencyInjection.cs` |
| 3.7 | **Tests unitarios de Validators y Services** |
| 3.8 | Ejecutar analisis SonarQube |

**Prioridad:** Alta
**Entregable:** Capa de aplicacion completa con tests

---

### Fase 4 - Capa Infrastructure

| Item | Descripcion |
|------|-------------|
| 4.1 | `AppDbContext` con EF Core InMemory |
| 4.2 | Entity Configurations (Fluent API) |
| 4.3 | `AppointmentRepository` |
| 4.4 | `TecnomApiSettings` (Options Pattern) |
| 4.5 | `TecnomApiClient` (Typed HttpClient + Basic Auth) |
| 4.6 | Polly policies (Retry, Circuit Breaker, Timeout) |
| 4.7 | `CachedWorkshopService` (Decorator con Redis) |
| 4.8 | `DependencyInjection.cs` |
| 4.9 | **Tests de Repository y HttpClient** |
| 4.10 | Ejecutar analisis SonarQube |

**Prioridad:** Alta
**Entregable:** Infraestructura completa con tests

---

### Fase 5 - Capa API

| Item | Descripcion |
|------|-------------|
| 5.1 | `AppointmentsController` (GET + POST) |
| 5.2 | `WorkshopsController` (GET) |
| 5.3 | `GlobalExceptionMiddleware` |
| 5.4 | `CorrelationIdMiddleware` |
| 5.5 | CORS configuration |
| 5.6 | Swagger/OpenAPI con XML comments |
| 5.7 | Serilog + Elasticsearch sink |
| 5.8 | Health Checks (self, Redis, Elasticsearch, Tecnom API) |
| 5.9 | `Program.cs` con DI completo |
| 5.10 | `appsettings.json` y `appsettings.Development.json` |
| 5.11 | **Tests de integracion de Controllers** |
| 5.12 | Ejecutar analisis SonarQube |

**Prioridad:** Alta
**Entregable:** API funcional completa con tests

---

### Fase 6 - Dockerfile y Optimizacion Docker

| Item | Descripcion |
|------|-------------|
| 6.1 | Crear `Dockerfile` multi-stage optimizado |
| 6.2 | Agregar servicio `quickreserve-api` a docker-compose |
| 6.3 | Crear `docker-compose.override.yml` para desarrollo |
| 6.4 | Crear `.dockerignore` |
| 6.5 | Verificar que todos los servicios levantan correctamente |
| 6.6 | Probar API containerizada contra Redis y ELK |

**Prioridad:** Media
**Entregable:** API corriendo en Docker con todos los servicios

---

### Fase 7 - CI/CD GitHub Actions

| Item | Descripcion |
|------|-------------|
| 7.1 | Workflow `build-and-test.yml` (restore, build, test con cobertura) |
| 7.2 | Workflow `sonar-analysis.yml` (analisis de calidad) |
| 7.3 | Cache de paquetes NuGet |
| 7.4 | Badges en README.md |
| 7.5 | Branch protection rules (opcional) |

**Prioridad:** Media
**Entregable:** CI/CD automatizado en cada push/PR

---

### Fase 8 - HTTP Files y Documentacion

| Item | Descripcion |
|------|-------------|
| 8.1 | Crear `requests.http` con ejemplos de todos los endpoints |
| 8.2 | Agregar XML comments en Controllers y DTOs publicos |
| 8.3 | Completar README.md con instrucciones de uso |
| 8.4 | Documentar decisiones arquitectonicas (ADRs opcionales) |

**Prioridad:** Media
**Entregable:** Documentacion completa

---

### Fase 9 - Cobertura y Calidad Final

| Item | Descripcion |
|------|-------------|
| 9.1 | Revisar cobertura de tests (objetivo: >80%) |
| 9.2 | Completar tests faltantes |
| 9.3 | Ejecutar analisis SonarQube final |
| 9.4 | Resolver code smells y vulnerabilidades |
| 9.5 | Verificar Quality Gate: PASSED |

**Prioridad:** Alta
**Entregable:** Codigo con calidad verificada

---

### Fase 10 - Validacion Final y Entrega

| Item | Descripcion |
|------|-------------|
| 10.1 | `dotnet build` sin warnings |
| 10.2 | `dotnet test` todos pasan |
| 10.3 | `docker-compose up` levanta todos los servicios |
| 10.4 | Probar endpoints en Swagger |
| 10.5 | Verificar Health Checks (`/health`) |
| 10.6 | Verificar logs en Kibana |
| 10.7 | Verificar SonarQube dashboard (0 bugs, 0 vulnerabilities) |
| 10.8 | Revisar historial de commits (Conventional Commits) |
| 10.9 | Compartir acceso a GitHub con evaluadores |

**Prioridad:** Alta
**Entregable:** Proyecto listo para evaluacion

---

## 10. Dependencias entre Fases

```
┌─────────────────────────────────────────────────────────────────┐
│  FASE 1: Setup + Docker + SonarQube (Fundacion de Calidad)      │
└─────────────────────────┬───────────────────────────────────────┘
                          │
                          ▼
┌─────────────────────────────────────────────────────────────────┐
│  FASE 2: Domain + Tests Domain                                  │
└─────────────────────────┬───────────────────────────────────────┘
                          │
                          ▼
┌─────────────────────────────────────────────────────────────────┐
│  FASE 3: Application + Tests Application                        │
└─────────────────────────┬───────────────────────────────────────┘
                          │
                          ▼
┌─────────────────────────────────────────────────────────────────┐
│  FASE 4: Infrastructure + Tests Infrastructure                  │
└─────────────────────────┬───────────────────────────────────────┘
                          │
                          ▼
┌─────────────────────────────────────────────────────────────────┐
│  FASE 5: API + Tests Integration                                │
└─────────────────────────┬───────────────────────────────────────┘
                          │
            ┌─────────────┴─────────────┐
            ▼                           ▼
┌───────────────────────┐   ┌───────────────────────┐
│  FASE 6: Dockerfile   │   │  FASE 7: CI/CD        │
└───────────┬───────────┘   └───────────┬───────────┘
            │                           │
            └─────────────┬─────────────┘
                          ▼
┌─────────────────────────────────────────────────────────────────┐
│  FASE 8: HTTP Files + Documentacion                             │
└─────────────────────────┬───────────────────────────────────────┘
                          │
                          ▼
┌─────────────────────────────────────────────────────────────────┐
│  FASE 9: Cobertura + Calidad Final                              │
└─────────────────────────┬───────────────────────────────────────┘
                          │
                          ▼
┌─────────────────────────────────────────────────────────────────┐
│  FASE 10: Validacion Final + Entrega                            │
└─────────────────────────────────────────────────────────────────┘
```

---

## 11. Docker Compose - Servicios

```yaml
services:
  quickreserve-api:    # .NET 10 API (puerto 5000/5001)
  redis:               # Redis 7 (puerto 6379)
  elasticsearch:       # Elasticsearch 8 (puerto 9200)
  kibana:              # Kibana 8 (puerto 5601)
  sonarqube:           # SonarQube Community (puerto 9000)
  sonar-db:            # PostgreSQL 16 (para SonarQube)
```

---

## 12. Sistema de Logging (Serilog + ELK)

### Componentes del Stack

| Componente        | Descripcion                            | Puerto | Funcion                                     |
| ----------------- | -------------------------------------- | ------ | ------------------------------------------- |
| **Serilog**       | Libreria de logging estructurado .NET  | -      | Genera logs estructurados desde la app      |
| **Elasticsearch** | Motor de busqueda y almacenamiento     | 9200   | Almacena y permite buscar logs              |
| **Kibana**        | UI web de visualizacion                | 5601   | Dashboards, busqueda y analisis visual      |

### Paquetes NuGet

```
Serilog.AspNetCore
Serilog.Sinks.Elasticsearch
Serilog.Sinks.Console
Serilog.Sinks.File
Serilog.Enrichers.Environment
Serilog.Enrichers.Process
Serilog.Enrichers.Thread
Serilog.Enrichers.CorrelationId
```

### Features de Logging

| Feature                      | Descripcion                                                                 |
| ---------------------------- | --------------------------------------------------------------------------- |
| **Structured Logging**       | Logs con propiedades tipadas, no solo strings                               |
| **Correlation ID**           | Header `X-Correlation-ID` para tracking de requests end-to-end              |
| **Request/Response Logging** | Middleware que loguea cada HTTP request con metodo, path, status, duracion  |
| **Performance Logging**      | Tiempo de ejecucion de cada endpoint                                        |
| **Enrichers**                | Contexto automatico: Machine, Environment, Thread, Process, Timestamp       |
| **Index por dia**            | `quickreserve-logs-{yyyy.MM.dd}` para rotacion automatica en Elasticsearch  |
| **Multiple Sinks**           | Console (desarrollo) + Elasticsearch (analisis) + File (backup)             |
| **Log Levels**               | Verbose, Debug, Information, Warning, Error, Fatal                          |

### Configuracion en appsettings.json

```json
{
  "Serilog": {
    "MinimumLevel": {
      "Default": "Information",
      "Override": {
        "Microsoft": "Warning",
        "System": "Warning",
        "Microsoft.Hosting.Lifetime": "Information"
      }
    },
    "WriteTo": [
      { "Name": "Console" },
      {
        "Name": "Elasticsearch",
        "Args": {
          "nodeUris": "http://elasticsearch:9200",
          "indexFormat": "quickreserve-logs-{0:yyyy.MM.dd}",
          "autoRegisterTemplate": true,
          "autoRegisterTemplateVersion": "ESv8"
        }
      }
    ],
    "Enrich": ["FromLogContext", "WithMachineName", "WithThreadId", "WithCorrelationId"]
  }
}
```

### Kibana Dashboards Sugeridos

| Dashboard            | Metricas                                                |
| -------------------- | ------------------------------------------------------- |
| **Request Overview** | Total requests, requests/min, status codes distribution |
| **Error Tracking**   | Errores por tipo, stack traces, tendencia de errores    |
| **Performance**      | Latencia promedio, P95, P99, endpoints mas lentos       |
| **Business Metrics** | Turnos creados por dia, talleres mas consultados        |

### Ejemplo de Log Estructurado

```json
{
  "@timestamp": "2026-02-26T15:30:00.000Z",
  "level": "Information",
  "messageTemplate": "HTTP {RequestMethod} {RequestPath} responded {StatusCode} in {Elapsed:0.0000} ms",
  "message": "HTTP POST /api/appointments responded 201 in 45.2300 ms",
  "fields": {
    "RequestMethod": "POST",
    "RequestPath": "/api/appointments",
    "StatusCode": 201,
    "Elapsed": 45.23,
    "CorrelationId": "abc-123-def-456",
    "MachineName": "quickreserve-api-1",
    "Environment": "Development"
  }
}
```

---

## 13. API Externa Tecnom

- **URL:** `https://dev.tecnomcrm.com/api/v1/places/workshops`
- **Auth:** Basic Authentication
  - User: `REDACTED_USERNAME`
  - Pass: `REDACTED_PASSWORD`

---

## 14. Repositorio GitHub

Compartir acceso a:
- `alainico1`
- `matiasguazzaroni`
- `mngobbi`

---

## 15. Resumen de Puertos (Docker)

| Servicio             | Puerto                   | URL Local                |
| -------------------- | ------------------------ | ------------------------ |
| QuickReserve API     | 5000 (HTTP) / 5001 (HTTPS) | http://localhost:5000    |
| Redis                | 6379                     | redis://localhost:6379   |
| Elasticsearch        | 9200                     | http://localhost:9200    |
| Kibana               | 5601                     | http://localhost:5601    |
| SonarQube            | 9000                     | http://localhost:9000    |
| PostgreSQL (Sonar)   | 5432                     | localhost:5432           |

---

## 16. Checklist de Entrega

- [ ] Codigo fuente en GitHub (publico o privado con acceso)
- [ ] README.md con instrucciones de ejecucion
- [ ] docker-compose funcional
- [ ] Tests con cobertura >80%
- [ ] SonarQube Quality Gate: PASSED
- [ ] Swagger UI funcionando
- [ ] Health Checks respondiendo
- [ ] Logs visibles en Kibana
- [ ] Conventional Commits en historial

---

## Tags

#QuickReserve #Backend #DotNet #CleanArchitecture #DDD #Challenge #Tecnom #ELK #Serilog #SonarQube #ShiftLeft
