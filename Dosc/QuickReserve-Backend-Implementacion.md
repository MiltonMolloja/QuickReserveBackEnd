# QuickReserve Backend - Guia de Implementacion

> **Documento tecnico de implementacion paso a paso**
> Basado en: [[QuickReserve-Backend-Plan]]
> Fecha: 2026-02-26
> Version: 3.0 (Clean Architecture + DDD + CQRS + Shift-Left Quality)

---

## Indice

1. [Fase 1 - Setup + Infraestructura de Calidad](#fase-1---setup--infraestructura-de-calidad)
2. [Fase 2 - Capa Domain (DDD) + Tests](#fase-2---capa-domain-ddd--tests)
3. [Fase 3 - Capa Application + Tests](#fase-3---capa-application--tests)
4. [Fase 4 - Capa Infrastructure + Tests](#fase-4---capa-infrastructure--tests)
5. [Fase 5 - Capa API + Tests Integration](#fase-5---capa-api--tests-integration)
6. [Fase 6 - Dockerfile y Optimizacion Docker](#fase-6---dockerfile-y-optimizacion-docker)
7. [Fase 7 - CI/CD GitHub Actions](#fase-7---cicd-github-actions)
8. [Fase 8 - HTTP Files y Documentacion](#fase-8---http-files-y-documentacion)
9. [Fase 9 - Cobertura y Calidad Final](#fase-9---cobertura-y-calidad-final)
10. [Fase 10 - Validacion Final y Entrega](#fase-10---validacion-final-y-entrega)

> **Nota:** Los tests se escriben junto con cada capa (no al final).
> SonarQube y Docker se configuran en la Fase 1 para tener feedback de calidad desde el inicio.

---

## Fase 1 - Setup + Infraestructura de Calidad

> **Objetivo:** Crear la base del proyecto CON infraestructura de calidad desde el dia 1.
> Al finalizar esta fase tendras: proyecto compilable + SonarQube + Redis + ELK funcionando.

### 1.1 Crear estructura de directorios

```bash
mkdir QuickReserveBackEnd
cd QuickReserveBackEnd
mkdir src tests
```

### 1.2 Crear Solution y Proyectos

```bash
# Crear solution
dotnet new sln -n QuickReserve

# Crear proyectos
dotnet new classlib -n QuickReserve.Domain -o src/QuickReserve.Domain
dotnet new classlib -n QuickReserve.Application -o src/QuickReserve.Application
dotnet new classlib -n QuickReserve.Infrastructure -o src/QuickReserve.Infrastructure
dotnet new webapi -n QuickReserve.API -o src/QuickReserve.API
dotnet new xunit -n QuickReserve.Tests -o tests/QuickReserve.Tests

# Agregar proyectos a la solution
dotnet sln add src/QuickReserve.Domain/QuickReserve.Domain.csproj
dotnet sln add src/QuickReserve.Application/QuickReserve.Application.csproj
dotnet sln add src/QuickReserve.Infrastructure/QuickReserve.Infrastructure.csproj
dotnet sln add src/QuickReserve.API/QuickReserve.API.csproj
dotnet sln add tests/QuickReserve.Tests/QuickReserve.Tests.csproj
```

### 1.3 Establecer referencias entre proyectos

```bash
# Domain no tiene referencias (es el core)

# Application referencia Domain
dotnet add src/QuickReserve.Application/QuickReserve.Application.csproj reference src/QuickReserve.Domain/QuickReserve.Domain.csproj

# Infrastructure referencia Domain y Application
dotnet add src/QuickReserve.Infrastructure/QuickReserve.Infrastructure.csproj reference src/QuickReserve.Domain/QuickReserve.Domain.csproj
dotnet add src/QuickReserve.Infrastructure/QuickReserve.Infrastructure.csproj reference src/QuickReserve.Application/QuickReserve.Application.csproj

# API referencia Application e Infrastructure
dotnet add src/QuickReserve.API/QuickReserve.API.csproj reference src/QuickReserve.Application/QuickReserve.Application.csproj
dotnet add src/QuickReserve.API/QuickReserve.API.csproj reference src/QuickReserve.Infrastructure/QuickReserve.Infrastructure.csproj

# Tests referencia todos
dotnet add tests/QuickReserve.Tests/QuickReserve.Tests.csproj reference src/QuickReserve.Domain/QuickReserve.Domain.csproj
dotnet add tests/QuickReserve.Tests/QuickReserve.Tests.csproj reference src/QuickReserve.Application/QuickReserve.Application.csproj
dotnet add tests/QuickReserve.Tests/QuickReserve.Tests.csproj reference src/QuickReserve.Infrastructure/QuickReserve.Infrastructure.csproj
dotnet add tests/QuickReserve.Tests/QuickReserve.Tests.csproj reference src/QuickReserve.API/QuickReserve.API.csproj
```

### 1.4 Crear Directory.Build.props (raiz del proyecto)

```xml
<Project>
  <PropertyGroup>
    <TargetFramework>net10.0</TargetFramework>
    <ImplicitUsings>enable</ImplicitUsings>
    <Nullable>enable</Nullable>
    <TreatWarningsAsErrors>true</TreatWarningsAsErrors>
    <AnalysisLevel>latest</AnalysisLevel>
    <EnforceCodeStyleInBuild>true</EnforceCodeStyleInBuild>
  </PropertyGroup>

  <!-- Versionado centralizado -->
  <PropertyGroup>
    <Version>1.0.0</Version>
    <Authors>Milton</Authors>
    <Company>QuickReserve</Company>
  </PropertyGroup>

  <!-- Analizadores de codigo -->
  <ItemGroup>
    <PackageReference Include="StyleCop.Analyzers" Version="1.2.0-beta.556">
      <PrivateAssets>all</PrivateAssets>
      <IncludeAssets>runtime; build; native; contentfiles; analyzers</IncludeAssets>
    </PackageReference>
  </ItemGroup>

  <!-- Configuracion de documentacion XML -->
  <PropertyGroup>
    <GenerateDocumentationFile>true</GenerateDocumentationFile>
    <NoWarn>$(NoWarn);1591</NoWarn>
  </PropertyGroup>
</Project>
```

### 1.5 Crear .editorconfig (raiz del proyecto)

```ini
# EditorConfig - QuickReserve
root = true

[*]
indent_style = space
indent_size = 4
end_of_line = lf
charset = utf-8
trim_trailing_whitespace = true
insert_final_newline = true

[*.{cs,csx}]
# Organizar usings
dotnet_sort_system_directives_first = true
dotnet_separate_import_directive_groups = false

# Preferencias de this.
dotnet_style_qualification_for_field = false:suggestion
dotnet_style_qualification_for_property = false:suggestion
dotnet_style_qualification_for_method = false:suggestion
dotnet_style_qualification_for_event = false:suggestion

# Preferencias de tipos
dotnet_style_predefined_type_for_locals_parameters_members = true:suggestion
dotnet_style_predefined_type_for_member_access = true:suggestion

# Preferencias de modificadores
dotnet_style_require_accessibility_modifiers = for_non_interface_members:warning
csharp_preferred_modifier_order = public,private,protected,internal,static,extern,new,virtual,abstract,sealed,override,readonly,unsafe,volatile,async:suggestion

# Preferencias de expresiones
dotnet_style_object_initializer = true:suggestion
dotnet_style_collection_initializer = true:suggestion
dotnet_style_explicit_tuple_names = true:suggestion
dotnet_style_prefer_inferred_tuple_names = true:suggestion
dotnet_style_prefer_inferred_anonymous_type_member_names = true:suggestion
dotnet_style_prefer_auto_properties = true:suggestion
dotnet_style_prefer_conditional_expression_over_assignment = true:suggestion
dotnet_style_prefer_conditional_expression_over_return = true:suggestion

# Preferencias de null checking
dotnet_style_coalesce_expression = true:suggestion
dotnet_style_null_propagation = true:suggestion
dotnet_style_prefer_is_null_check_over_reference_equality_method = true:suggestion

# C# Preferencias de estilo
csharp_style_var_for_built_in_types = true:suggestion
csharp_style_var_when_type_is_apparent = true:suggestion
csharp_style_var_elsewhere = true:suggestion

# Preferencias de expresiones
csharp_style_expression_bodied_methods = when_on_single_line:suggestion
csharp_style_expression_bodied_constructors = false:suggestion
csharp_style_expression_bodied_operators = when_on_single_line:suggestion
csharp_style_expression_bodied_properties = true:suggestion
csharp_style_expression_bodied_indexers = true:suggestion
csharp_style_expression_bodied_accessors = true:suggestion
csharp_style_expression_bodied_lambdas = true:suggestion
csharp_style_expression_bodied_local_functions = when_on_single_line:suggestion

# Pattern matching
csharp_style_pattern_matching_over_is_with_cast_check = true:suggestion
csharp_style_pattern_matching_over_as_with_null_check = true:suggestion

# Inlined variable declarations
csharp_style_inlined_variable_declaration = true:suggestion

# Null checking
csharp_style_throw_expression = true:suggestion
csharp_style_conditional_delegate_call = true:suggestion

# Preferencias de bloques de codigo
csharp_prefer_braces = true:warning
csharp_prefer_simple_using_statement = true:suggestion

# Namespaces
csharp_style_namespace_declarations = file_scoped:warning

# Nuevas lineas
csharp_new_line_before_open_brace = all
csharp_new_line_before_else = true
csharp_new_line_before_catch = true
csharp_new_line_before_finally = true
csharp_new_line_before_members_in_object_initializers = true
csharp_new_line_before_members_in_anonymous_types = true
csharp_new_line_between_query_expression_clauses = true

# Indentacion
csharp_indent_case_contents = true
csharp_indent_switch_labels = true
csharp_indent_labels = one_less_than_current

# Espaciado
csharp_space_after_cast = false
csharp_space_after_keywords_in_control_flow_statements = true
csharp_space_between_parentheses = false
csharp_space_before_colon_in_inheritance_clause = true
csharp_space_after_colon_in_inheritance_clause = true
csharp_space_around_binary_operators = before_and_after
csharp_space_between_method_declaration_parameter_list_parentheses = false
csharp_space_between_method_declaration_empty_parameter_list_parentheses = false
csharp_space_between_method_declaration_name_and_open_parenthesis = false
csharp_space_between_method_call_parameter_list_parentheses = false
csharp_space_between_method_call_empty_parameter_list_parentheses = false
csharp_space_between_method_call_name_and_opening_parenthesis = false
csharp_space_after_comma = true
csharp_space_before_comma = false
csharp_space_after_dot = false
csharp_space_before_dot = false
csharp_space_after_semicolon_in_for_statement = true
csharp_space_before_semicolon_in_for_statement = false
csharp_space_around_declaration_statements = false
csharp_space_before_open_square_brackets = false
csharp_space_between_empty_square_brackets = false
csharp_space_between_square_brackets = false

# Wrapping
csharp_preserve_single_line_statements = false
csharp_preserve_single_line_blocks = true

# Naming conventions
dotnet_naming_rule.interface_should_be_begins_with_i.severity = warning
dotnet_naming_rule.interface_should_be_begins_with_i.symbols = interface
dotnet_naming_rule.interface_should_be_begins_with_i.style = begins_with_i

dotnet_naming_rule.types_should_be_pascal_case.severity = warning
dotnet_naming_rule.types_should_be_pascal_case.symbols = types
dotnet_naming_rule.types_should_be_pascal_case.style = pascal_case

dotnet_naming_rule.private_fields_should_be_camel_case_with_underscore.severity = warning
dotnet_naming_rule.private_fields_should_be_camel_case_with_underscore.symbols = private_fields
dotnet_naming_rule.private_fields_should_be_camel_case_with_underscore.style = camel_case_with_underscore

dotnet_naming_symbols.interface.applicable_kinds = interface
dotnet_naming_symbols.types.applicable_kinds = class, struct, interface, enum
dotnet_naming_symbols.private_fields.applicable_kinds = field
dotnet_naming_symbols.private_fields.applicable_accessibilities = private

dotnet_naming_style.begins_with_i.required_prefix = I
dotnet_naming_style.begins_with_i.capitalization = pascal_case
dotnet_naming_style.pascal_case.capitalization = pascal_case
dotnet_naming_style.camel_case_with_underscore.required_prefix = _
dotnet_naming_style.camel_case_with_underscore.capitalization = camel_case

[*.json]
indent_size = 2

[*.{yml,yaml}]
indent_size = 2

[*.md]
trim_trailing_whitespace = false
```

### 1.6 Crear .gitignore

```gitignore
## .NET
bin/
obj/
*.user
*.userosscache
*.suo
*.cache
*.dll
*.exe
*.pdb

## IDE
.vs/
.vscode/
.idea/
*.swp
*~

## Rider
.idea/

## User-specific files
*.rsuser
*.suo
*.user
*.userosscache
*.sln.docstates

## Build results
[Dd]ebug/
[Rr]elease/
x64/
x86/
[Aa][Rr][Mm]/
[Aa][Rr][Mm]64/
bld/
[Bb]in/
[Oo]bj/
[Ll]og/
[Ll]ogs/

## NuGet
*.nupkg
*.snupkg
.nuget/
packages/
project.lock.json
project.fragment.lock.json
artifacts/

## Test Results
[Tt]est[Rr]esult*/
[Bb]uild[Ll]og.*
*.trx
*.coverage
*.coveragexml
coverage*.json
coverage*.xml
coverage*.info

## Docker
.docker/

## Secrets
*.pfx
*.key
appsettings.*.json
!appsettings.json
!appsettings.Development.json

## OS
.DS_Store
Thumbs.db

## Logs
logs/
*.log

## Sonar
.sonarqube/
.scannerwork/
```

### 1.7 Crear .gitattributes

```gitattributes
# Auto detect text files and perform LF normalization
* text=auto

# C# files
*.cs text diff=csharp

# Project files
*.sln text eol=crlf
*.csproj text eol=lf
*.props text eol=lf
*.targets text eol=lf

# Config files
*.json text eol=lf
*.yml text eol=lf
*.yaml text eol=lf
*.xml text eol=lf
*.config text eol=lf

# Shell scripts
*.sh text eol=lf
*.bash text eol=lf

# Windows scripts
*.cmd text eol=crlf
*.bat text eol=crlf
*.ps1 text eol=crlf

# Docker
Dockerfile text eol=lf
*.dockerfile text eol=lf
docker-compose*.yml text eol=lf

# Documentation
*.md text eol=lf
LICENSE text eol=lf

# Binary files
*.png binary
*.jpg binary
*.jpeg binary
*.gif binary
*.ico binary
*.pdf binary
```

### 1.8 Instalar paquetes NuGet por proyecto

#### QuickReserve.Domain (sin dependencias externas)
```bash
# No requiere paquetes externos - es el core puro
```

#### QuickReserve.Application
```bash
cd src/QuickReserve.Application
dotnet add package MediatR --version 12.*
dotnet add package FluentValidation --version 11.*
dotnet add package FluentValidation.DependencyInjectionExtensions --version 11.*
dotnet add package Mapster --version 7.*
dotnet add package Mapster.DependencyInjection --version 1.*
```

#### QuickReserve.Infrastructure
```bash
cd src/QuickReserve.Infrastructure
dotnet add package Microsoft.EntityFrameworkCore --version 10.*
dotnet add package Microsoft.EntityFrameworkCore.InMemory --version 10.*
dotnet add package Microsoft.Extensions.Http.Polly --version 10.*
dotnet add package Microsoft.Extensions.Caching.StackExchangeRedis --version 10.*
dotnet add package Polly --version 8.*
dotnet add package Polly.Extensions.Http --version 3.*
```

#### QuickReserve.API
```bash
cd src/QuickReserve.API
dotnet add package Swashbuckle.AspNetCore --version 7.*
dotnet add package Serilog.AspNetCore --version 9.*
dotnet add package Serilog.Sinks.Console --version 6.*
dotnet add package Serilog.Sinks.File --version 6.*
dotnet add package Serilog.Sinks.Elasticsearch --version 10.*
dotnet add package Serilog.Enrichers.Environment --version 3.*
dotnet add package Serilog.Enrichers.Process --version 3.*
dotnet add package Serilog.Enrichers.Thread --version 4.*
dotnet add package Serilog.Enrichers.CorrelationId --version 3.*
dotnet add package AspNetCore.HealthChecks.Redis --version 8.*
dotnet add package AspNetCore.HealthChecks.Uris --version 8.*
```

#### QuickReserve.Tests
```bash
cd tests/QuickReserve.Tests
dotnet add package Moq --version 4.*
dotnet add package FluentAssertions --version 7.*
dotnet add package coverlet.collector --version 6.*
dotnet add package Microsoft.AspNetCore.Mvc.Testing --version 10.*
dotnet add package Microsoft.EntityFrameworkCore.InMemory --version 10.*
```

### 1.9 Crear docker-compose.yml (Infraestructura de Calidad)

> **IMPORTANTE:** Configuramos Docker ANTES de escribir codigo para tener SonarQube disponible desde el inicio.

```yaml
# docker-compose.yml
version: '3.8'

services:
  # Redis para caching
  redis:
    image: redis:7-alpine
    container_name: quickreserve-redis
    ports:
      - "6379:6379"
    volumes:
      - redis-data:/data
    networks:
      - quickreserve-network
    healthcheck:
      test: ["CMD", "redis-cli", "ping"]
      interval: 10s
      timeout: 5s
      retries: 5

  # Elasticsearch para logs
  elasticsearch:
    image: docker.elastic.co/elasticsearch/elasticsearch:8.12.0
    container_name: quickreserve-elasticsearch
    environment:
      - discovery.type=single-node
      - xpack.security.enabled=false
      - "ES_JAVA_OPTS=-Xms512m -Xmx512m"
    ports:
      - "9200:9200"
    volumes:
      - elasticsearch-data:/usr/share/elasticsearch/data
    networks:
      - quickreserve-network
    healthcheck:
      test: ["CMD-SHELL", "curl -f http://localhost:9200/_cluster/health || exit 1"]
      interval: 30s
      timeout: 10s
      retries: 5

  # Kibana para visualizacion de logs
  kibana:
    image: docker.elastic.co/kibana/kibana:8.12.0
    container_name: quickreserve-kibana
    environment:
      - ELASTICSEARCH_HOSTS=http://elasticsearch:9200
    ports:
      - "5601:5601"
    depends_on:
      elasticsearch:
        condition: service_healthy
    networks:
      - quickreserve-network

  # PostgreSQL para SonarQube
  sonar-db:
    image: postgres:16-alpine
    container_name: quickreserve-sonar-db
    environment:
      - POSTGRES_USER=sonar
      - POSTGRES_PASSWORD=sonar
      - POSTGRES_DB=sonar
    volumes:
      - sonar-db-data:/var/lib/postgresql/data
    networks:
      - quickreserve-network
    healthcheck:
      test: ["CMD-SHELL", "pg_isready -U sonar"]
      interval: 10s
      timeout: 5s
      retries: 5

  # SonarQube para analisis de calidad
  sonarqube:
    image: sonarqube:community
    container_name: quickreserve-sonarqube
    environment:
      - SONAR_JDBC_URL=jdbc:postgresql://sonar-db:5432/sonar
      - SONAR_JDBC_USERNAME=sonar
      - SONAR_JDBC_PASSWORD=sonar
    ports:
      - "9000:9000"
    depends_on:
      sonar-db:
        condition: service_healthy
    volumes:
      - sonarqube-data:/opt/sonarqube/data
      - sonarqube-extensions:/opt/sonarqube/extensions
      - sonarqube-logs:/opt/sonarqube/logs
    networks:
      - quickreserve-network

networks:
  quickreserve-network:
    driver: bridge

volumes:
  redis-data:
  elasticsearch-data:
  sonarqube-data:
  sonarqube-extensions:
  sonarqube-logs:
  sonar-db-data:
```

### 1.10 Crear sonar-project.properties

```properties
# sonar-project.properties
sonar.projectKey=QuickReserve
sonar.projectName=QuickReserve Backend
sonar.projectVersion=1.0

# Paths
sonar.sources=src
sonar.tests=tests
sonar.exclusions=**/bin/**,**/obj/**,**/Migrations/**

# C# specific
sonar.cs.opencover.reportsPaths=**/coverage.opencover.xml
sonar.cs.vstest.reportsPaths=**/*.trx

# Encoding
sonar.sourceEncoding=UTF-8
```

### 1.11 Levantar infraestructura y verificar

```bash
# Levantar servicios de infraestructura
docker-compose up -d redis elasticsearch kibana sonar-db sonarqube

# Esperar a que levanten (puede tomar 1-2 minutos)
docker-compose logs -f sonarqube

# Verificar servicios
# Redis: redis-cli ping -> PONG
# Elasticsearch: curl http://localhost:9200 -> JSON con info del cluster
# Kibana: http://localhost:5601 -> UI
# SonarQube: http://localhost:9000 -> UI (admin/admin)
```

### 1.12 Instalar dotnet-sonarscanner

```bash
dotnet tool install --global dotnet-sonarscanner
```

### 1.13 Ejecutar primer analisis SonarQube (baseline)

```bash
# Iniciar analisis
dotnet sonarscanner begin /k:"QuickReserve" /d:sonar.host.url="http://localhost:9000" /d:sonar.token="TU_TOKEN_AQUI"

# Build
dotnet build

# Finalizar analisis
dotnet sonarscanner end /d:sonar.token="TU_TOKEN_AQUI"
```

> **Nota:** Genera un token en SonarQube: My Account -> Security -> Generate Token

### 1.14 Crear README.md basico

```markdown
# QuickReserve Backend

API para reserva de turnos en talleres - Challenge Tecnico Tecnom

## Requisitos

- .NET 10 SDK
- Docker y Docker Compose

## Quick Start

```bash
# Levantar infraestructura
docker-compose up -d

# Ejecutar API
dotnet run --project src/QuickReserve.API

# Ejecutar tests
dotnet test

# Analisis SonarQube
dotnet sonarscanner begin /k:"QuickReserve" /d:sonar.host.url="http://localhost:9000"
dotnet build
dotnet test --collect:"XPlat Code Coverage"
dotnet sonarscanner end
```

## URLs

| Servicio | URL |
|----------|-----|
| API | http://localhost:5000 |
| Swagger | http://localhost:5000/swagger |
| Health | http://localhost:5000/health |
| Kibana | http://localhost:5601 |
| SonarQube | http://localhost:9000 |

## Arquitectura

Clean Architecture + DDD

```
src/
├── QuickReserve.Domain        # Entidades, Value Objects, Interfaces
├── QuickReserve.Application   # DTOs, Validators, Services
├── QuickReserve.Infrastructure # EF Core, HttpClient, Cache
└── QuickReserve.API           # Controllers, Middleware
```
```

### 1.15 Inicializar Git con Conventional Commits

```bash
git init
git add .
git commit -m "chore: initial project setup with Clean Architecture and quality infrastructure

- Add solution with 5 projects (Domain, Application, Infrastructure, API, Tests)
- Configure Directory.Build.props with StyleCop and analyzers
- Add .editorconfig, .gitignore, .gitattributes
- Add docker-compose with Redis, ELK, SonarQube
- Configure sonar-project.properties
- Add README.md"
```

---

## Fase 2 - Capa Domain (DDD) + Tests

> **Objetivo:** Implementar el corazon del dominio CON tests unitarios.
> Al finalizar: Entidades, Value Objects, Excepciones, Interfaces + Tests

### 2.1 Estructura de carpetas

```
src/QuickReserve.Domain/
├── Entities/
│   ├── Appointment.cs          # Aggregate Root
│   ├── Contact.cs
│   └── Vehicle.cs
├── ValueObjects/
│   ├── Email.cs
│   ├── Phone.cs
│   ├── LicensePlate.cs
│   └── ServiceType.cs
├── Exceptions/
│   ├── DomainException.cs
│   ├── InvalidEmailException.cs
│   ├── InvalidPhoneException.cs
│   ├── InvalidLicensePlateException.cs
│   └── InvalidWorkshopException.cs
├── Interfaces/
│   ├── IAppointmentRepository.cs
│   └── IWorkshopService.cs
└── Services/
    └── AppointmentDomainService.cs
```

### 2.2 Value Objects

#### Email.cs
```csharp
namespace QuickReserve.Domain.ValueObjects;

using System.Text.RegularExpressions;
using QuickReserve.Domain.Exceptions;

public sealed partial class Email : IEquatable<Email>
{
    private static readonly Regex EmailRegex = MyEmailRegex();

    public string Value { get; }

    private Email(string value)
    {
        Value = value;
    }

    public static Email Create(string email)
    {
        if (string.IsNullOrWhiteSpace(email))
        {
            throw new InvalidEmailException("El email no puede estar vacio.");
        }

        var trimmedEmail = email.Trim().ToLowerInvariant();

        if (!EmailRegex.IsMatch(trimmedEmail))
        {
            throw new InvalidEmailException($"El formato del email '{email}' no es valido.");
        }

        return new Email(trimmedEmail);
    }

    public bool Equals(Email? other)
    {
        return other is not null && Value == other.Value;
    }

    public override bool Equals(object? obj)
    {
        return obj is Email other && Equals(other);
    }

    public override int GetHashCode()
    {
        return Value.GetHashCode();
    }

    public override string ToString()
    {
        return Value;
    }

    public static implicit operator string(Email email)
    {
        return email.Value;
    }

    [GeneratedRegex(@"^[^@\s]+@[^@\s]+\.[^@\s]+$", RegexOptions.Compiled | RegexOptions.IgnoreCase)]
    private static partial Regex MyEmailRegex();
}
```

#### Phone.cs
```csharp
namespace QuickReserve.Domain.ValueObjects;

using System.Text.RegularExpressions;
using QuickReserve.Domain.Exceptions;

public sealed partial class Phone : IEquatable<Phone>
{
    private static readonly Regex PhoneRegex = MyPhoneRegex();

    public string Value { get; }

    private Phone(string value)
    {
        Value = value;
    }

    public static Phone Create(string phone)
    {
        if (string.IsNullOrWhiteSpace(phone))
        {
            throw new InvalidPhoneException("El telefono no puede estar vacio.");
        }

        var cleanedPhone = CleanPhoneNumber(phone);

        if (!PhoneRegex.IsMatch(cleanedPhone))
        {
            throw new InvalidPhoneException($"El formato del telefono '{phone}' no es valido.");
        }

        return new Phone(cleanedPhone);
    }

    private static string CleanPhoneNumber(string phone)
    {
        return Regex.Replace(phone, @"[\s\-\(\)]", string.Empty);
    }

    public bool Equals(Phone? other)
    {
        return other is not null && Value == other.Value;
    }

    public override bool Equals(object? obj)
    {
        return obj is Phone other && Equals(other);
    }

    public override int GetHashCode()
    {
        return Value.GetHashCode();
    }

    public override string ToString()
    {
        return Value;
    }

    public static implicit operator string(Phone phone)
    {
        return phone.Value;
    }

    [GeneratedRegex(@"^\+?[1-9]\d{6,14}$", RegexOptions.Compiled)]
    private static partial Regex MyPhoneRegex();
}
```

#### LicensePlate.cs
```csharp
namespace QuickReserve.Domain.ValueObjects;

using System.Text.RegularExpressions;
using QuickReserve.Domain.Exceptions;

public sealed partial class LicensePlate : IEquatable<LicensePlate>
{
    // Formato Argentina: ABC123 (viejo) o AB123CD (nuevo)
    private static readonly Regex LicensePlateRegex = MyLicensePlateRegex();

    public string Value { get; }

    private LicensePlate(string value)
    {
        Value = value;
    }

    public static LicensePlate? Create(string? licensePlate)
    {
        if (string.IsNullOrWhiteSpace(licensePlate))
        {
            return null;
        }

        var upperPlate = licensePlate.Trim().ToUpperInvariant();

        if (!LicensePlateRegex.IsMatch(upperPlate))
        {
            throw new InvalidLicensePlateException($"El formato de la patente '{licensePlate}' no es valido. Use formato ABC123 o AB123CD.");
        }

        return new LicensePlate(upperPlate);
    }

    public bool Equals(LicensePlate? other)
    {
        return other is not null && Value == other.Value;
    }

    public override bool Equals(object? obj)
    {
        return obj is LicensePlate other && Equals(other);
    }

    public override int GetHashCode()
    {
        return Value.GetHashCode();
    }

    public override string ToString()
    {
        return Value;
    }

    public static implicit operator string?(LicensePlate? plate)
    {
        return plate?.Value;
    }

    [GeneratedRegex(@"^([A-Z]{3}\d{3}|[A-Z]{2}\d{3}[A-Z]{2})$", RegexOptions.Compiled)]
    private static partial Regex MyLicensePlateRegex();
}
```

#### ServiceType.cs
```csharp
namespace QuickReserve.Domain.ValueObjects;

using QuickReserve.Domain.Exceptions;

public sealed class ServiceType : IEquatable<ServiceType>
{
    private static readonly HashSet<string> ValidServiceTypes = new(StringComparer.OrdinalIgnoreCase)
    {
        "Mantenimiento",
        "Reparacion",
        "Revision",
        "Diagnostico",
        "Service",
        "Otro"
    };

    public string Value { get; }

    private ServiceType(string value)
    {
        Value = value;
    }

    public static ServiceType Create(string serviceType)
    {
        if (string.IsNullOrWhiteSpace(serviceType))
        {
            throw new DomainException("El tipo de servicio no puede estar vacio.");
        }

        var trimmedType = serviceType.Trim();

        // Permitimos cualquier tipo de servicio, pero normalizamos los conocidos
        var normalizedType = ValidServiceTypes
            .FirstOrDefault(v => v.Equals(trimmedType, StringComparison.OrdinalIgnoreCase))
            ?? trimmedType;

        return new ServiceType(normalizedType);
    }

    public bool Equals(ServiceType? other)
    {
        return other is not null && Value.Equals(other.Value, StringComparison.OrdinalIgnoreCase);
    }

    public override bool Equals(object? obj)
    {
        return obj is ServiceType other && Equals(other);
    }

    public override int GetHashCode()
    {
        return Value.ToUpperInvariant().GetHashCode();
    }

    public override string ToString()
    {
        return Value;
    }

    public static implicit operator string(ServiceType serviceType)
    {
        return serviceType.Value;
    }
}
```

### 2.3 Domain Exceptions

#### DomainException.cs
```csharp
namespace QuickReserve.Domain.Exceptions;

public class DomainException : Exception
{
    public DomainException(string message)
        : base(message)
    {
    }

    public DomainException(string message, Exception innerException)
        : base(message, innerException)
    {
    }
}
```

#### InvalidEmailException.cs
```csharp
namespace QuickReserve.Domain.Exceptions;

public sealed class InvalidEmailException : DomainException
{
    public InvalidEmailException(string message)
        : base(message)
    {
    }
}
```

#### InvalidPhoneException.cs
```csharp
namespace QuickReserve.Domain.Exceptions;

public sealed class InvalidPhoneException : DomainException
{
    public InvalidPhoneException(string message)
        : base(message)
    {
    }
}
```

#### InvalidLicensePlateException.cs
```csharp
namespace QuickReserve.Domain.Exceptions;

public sealed class InvalidLicensePlateException : DomainException
{
    public InvalidLicensePlateException(string message)
        : base(message)
    {
    }
}
```

#### InvalidWorkshopException.cs
```csharp
namespace QuickReserve.Domain.Exceptions;

public sealed class InvalidWorkshopException : DomainException
{
    public int PlaceId { get; }

    public InvalidWorkshopException(int placeId)
        : base($"El taller con ID {placeId} no existe o no esta activo.")
    {
        PlaceId = placeId;
    }
}
```

### 2.4 Entities

#### Contact.cs
```csharp
namespace QuickReserve.Domain.Entities;

using QuickReserve.Domain.ValueObjects;

public sealed class Contact
{
    public string Name { get; private set; }
    public Email Email { get; private set; }
    public Phone Phone { get; private set; }

    private Contact()
    {
        // EF Core
        Name = string.Empty;
        Email = null!;
        Phone = null!;
    }

    private Contact(string name, Email email, Phone phone)
    {
        Name = name;
        Email = email;
        Phone = phone;
    }

    public static Contact Create(string name, string email, string phone)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            throw new Exceptions.DomainException("El nombre del contacto no puede estar vacio.");
        }

        return new Contact(
            name.Trim(),
            Email.Create(email),
            Phone.Create(phone));
    }
}
```

#### Vehicle.cs
```csharp
namespace QuickReserve.Domain.Entities;

using QuickReserve.Domain.ValueObjects;

public sealed class Vehicle
{
    public string? Make { get; private set; }
    public string? Model { get; private set; }
    public int? Year { get; private set; }
    public LicensePlate? LicensePlate { get; private set; }

    private Vehicle()
    {
        // EF Core
    }

    private Vehicle(string? make, string? model, int? year, LicensePlate? licensePlate)
    {
        Make = make;
        Model = model;
        Year = year;
        LicensePlate = licensePlate;
    }

    public static Vehicle? Create(string? make, string? model, int? year, string? licensePlate)
    {
        // Si todos los campos son null/empty, no crear vehiculo
        if (string.IsNullOrWhiteSpace(make) &&
            string.IsNullOrWhiteSpace(model) &&
            !year.HasValue &&
            string.IsNullOrWhiteSpace(licensePlate))
        {
            return null;
        }

        return new Vehicle(
            make?.Trim(),
            model?.Trim(),
            year,
            LicensePlate.Create(licensePlate));
    }
}
```

#### Appointment.cs (Aggregate Root)
```csharp
namespace QuickReserve.Domain.Entities;

using QuickReserve.Domain.Exceptions;
using QuickReserve.Domain.ValueObjects;

public sealed class Appointment
{
    public Guid Id { get; private set; }
    public int PlaceId { get; private set; }
    public DateTime AppointmentAt { get; private set; }
    public ServiceType ServiceType { get; private set; }
    public Contact Contact { get; private set; }
    public Vehicle? Vehicle { get; private set; }
    public DateTime CreatedAt { get; private set; }

    private Appointment()
    {
        // EF Core
        ServiceType = null!;
        Contact = null!;
    }

    private Appointment(
        Guid id,
        int placeId,
        DateTime appointmentAt,
        ServiceType serviceType,
        Contact contact,
        Vehicle? vehicle,
        DateTime createdAt)
    {
        Id = id;
        PlaceId = placeId;
        AppointmentAt = appointmentAt;
        ServiceType = serviceType;
        Contact = contact;
        Vehicle = vehicle;
        CreatedAt = createdAt;
    }

    public static Appointment Create(
        int placeId,
        DateTime appointmentAt,
        string serviceType,
        string contactName,
        string contactEmail,
        string contactPhone,
        string? vehicleMake = null,
        string? vehicleModel = null,
        int? vehicleYear = null,
        string? vehicleLicensePlate = null)
    {
        ValidateAppointmentDate(appointmentAt);

        return new Appointment(
            Guid.NewGuid(),
            placeId,
            appointmentAt,
            ServiceType.Create(serviceType),
            Contact.Create(contactName, contactEmail, contactPhone),
            Vehicle.Create(vehicleMake, vehicleModel, vehicleYear, vehicleLicensePlate),
            DateTime.UtcNow);
    }

    private static void ValidateAppointmentDate(DateTime appointmentAt)
    {
        if (appointmentAt <= DateTime.UtcNow)
        {
            throw new DomainException("La fecha del turno debe ser futura.");
        }
    }
}
```

### 2.5 Interfaces (Puertos)

#### IAppointmentRepository.cs
```csharp
namespace QuickReserve.Domain.Interfaces;

using QuickReserve.Domain.Entities;

public interface IAppointmentRepository
{
    Task<Appointment?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<Appointment>> GetAllAsync(CancellationToken cancellationToken = default);
    Task<Appointment> AddAsync(Appointment appointment, CancellationToken cancellationToken = default);
}
```

#### IWorkshopService.cs
```csharp
namespace QuickReserve.Domain.Interfaces;

public interface IWorkshopService
{
    Task<bool> IsActiveWorkshopAsync(int placeId, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<WorkshopInfo>> GetActiveWorkshopsAsync(CancellationToken cancellationToken = default);
}

public sealed record WorkshopInfo(
    int Id,
    string Name,
    string? Address,
    string? Email,
    string? Whatsapp);
```

### 2.6 Domain Service

#### AppointmentDomainService.cs
```csharp
namespace QuickReserve.Domain.Services;

using QuickReserve.Domain.Entities;
using QuickReserve.Domain.Exceptions;
using QuickReserve.Domain.Interfaces;

public sealed class AppointmentDomainService
{
    private readonly IWorkshopService _workshopService;

    public AppointmentDomainService(IWorkshopService workshopService)
    {
        _workshopService = workshopService;
    }

    public async Task<Appointment> CreateAppointmentAsync(
        int placeId,
        DateTime appointmentAt,
        string serviceType,
        string contactName,
        string contactEmail,
        string contactPhone,
        string? vehicleMake = null,
        string? vehicleModel = null,
        int? vehicleYear = null,
        string? vehicleLicensePlate = null,
        CancellationToken cancellationToken = default)
    {
        // Validar que el taller existe y esta activo
        var isActiveWorkshop = await _workshopService.IsActiveWorkshopAsync(placeId, cancellationToken);

        if (!isActiveWorkshop)
        {
            throw new InvalidWorkshopException(placeId);
        }

        // Crear el appointment (las validaciones de dominio estan en la entidad)
        return Appointment.Create(
            placeId,
            appointmentAt,
            serviceType,
            contactName,
            contactEmail,
            contactPhone,
            vehicleMake,
            vehicleModel,
            vehicleYear,
            vehicleLicensePlate);
    }
}
```

### 2.7 Tests Unitarios de Domain

#### tests/QuickReserve.Tests/Domain/ValueObjects/EmailTests.cs
```csharp
namespace QuickReserve.Tests.Domain.ValueObjects;

using FluentAssertions;
using QuickReserve.Domain.Exceptions;
using QuickReserve.Domain.ValueObjects;

public class EmailTests
{
    [Theory]
    [InlineData("test@example.com")]
    [InlineData("user.name@domain.org")]
    [InlineData("user+tag@example.co.uk")]
    public void Create_WithValidEmail_ShouldSucceed(string validEmail)
    {
        // Act
        var email = Email.Create(validEmail);

        // Assert
        email.Value.Should().Be(validEmail.ToLowerInvariant());
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData(null)]
    public void Create_WithEmptyEmail_ShouldThrowException(string? invalidEmail)
    {
        // Act
        var act = () => Email.Create(invalidEmail!);

        // Assert
        act.Should().Throw<InvalidEmailException>()
            .WithMessage("*vacio*");
    }

    [Theory]
    [InlineData("invalid")]
    [InlineData("@domain.com")]
    [InlineData("user@")]
    [InlineData("user@.com")]
    public void Create_WithInvalidFormat_ShouldThrowException(string invalidEmail)
    {
        // Act
        var act = () => Email.Create(invalidEmail);

        // Assert
        act.Should().Throw<InvalidEmailException>()
            .WithMessage("*formato*");
    }

    [Fact]
    public void Equals_WithSameValue_ShouldBeTrue()
    {
        // Arrange
        var email1 = Email.Create("test@example.com");
        var email2 = Email.Create("TEST@EXAMPLE.COM");

        // Assert
        email1.Should().Be(email2);
    }
}
```

#### tests/QuickReserve.Tests/Domain/Entities/AppointmentTests.cs
```csharp
namespace QuickReserve.Tests.Domain.Entities;

using FluentAssertions;
using QuickReserve.Domain.Entities;
using QuickReserve.Domain.Exceptions;

public class AppointmentTests
{
    [Fact]
    public void Create_WithValidData_ShouldSucceed()
    {
        // Arrange
        var futureDate = DateTime.UtcNow.AddDays(7);

        // Act
        var appointment = Appointment.Create(
            placeId: 123,
            appointmentAt: futureDate,
            serviceType: "Mantenimiento",
            contactName: "Juan Perez",
            contactEmail: "juan@email.com",
            contactPhone: "+5491155551234");

        // Assert
        appointment.Id.Should().NotBeEmpty();
        appointment.PlaceId.Should().Be(123);
        appointment.AppointmentAt.Should().Be(futureDate);
        appointment.ServiceType.Value.Should().Be("Mantenimiento");
        appointment.Contact.Name.Should().Be("Juan Perez");
        appointment.Contact.Email.Value.Should().Be("juan@email.com");
        appointment.Vehicle.Should().BeNull();
        appointment.CreatedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(5));
    }

    [Fact]
    public void Create_WithPastDate_ShouldThrowException()
    {
        // Arrange
        var pastDate = DateTime.UtcNow.AddDays(-1);

        // Act
        var act = () => Appointment.Create(
            placeId: 123,
            appointmentAt: pastDate,
            serviceType: "Mantenimiento",
            contactName: "Juan Perez",
            contactEmail: "juan@email.com",
            contactPhone: "+5491155551234");

        // Assert
        act.Should().Throw<DomainException>()
            .WithMessage("*fecha*futura*");
    }

    [Fact]
    public void Create_WithVehicle_ShouldIncludeVehicle()
    {
        // Arrange
        var futureDate = DateTime.UtcNow.AddDays(7);

        // Act
        var appointment = Appointment.Create(
            placeId: 123,
            appointmentAt: futureDate,
            serviceType: "Mantenimiento",
            contactName: "Juan Perez",
            contactEmail: "juan@email.com",
            contactPhone: "+5491155551234",
            vehicleMake: "Toyota",
            vehicleModel: "Corolla",
            vehicleYear: 2022,
            vehicleLicensePlate: "AB123CD");

        // Assert
        appointment.Vehicle.Should().NotBeNull();
        appointment.Vehicle!.Make.Should().Be("Toyota");
        appointment.Vehicle.LicensePlate!.Value.Should().Be("AB123CD");
    }
}
```

### 2.8 Ejecutar tests y analisis SonarQube

```bash
# Ejecutar tests con cobertura
dotnet test --collect:"XPlat Code Coverage" --results-directory ./coverage

# Analisis SonarQube
dotnet sonarscanner begin /k:"QuickReserve" /d:sonar.host.url="http://localhost:9000" /d:sonar.cs.opencover.reportsPaths="**/coverage.opencover.xml"
dotnet build
dotnet test --collect:"XPlat Code Coverage"
dotnet sonarscanner end
```

### 2.9 Commit Fase 2

```bash
git add .
git commit -m "feat(domain): add entities, value objects, exceptions and tests

- Add Appointment aggregate root with Contact and Vehicle entities
- Add value objects: Email, Phone, LicensePlate, ServiceType
- Add domain exceptions for validation errors
- Add repository and service interfaces (ports)
- Add AppointmentDomainService for business rules
- Add unit tests for value objects and entities"
```

---

## Fase 3 - Capa Application + Tests

> **Objetivo:** Implementar la capa de aplicacion CON tests unitarios.

### 3.1 Estructura de carpetas

```
src/QuickReserve.Application/
├── DTOs/
│   ├── Requests/
│   │   ├── CreateAppointmentRequest.cs
│   │   ├── ContactRequest.cs
│   │   └── VehicleRequest.cs
│   └── Responses/
│       ├── ApiResponse.cs
│       ├── AppointmentResponse.cs
│       ├── ContactResponse.cs
│       ├── VehicleResponse.cs
│       └── WorkshopResponse.cs
├── Validators/
│   ├── CreateAppointmentValidator.cs
│   ├── ContactRequestValidator.cs
│   └── VehicleRequestValidator.cs
├── Mappings/
│   └── MappingConfig.cs
├── Interfaces/
│   ├── IAppointmentAppService.cs
│   └── IWorkshopAppService.cs
├── Services/
│   ├── AppointmentAppService.cs
│   └── WorkshopAppService.cs
└── DependencyInjection.cs
```

### 3.2 DTOs - Requests

#### CreateAppointmentRequest.cs
```csharp
namespace QuickReserve.Application.DTOs.Requests;

using System.Text.Json.Serialization;

public sealed record CreateAppointmentRequest
{
    [JsonPropertyName("place_id")]
    public int PlaceId { get; init; }

    [JsonPropertyName("appointment_at")]
    public DateTime AppointmentAt { get; init; }

    [JsonPropertyName("service_type")]
    public string ServiceType { get; init; } = string.Empty;

    [JsonPropertyName("contact")]
    public ContactRequest Contact { get; init; } = null!;

    [JsonPropertyName("vehicle")]
    public VehicleRequest? Vehicle { get; init; }
}
```

#### ContactRequest.cs
```csharp
namespace QuickReserve.Application.DTOs.Requests;

using System.Text.Json.Serialization;

public sealed record ContactRequest
{
    [JsonPropertyName("name")]
    public string Name { get; init; } = string.Empty;

    [JsonPropertyName("email")]
    public string Email { get; init; } = string.Empty;

    [JsonPropertyName("phone")]
    public string Phone { get; init; } = string.Empty;
}
```

#### VehicleRequest.cs
```csharp
namespace QuickReserve.Application.DTOs.Requests;

using System.Text.Json.Serialization;

public sealed record VehicleRequest
{
    [JsonPropertyName("make")]
    public string? Make { get; init; }

    [JsonPropertyName("model")]
    public string? Model { get; init; }

    [JsonPropertyName("year")]
    public int? Year { get; init; }

    [JsonPropertyName("license_plate")]
    public string? LicensePlate { get; init; }
}
```

### 3.3 DTOs - Responses

#### ApiResponse.cs
```csharp
namespace QuickReserve.Application.DTOs.Responses;

using System.Text.Json.Serialization;

public sealed record ApiResponse<T>
{
    [JsonPropertyName("success")]
    public bool Success { get; init; }

    [JsonPropertyName("data")]
    public T? Data { get; init; }

    [JsonPropertyName("errors")]
    public IReadOnlyList<string>? Errors { get; init; }

    public static ApiResponse<T> Ok(T data)
    {
        return new ApiResponse<T>
        {
            Success = true,
            Data = data,
            Errors = null
        };
    }

    public static ApiResponse<T> Fail(IEnumerable<string> errors)
    {
        return new ApiResponse<T>
        {
            Success = false,
            Data = default,
            Errors = errors.ToList()
        };
    }

    public static ApiResponse<T> Fail(string error)
    {
        return Fail(new[] { error });
    }
}
```

#### AppointmentResponse.cs
```csharp
namespace QuickReserve.Application.DTOs.Responses;

using System.Text.Json.Serialization;

public sealed record AppointmentResponse
{
    [JsonPropertyName("id")]
    public Guid Id { get; init; }

    [JsonPropertyName("place_id")]
    public int PlaceId { get; init; }

    [JsonPropertyName("appointment_at")]
    public DateTime AppointmentAt { get; init; }

    [JsonPropertyName("service_type")]
    public string ServiceType { get; init; } = string.Empty;

    [JsonPropertyName("contact")]
    public ContactResponse Contact { get; init; } = null!;

    [JsonPropertyName("vehicle")]
    public VehicleResponse? Vehicle { get; init; }

    [JsonPropertyName("created_at")]
    public DateTime CreatedAt { get; init; }
}
```

#### ContactResponse.cs
```csharp
namespace QuickReserve.Application.DTOs.Responses;

using System.Text.Json.Serialization;

public sealed record ContactResponse
{
    [JsonPropertyName("name")]
    public string Name { get; init; } = string.Empty;

    [JsonPropertyName("email")]
    public string Email { get; init; } = string.Empty;

    [JsonPropertyName("phone")]
    public string Phone { get; init; } = string.Empty;
}
```

#### VehicleResponse.cs
```csharp
namespace QuickReserve.Application.DTOs.Responses;

using System.Text.Json.Serialization;

public sealed record VehicleResponse
{
    [JsonPropertyName("make")]
    public string? Make { get; init; }

    [JsonPropertyName("model")]
    public string? Model { get; init; }

    [JsonPropertyName("year")]
    public int? Year { get; init; }

    [JsonPropertyName("license_plate")]
    public string? LicensePlate { get; init; }
}
```

#### WorkshopResponse.cs
```csharp
namespace QuickReserve.Application.DTOs.Responses;

using System.Text.Json.Serialization;

public sealed record WorkshopResponse
{
    [JsonPropertyName("id")]
    public int Id { get; init; }

    [JsonPropertyName("name")]
    public string Name { get; init; } = string.Empty;

    [JsonPropertyName("address")]
    public string? Address { get; init; }

    [JsonPropertyName("email")]
    public string? Email { get; init; }

    [JsonPropertyName("whatsapp")]
    public string? Whatsapp { get; init; }
}
```

### 3.4 Validators (FluentValidation)

#### CreateAppointmentValidator.cs
```csharp
namespace QuickReserve.Application.Validators;

using FluentValidation;
using QuickReserve.Application.DTOs.Requests;

public sealed class CreateAppointmentValidator : AbstractValidator<CreateAppointmentRequest>
{
    public CreateAppointmentValidator()
    {
        RuleFor(x => x.PlaceId)
            .GreaterThan(0)
            .WithMessage("El place_id debe ser mayor a 0.");

        RuleFor(x => x.AppointmentAt)
            .GreaterThan(DateTime.UtcNow)
            .WithMessage("La fecha del turno debe ser futura.");

        RuleFor(x => x.ServiceType)
            .NotEmpty()
            .WithMessage("El tipo de servicio es requerido.")
            .MaximumLength(100)
            .WithMessage("El tipo de servicio no puede exceder 100 caracteres.");

        RuleFor(x => x.Contact)
            .NotNull()
            .WithMessage("Los datos de contacto son requeridos.")
            .SetValidator(new ContactRequestValidator());

        When(x => x.Vehicle is not null, () =>
        {
            RuleFor(x => x.Vehicle!)
                .SetValidator(new VehicleRequestValidator());
        });
    }
}
```

#### ContactRequestValidator.cs
```csharp
namespace QuickReserve.Application.Validators;

using FluentValidation;
using QuickReserve.Application.DTOs.Requests;

public sealed class ContactRequestValidator : AbstractValidator<ContactRequest>
{
    public ContactRequestValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty()
            .WithMessage("El nombre es requerido.")
            .MaximumLength(200)
            .WithMessage("El nombre no puede exceder 200 caracteres.");

        RuleFor(x => x.Email)
            .NotEmpty()
            .WithMessage("El email es requerido.")
            .EmailAddress()
            .WithMessage("El formato del email no es valido.")
            .MaximumLength(254)
            .WithMessage("El email no puede exceder 254 caracteres.");

        RuleFor(x => x.Phone)
            .NotEmpty()
            .WithMessage("El telefono es requerido.")
            .Matches(@"^\+?[1-9][\d\s\-\(\)]{6,20}$")
            .WithMessage("El formato del telefono no es valido.");
    }
}
```

#### VehicleRequestValidator.cs
```csharp
namespace QuickReserve.Application.Validators;

using FluentValidation;
using QuickReserve.Application.DTOs.Requests;

public sealed class VehicleRequestValidator : AbstractValidator<VehicleRequest>
{
    public VehicleRequestValidator()
    {
        RuleFor(x => x.Make)
            .MaximumLength(100)
            .WithMessage("La marca no puede exceder 100 caracteres.");

        RuleFor(x => x.Model)
            .MaximumLength(100)
            .WithMessage("El modelo no puede exceder 100 caracteres.");

        RuleFor(x => x.Year)
            .InclusiveBetween(1900, DateTime.UtcNow.Year + 1)
            .When(x => x.Year.HasValue)
            .WithMessage($"El ano debe estar entre 1900 y {DateTime.UtcNow.Year + 1}.");

        RuleFor(x => x.LicensePlate)
            .Matches(@"^([A-Za-z]{3}\d{3}|[A-Za-z]{2}\d{3}[A-Za-z]{2})$")
            .When(x => !string.IsNullOrWhiteSpace(x.LicensePlate))
            .WithMessage("El formato de la patente no es valido. Use ABC123 o AB123CD.");
    }
}
```

### 3.5 Mappings (Mapster)

#### MappingConfig.cs
```csharp
namespace QuickReserve.Application.Mappings;

using Mapster;
using QuickReserve.Application.DTOs.Responses;
using QuickReserve.Domain.Entities;
using QuickReserve.Domain.Interfaces;

public static class MappingConfig
{
    public static void Configure()
    {
        // Appointment -> AppointmentResponse
        TypeAdapterConfig<Appointment, AppointmentResponse>
            .NewConfig()
            .Map(dest => dest.ServiceType, src => src.ServiceType.Value)
            .Map(dest => dest.Contact, src => src.Contact)
            .Map(dest => dest.Vehicle, src => src.Vehicle);

        // Contact -> ContactResponse
        TypeAdapterConfig<Contact, ContactResponse>
            .NewConfig()
            .Map(dest => dest.Email, src => src.Email.Value)
            .Map(dest => dest.Phone, src => src.Phone.Value);

        // Vehicle -> VehicleResponse
        TypeAdapterConfig<Vehicle, VehicleResponse>
            .NewConfig()
            .Map(dest => dest.LicensePlate, src => src.LicensePlate != null ? src.LicensePlate.Value : null);

        // WorkshopInfo -> WorkshopResponse
        TypeAdapterConfig<WorkshopInfo, WorkshopResponse>
            .NewConfig();
    }
}
```

### 3.6 Application Services Interfaces

#### IAppointmentAppService.cs
```csharp
namespace QuickReserve.Application.Interfaces;

using QuickReserve.Application.DTOs.Requests;
using QuickReserve.Application.DTOs.Responses;

public interface IAppointmentAppService
{
    Task<ApiResponse<AppointmentResponse>> CreateAsync(CreateAppointmentRequest request, CancellationToken cancellationToken = default);
    Task<ApiResponse<IReadOnlyList<AppointmentResponse>>> GetAllAsync(CancellationToken cancellationToken = default);
}
```

#### IWorkshopAppService.cs
```csharp
namespace QuickReserve.Application.Interfaces;

using QuickReserve.Application.DTOs.Responses;

public interface IWorkshopAppService
{
    Task<ApiResponse<IReadOnlyList<WorkshopResponse>>> GetAllActiveAsync(CancellationToken cancellationToken = default);
}
```

### 3.7 Application Services Implementation

#### AppointmentAppService.cs
```csharp
namespace QuickReserve.Application.Services;

using FluentValidation;
using Mapster;
using Microsoft.Extensions.Logging;
using QuickReserve.Application.DTOs.Requests;
using QuickReserve.Application.DTOs.Responses;
using QuickReserve.Application.Interfaces;
using QuickReserve.Domain.Exceptions;
using QuickReserve.Domain.Interfaces;
using QuickReserve.Domain.Services;

public sealed class AppointmentAppService : IAppointmentAppService
{
    private readonly IAppointmentRepository _appointmentRepository;
    private readonly AppointmentDomainService _domainService;
    private readonly IValidator<CreateAppointmentRequest> _validator;
    private readonly ILogger<AppointmentAppService> _logger;

    public AppointmentAppService(
        IAppointmentRepository appointmentRepository,
        AppointmentDomainService domainService,
        IValidator<CreateAppointmentRequest> validator,
        ILogger<AppointmentAppService> logger)
    {
        _appointmentRepository = appointmentRepository;
        _domainService = domainService;
        _validator = validator;
        _logger = logger;
    }

    public async Task<ApiResponse<AppointmentResponse>> CreateAsync(
        CreateAppointmentRequest request,
        CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Creating appointment for place {PlaceId}", request.PlaceId);

        // Validar request
        var validationResult = await _validator.ValidateAsync(request, cancellationToken);

        if (!validationResult.IsValid)
        {
            var errors = validationResult.Errors.Select(e => e.ErrorMessage).ToList();
            _logger.LogWarning("Validation failed for appointment: {Errors}", string.Join(", ", errors));
            return ApiResponse<AppointmentResponse>.Fail(errors);
        }

        try
        {
            // Crear appointment via domain service
            var appointment = await _domainService.CreateAppointmentAsync(
                request.PlaceId,
                request.AppointmentAt,
                request.ServiceType,
                request.Contact.Name,
                request.Contact.Email,
                request.Contact.Phone,
                request.Vehicle?.Make,
                request.Vehicle?.Model,
                request.Vehicle?.Year,
                request.Vehicle?.LicensePlate,
                cancellationToken);

            // Persistir
            await _appointmentRepository.AddAsync(appointment, cancellationToken);

            _logger.LogInformation("Appointment {AppointmentId} created successfully", appointment.Id);

            // Mapear a response
            var response = appointment.Adapt<AppointmentResponse>();
            return ApiResponse<AppointmentResponse>.Ok(response);
        }
        catch (DomainException ex)
        {
            _logger.LogWarning(ex, "Domain validation failed: {Message}", ex.Message);
            return ApiResponse<AppointmentResponse>.Fail(ex.Message);
        }
    }

    public async Task<ApiResponse<IReadOnlyList<AppointmentResponse>>> GetAllAsync(
        CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Getting all appointments");

        var appointments = await _appointmentRepository.GetAllAsync(cancellationToken);
        var response = appointments.Adapt<IReadOnlyList<AppointmentResponse>>();

        _logger.LogInformation("Retrieved {Count} appointments", appointments.Count);

        return ApiResponse<IReadOnlyList<AppointmentResponse>>.Ok(response);
    }
}
```

#### WorkshopAppService.cs
```csharp
namespace QuickReserve.Application.Services;

using Mapster;
using Microsoft.Extensions.Logging;
using QuickReserve.Application.DTOs.Responses;
using QuickReserve.Application.Interfaces;
using QuickReserve.Domain.Interfaces;

public sealed class WorkshopAppService : IWorkshopAppService
{
    private readonly IWorkshopService _workshopService;
    private readonly ILogger<WorkshopAppService> _logger;

    public WorkshopAppService(
        IWorkshopService workshopService,
        ILogger<WorkshopAppService> logger)
    {
        _workshopService = workshopService;
        _logger = logger;
    }

    public async Task<ApiResponse<IReadOnlyList<WorkshopResponse>>> GetAllActiveAsync(
        CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Getting all active workshops");

        try
        {
            var workshops = await _workshopService.GetActiveWorkshopsAsync(cancellationToken);
            var response = workshops.Adapt<IReadOnlyList<WorkshopResponse>>();

            _logger.LogInformation("Retrieved {Count} active workshops", workshops.Count);

            return ApiResponse<IReadOnlyList<WorkshopResponse>>.Ok(response);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving workshops from external API");
            return ApiResponse<IReadOnlyList<WorkshopResponse>>.Fail("Error al obtener los talleres. Intente nuevamente.");
        }
    }
}
```

### 3.8 Dependency Injection

#### DependencyInjection.cs
```csharp
namespace QuickReserve.Application;

using FluentValidation;
using Microsoft.Extensions.DependencyInjection;
using QuickReserve.Application.Interfaces;
using QuickReserve.Application.Mappings;
using QuickReserve.Application.Services;
using QuickReserve.Domain.Services;

public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        // Mapster configuration
        MappingConfig.Configure();

        // FluentValidation - registrar todos los validators del assembly
        services.AddValidatorsFromAssemblyContaining<IAppointmentAppService>();

        // Domain Services
        services.AddScoped<AppointmentDomainService>();

        // Application Services
        services.AddScoped<IAppointmentAppService, AppointmentAppService>();
        services.AddScoped<IWorkshopAppService, WorkshopAppService>();

        return services;
    }
}
```

### 3.9 Commit Fase 3

```bash
git add .
git commit -m "feat(application): add DTOs, validators, mappings and application services

- Add request/response DTOs with JSON serialization
- Add FluentValidation validators for input validation
- Add Mapster configuration for entity-to-DTO mapping
- Add AppointmentAppService and WorkshopAppService
- Add ApiResponse wrapper for consistent API responses
- Add DependencyInjection extension method"
```

---

## Fase 4 - Capa Infrastructure

### 4.1 Estructura de carpetas

```
src/QuickReserve.Infrastructure/
├── Persistence/
│   ├── AppDbContext.cs
│   ├── Configurations/
│   │   ├── AppointmentConfiguration.cs
│   │   ├── ContactConfiguration.cs
│   │   └── VehicleConfiguration.cs
│   └── Repositories/
│       └── AppointmentRepository.cs
├── ExternalServices/
│   ├── TecnomApiClient.cs
│   ├── CachedWorkshopService.cs
│   └── Models/
│       └── TecnomWorkshopDto.cs
├── Configuration/
│   └── TecnomApiSettings.cs
└── DependencyInjection.cs
```

### 4.2 EF Core DbContext

#### AppDbContext.cs
```csharp
namespace QuickReserve.Infrastructure.Persistence;

using Microsoft.EntityFrameworkCore;
using QuickReserve.Domain.Entities;

public sealed class AppDbContext : DbContext
{
    public DbSet<Appointment> Appointments => Set<Appointment>();

    public AppDbContext(DbContextOptions<AppDbContext> options)
        : base(options)
    {
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(AppDbContext).Assembly);
        base.OnModelCreating(modelBuilder);
    }
}
```

### 4.3 Entity Configurations

#### AppointmentConfiguration.cs
```csharp
namespace QuickReserve.Infrastructure.Persistence.Configurations;

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using QuickReserve.Domain.Entities;

public sealed class AppointmentConfiguration : IEntityTypeConfiguration<Appointment>
{
    public void Configure(EntityTypeBuilder<Appointment> builder)
    {
        builder.ToTable("Appointments");

        builder.HasKey(a => a.Id);

        builder.Property(a => a.Id)
            .ValueGeneratedNever();

        builder.Property(a => a.PlaceId)
            .IsRequired();

        builder.Property(a => a.AppointmentAt)
            .IsRequired();

        builder.Property(a => a.CreatedAt)
            .IsRequired();

        // ServiceType Value Object
        builder.Property(a => a.ServiceType)
            .HasConversion(
                v => v.Value,
                v => Domain.ValueObjects.ServiceType.Create(v))
            .HasMaxLength(100)
            .IsRequired();

        // Contact como Owned Entity
        builder.OwnsOne(a => a.Contact, contact =>
        {
            contact.Property(c => c.Name)
                .HasColumnName("ContactName")
                .HasMaxLength(200)
                .IsRequired();

            contact.Property(c => c.Email)
                .HasConversion(
                    v => v.Value,
                    v => Domain.ValueObjects.Email.Create(v))
                .HasColumnName("ContactEmail")
                .HasMaxLength(254)
                .IsRequired();

            contact.Property(c => c.Phone)
                .HasConversion(
                    v => v.Value,
                    v => Domain.ValueObjects.Phone.Create(v))
                .HasColumnName("ContactPhone")
                .HasMaxLength(20)
                .IsRequired();
        });

        // Vehicle como Owned Entity (opcional)
        builder.OwnsOne(a => a.Vehicle, vehicle =>
        {
            vehicle.Property(v => v.Make)
                .HasColumnName("VehicleMake")
                .HasMaxLength(100);

            vehicle.Property(v => v.Model)
                .HasColumnName("VehicleModel")
                .HasMaxLength(100);

            vehicle.Property(v => v.Year)
                .HasColumnName("VehicleYear");

            vehicle.Property(v => v.LicensePlate)
                .HasConversion(
                    v => v != null ? v.Value : null,
                    v => v != null ? Domain.ValueObjects.LicensePlate.Create(v) : null)
                .HasColumnName("VehicleLicensePlate")
                .HasMaxLength(10);
        });

        builder.Navigation(a => a.Contact).IsRequired();
        builder.Navigation(a => a.Vehicle);
    }
}
```

### 4.4 Repository

#### AppointmentRepository.cs
```csharp
namespace QuickReserve.Infrastructure.Persistence.Repositories;

using Microsoft.EntityFrameworkCore;
using QuickReserve.Domain.Entities;
using QuickReserve.Domain.Interfaces;

public sealed class AppointmentRepository : IAppointmentRepository
{
    private readonly AppDbContext _context;

    public AppointmentRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<Appointment?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _context.Appointments
            .FirstOrDefaultAsync(a => a.Id == id, cancellationToken);
    }

    public async Task<IReadOnlyList<Appointment>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        return await _context.Appointments
            .OrderByDescending(a => a.CreatedAt)
            .ToListAsync(cancellationToken);
    }

    public async Task<Appointment> AddAsync(Appointment appointment, CancellationToken cancellationToken = default)
    {
        await _context.Appointments.AddAsync(appointment, cancellationToken);
        await _context.SaveChangesAsync(cancellationToken);
        return appointment;
    }
}
```

### 4.5 Configuration (Options Pattern)

#### TecnomApiSettings.cs
```csharp
namespace QuickReserve.Infrastructure.Configuration;

public sealed class TecnomApiSettings
{
    public const string SectionName = "TecnomApi";

    public string BaseUrl { get; set; } = string.Empty;
    public string Username { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
    public int CacheExpirationMinutes { get; set; } = 5;
    public int TimeoutSeconds { get; set; } = 30;
    public int RetryCount { get; set; } = 3;
}
```

### 4.6 External Services

#### TecnomWorkshopDto.cs
```csharp
namespace QuickReserve.Infrastructure.ExternalServices.Models;

using System.Text.Json.Serialization;

public sealed record TecnomWorkshopDto
{
    [JsonPropertyName("id")]
    public int Id { get; init; }

    [JsonPropertyName("name")]
    public string Name { get; init; } = string.Empty;

    [JsonPropertyName("address")]
    public string? Address { get; init; }

    [JsonPropertyName("email")]
    public string? Email { get; init; }

    [JsonPropertyName("whatsapp")]
    public string? Whatsapp { get; init; }

    [JsonPropertyName("active")]
    public bool Active { get; init; }
}
```

#### TecnomApiClient.cs
```csharp
namespace QuickReserve.Infrastructure.ExternalServices;

using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using QuickReserve.Domain.Interfaces;
using QuickReserve.Infrastructure.Configuration;
using QuickReserve.Infrastructure.ExternalServices.Models;

public sealed class TecnomApiClient : IWorkshopService
{
    private readonly HttpClient _httpClient;
    private readonly ILogger<TecnomApiClient> _logger;

    public TecnomApiClient(
        HttpClient httpClient,
        IOptions<TecnomApiSettings> settings,
        ILogger<TecnomApiClient> logger)
    {
        _httpClient = httpClient;
        _logger = logger;

        // Configurar Basic Auth
        var credentials = Convert.ToBase64String(
            Encoding.ASCII.GetBytes($"{settings.Value.Username}:{settings.Value.Password}"));
        _httpClient.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue("Basic", credentials);
    }

    public async Task<bool> IsActiveWorkshopAsync(int placeId, CancellationToken cancellationToken = default)
    {
        _logger.LogDebug("Checking if workshop {PlaceId} is active", placeId);

        var workshops = await GetActiveWorkshopsAsync(cancellationToken);
        return workshops.Any(w => w.Id == placeId);
    }

    public async Task<IReadOnlyList<WorkshopInfo>> GetActiveWorkshopsAsync(CancellationToken cancellationToken = default)
    {
        _logger.LogDebug("Fetching workshops from Tecnom API");

        var response = await _httpClient.GetAsync("places/workshops", cancellationToken);
        response.EnsureSuccessStatusCode();

        var workshops = await response.Content.ReadFromJsonAsync<List<TecnomWorkshopDto>>(cancellationToken)
            ?? new List<TecnomWorkshopDto>();

        var activeWorkshops = workshops
            .Where(w => w.Active)
            .Select(w => new WorkshopInfo(w.Id, w.Name, w.Address, w.Email, w.Whatsapp))
            .ToList();

        _logger.LogDebug("Retrieved {Count} active workshops", activeWorkshops.Count);

        return activeWorkshops;
    }
}
```

#### CachedWorkshopService.cs (Decorator con Redis)
```csharp
namespace QuickReserve.Infrastructure.ExternalServices;

using System.Text.Json;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using QuickReserve.Domain.Interfaces;
using QuickReserve.Infrastructure.Configuration;

public sealed class CachedWorkshopService : IWorkshopService
{
    private readonly IWorkshopService _innerService;
    private readonly IDistributedCache _cache;
    private readonly ILogger<CachedWorkshopService> _logger;
    private readonly TecnomApiSettings _settings;

    private const string CacheKey = "workshops:active";

    public CachedWorkshopService(
        TecnomApiClient innerService,
        IDistributedCache cache,
        IOptions<TecnomApiSettings> settings,
        ILogger<CachedWorkshopService> logger)
    {
        _innerService = innerService;
        _cache = cache;
        _settings = settings.Value;
        _logger = logger;
    }

    public async Task<bool> IsActiveWorkshopAsync(int placeId, CancellationToken cancellationToken = default)
    {
        var workshops = await GetActiveWorkshopsAsync(cancellationToken);
        return workshops.Any(w => w.Id == placeId);
    }

    public async Task<IReadOnlyList<WorkshopInfo>> GetActiveWorkshopsAsync(CancellationToken cancellationToken = default)
    {
        // Intentar obtener del cache
        var cachedData = await _cache.GetStringAsync(CacheKey, cancellationToken);

        if (!string.IsNullOrEmpty(cachedData))
        {
            _logger.LogDebug("Workshops retrieved from cache");
            return JsonSerializer.Deserialize<List<WorkshopInfo>>(cachedData) ?? new List<WorkshopInfo>();
        }

        // Si no esta en cache, obtener de la API
        _logger.LogDebug("Cache miss, fetching workshops from API");
        var workshops = await _innerService.GetActiveWorkshopsAsync(cancellationToken);

        // Guardar en cache
        var cacheOptions = new DistributedCacheEntryOptions
        {
            AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(_settings.CacheExpirationMinutes)
        };

        await _cache.SetStringAsync(
            CacheKey,
            JsonSerializer.Serialize(workshops),
            cacheOptions,
            cancellationToken);

        _logger.LogDebug("Workshops cached for {Minutes} minutes", _settings.CacheExpirationMinutes);

        return workshops;
    }
}
```

### 4.7 Dependency Injection

#### DependencyInjection.cs
```csharp
namespace QuickReserve.Infrastructure;

using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Polly;
using Polly.Extensions.Http;
using QuickReserve.Domain.Interfaces;
using QuickReserve.Infrastructure.Configuration;
using QuickReserve.Infrastructure.ExternalServices;
using QuickReserve.Infrastructure.Persistence;
using QuickReserve.Infrastructure.Persistence.Repositories;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        // EF Core InMemory
        services.AddDbContext<AppDbContext>(options =>
            options.UseInMemoryDatabase("QuickReserveDb"));

        // Repository
        services.AddScoped<IAppointmentRepository, AppointmentRepository>();

        // Tecnom API Settings
        services.Configure<TecnomApiSettings>(
            configuration.GetSection(TecnomApiSettings.SectionName));

        var tecnomSettings = configuration
            .GetSection(TecnomApiSettings.SectionName)
            .Get<TecnomApiSettings>() ?? new TecnomApiSettings();

        // HttpClient con Polly
        services.AddHttpClient<TecnomApiClient>(client =>
        {
            client.BaseAddress = new Uri(tecnomSettings.BaseUrl);
            client.Timeout = TimeSpan.FromSeconds(tecnomSettings.TimeoutSeconds);
        })
        .AddPolicyHandler(GetRetryPolicy(tecnomSettings.RetryCount))
        .AddPolicyHandler(GetCircuitBreakerPolicy());

        // Workshop Service con Cache (Decorator)
        services.AddScoped<IWorkshopService, CachedWorkshopService>();

        // Redis Cache
        var redisConnection = configuration.GetConnectionString("Redis");
        if (!string.IsNullOrEmpty(redisConnection))
        {
            services.AddStackExchangeRedisCache(options =>
            {
                options.Configuration = redisConnection;
                options.InstanceName = "QuickReserve:";
            });
        }
        else
        {
            // Fallback a memoria si no hay Redis
            services.AddDistributedMemoryCache();
        }

        return services;
    }

    private static IAsyncPolicy<HttpResponseMessage> GetRetryPolicy(int retryCount)
    {
        return HttpPolicyExtensions
            .HandleTransientHttpError()
            .WaitAndRetryAsync(
                retryCount,
                retryAttempt => TimeSpan.FromSeconds(Math.Pow(2, retryAttempt)),
                onRetry: (outcome, timespan, retryAttempt, context) =>
                {
                    // Log retry attempts
                });
    }

    private static IAsyncPolicy<HttpResponseMessage> GetCircuitBreakerPolicy()
    {
        return HttpPolicyExtensions
            .HandleTransientHttpError()
            .CircuitBreakerAsync(
                handledEventsAllowedBeforeBreaking: 5,
                durationOfBreak: TimeSpan.FromSeconds(30));
    }
}
```

### 4.8 Commit Fase 4

```bash
git add .
git commit -m "feat(infrastructure): add EF Core, repository, external API client and caching

- Add AppDbContext with InMemory provider
- Add entity configurations with value object conversions
- Add AppointmentRepository implementation
- Add TecnomApiClient with Basic Auth and Polly policies
- Add CachedWorkshopService decorator with Redis/Memory cache
- Add TecnomApiSettings for configuration
- Add DependencyInjection with HttpClient factory and Polly"
```

---

## Fase 5 - Capa API

### 5.1 Estructura de carpetas

```
src/QuickReserve.API/
├── Controllers/
│   ├── AppointmentsController.cs
│   └── WorkshopsController.cs
├── Middleware/
│   ├── GlobalExceptionMiddleware.cs
│   └── CorrelationIdMiddleware.cs
├── Extensions/
│   ├── ServiceCollectionExtensions.cs
│   └── ApplicationBuilderExtensions.cs
├── Program.cs
├── appsettings.json
└── appsettings.Development.json
```

### 5.2 Controllers

#### AppointmentsController.cs
```csharp
namespace QuickReserve.API.Controllers;

using Microsoft.AspNetCore.Mvc;
using QuickReserve.Application.DTOs.Requests;
using QuickReserve.Application.DTOs.Responses;
using QuickReserve.Application.Interfaces;

/// <summary>
/// Controller para gestion de turnos/appointments.
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Produces("application/json")]
public sealed class AppointmentsController : ControllerBase
{
    private readonly IAppointmentAppService _appointmentService;
    private readonly ILogger<AppointmentsController> _logger;

    public AppointmentsController(
        IAppointmentAppService appointmentService,
        ILogger<AppointmentsController> logger)
    {
        _appointmentService = appointmentService;
        _logger = logger;
    }

    /// <summary>
    /// Obtiene todos los turnos creados.
    /// </summary>
    /// <param name="cancellationToken">Token de cancelacion.</param>
    /// <returns>Lista de turnos.</returns>
    /// <response code="200">Lista de turnos obtenida exitosamente.</response>
    [HttpGet]
    [ProducesResponseType(typeof(ApiResponse<IReadOnlyList<AppointmentResponse>>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAll(CancellationToken cancellationToken)
    {
        _logger.LogInformation("GET /api/appointments");

        var result = await _appointmentService.GetAllAsync(cancellationToken);
        return Ok(result);
    }

    /// <summary>
    /// Crea un nuevo turno.
    /// </summary>
    /// <param name="request">Datos del turno a crear.</param>
    /// <param name="cancellationToken">Token de cancelacion.</param>
    /// <returns>Turno creado.</returns>
    /// <response code="201">Turno creado exitosamente.</response>
    /// <response code="400">Datos invalidos o taller no activo.</response>
    [HttpPost]
    [ProducesResponseType(typeof(ApiResponse<AppointmentResponse>), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ApiResponse<AppointmentResponse>), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Create(
        [FromBody] CreateAppointmentRequest request,
        CancellationToken cancellationToken)
    {
        _logger.LogInformation("POST /api/appointments for place {PlaceId}", request.PlaceId);

        var result = await _appointmentService.CreateAsync(request, cancellationToken);

        if (!result.Success)
        {
            return BadRequest(result);
        }

        return CreatedAtAction(nameof(GetAll), result);
    }
}
```

#### WorkshopsController.cs
```csharp
namespace QuickReserve.API.Controllers;

using Microsoft.AspNetCore.Mvc;
using QuickReserve.Application.DTOs.Responses;
using QuickReserve.Application.Interfaces;

/// <summary>
/// Controller para consulta de talleres.
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Produces("application/json")]
public sealed class WorkshopsController : ControllerBase
{
    private readonly IWorkshopAppService _workshopService;
    private readonly ILogger<WorkshopsController> _logger;

    public WorkshopsController(
        IWorkshopAppService workshopService,
        ILogger<WorkshopsController> logger)
    {
        _workshopService = workshopService;
        _logger = logger;
    }

    /// <summary>
    /// Obtiene todos los talleres activos.
    /// </summary>
    /// <param name="cancellationToken">Token de cancelacion.</param>
    /// <returns>Lista de talleres activos.</returns>
    /// <response code="200">Lista de talleres obtenida exitosamente.</response>
    /// <response code="503">Error al conectar con el servicio externo.</response>
    [HttpGet]
    [ProducesResponseType(typeof(ApiResponse<IReadOnlyList<WorkshopResponse>>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<IReadOnlyList<WorkshopResponse>>), StatusCodes.Status503ServiceUnavailable)]
    public async Task<IActionResult> GetAll(CancellationToken cancellationToken)
    {
        _logger.LogInformation("GET /api/workshops");

        var result = await _workshopService.GetAllActiveAsync(cancellationToken);

        if (!result.Success)
        {
            return StatusCode(StatusCodes.Status503ServiceUnavailable, result);
        }

        return Ok(result);
    }
}
```

### 5.3 Middleware

#### GlobalExceptionMiddleware.cs
```csharp
namespace QuickReserve.API.Middleware;

using System.Net;
using System.Text.Json;
using QuickReserve.Application.DTOs.Responses;
using QuickReserve.Domain.Exceptions;

public sealed class GlobalExceptionMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<GlobalExceptionMiddleware> _logger;

    public GlobalExceptionMiddleware(RequestDelegate next, ILogger<GlobalExceptionMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (Exception ex)
        {
            await HandleExceptionAsync(context, ex);
        }
    }

    private async Task HandleExceptionAsync(HttpContext context, Exception exception)
    {
        var (statusCode, message) = exception switch
        {
            DomainException domainEx => (HttpStatusCode.BadRequest, domainEx.Message),
            InvalidWorkshopException workshopEx => (HttpStatusCode.BadRequest, workshopEx.Message),
            HttpRequestException => (HttpStatusCode.ServiceUnavailable, "Error al conectar con el servicio externo."),
            TaskCanceledException => (HttpStatusCode.RequestTimeout, "La solicitud ha expirado."),
            _ => (HttpStatusCode.InternalServerError, "Ha ocurrido un error interno.")
        };

        _logger.LogError(exception, "Exception caught: {Message}", exception.Message);

        context.Response.ContentType = "application/json";
        context.Response.StatusCode = (int)statusCode;

        var response = ApiResponse<object>.Fail(message);
        var json = JsonSerializer.Serialize(response, new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        });

        await context.Response.WriteAsync(json);
    }
}
```

#### CorrelationIdMiddleware.cs
```csharp
namespace QuickReserve.API.Middleware;

using Serilog.Context;

public sealed class CorrelationIdMiddleware
{
    private const string CorrelationIdHeader = "X-Correlation-ID";
    private readonly RequestDelegate _next;

    public CorrelationIdMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        var correlationId = context.Request.Headers[CorrelationIdHeader].FirstOrDefault()
            ?? Guid.NewGuid().ToString();

        context.Response.Headers[CorrelationIdHeader] = correlationId;

        using (LogContext.PushProperty("CorrelationId", correlationId))
        {
            await _next(context);
        }
    }
}
```

### 5.4 Extensions

#### ServiceCollectionExtensions.cs
```csharp
namespace QuickReserve.API.Extensions;

using Microsoft.OpenApi.Models;
using System.Reflection;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddSwaggerDocumentation(this IServiceCollection services)
    {
        services.AddSwaggerGen(options =>
        {
            options.SwaggerDoc("v1", new OpenApiInfo
            {
                Title = "QuickReserve API",
                Version = "v1",
                Description = "API para reserva de turnos en talleres - Challenge Tecnom",
                Contact = new OpenApiContact
                {
                    Name = "Milton",
                    Email = "milton@example.com"
                }
            });

            // Incluir comentarios XML
            var xmlFile = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
            var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
            if (File.Exists(xmlPath))
            {
                options.IncludeXmlComments(xmlPath);
            }
        });

        return services;
    }

    public static IServiceCollection AddCorsPolicy(this IServiceCollection services)
    {
        services.AddCors(options =>
        {
            options.AddPolicy("AllowAngular", policy =>
            {
                policy
                    .WithOrigins("http://localhost:4200", "https://localhost:4200")
                    .AllowAnyMethod()
                    .AllowAnyHeader()
                    .AllowCredentials();
            });
        });

        return services;
    }

    public static IServiceCollection AddHealthChecksConfiguration(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        var healthChecks = services.AddHealthChecks();

        // Redis health check
        var redisConnection = configuration.GetConnectionString("Redis");
        if (!string.IsNullOrEmpty(redisConnection))
        {
            healthChecks.AddRedis(redisConnection, name: "redis");
        }

        // Tecnom API health check
        var tecnomBaseUrl = configuration["TecnomApi:BaseUrl"];
        if (!string.IsNullOrEmpty(tecnomBaseUrl))
        {
            healthChecks.AddUrlGroup(
                new Uri($"{tecnomBaseUrl}/places/workshops"),
                name: "tecnom-api",
                timeout: TimeSpan.FromSeconds(10));
        }

        return services;
    }
}
```

#### ApplicationBuilderExtensions.cs
```csharp
namespace QuickReserve.API.Extensions;

using QuickReserve.API.Middleware;

public static class ApplicationBuilderExtensions
{
    public static IApplicationBuilder UseGlobalExceptionHandler(this IApplicationBuilder app)
    {
        return app.UseMiddleware<GlobalExceptionMiddleware>();
    }

    public static IApplicationBuilder UseCorrelationId(this IApplicationBuilder app)
    {
        return app.UseMiddleware<CorrelationIdMiddleware>();
    }
}
```

### 5.5 Program.cs

```csharp
using QuickReserve.API.Extensions;
using QuickReserve.Application;
using QuickReserve.Infrastructure;
using Serilog;

var builder = WebApplication.CreateBuilder(args);

// Serilog Configuration
Log.Logger = new LoggerConfiguration()
    .ReadFrom.Configuration(builder.Configuration)
    .Enrich.FromLogContext()
    .Enrich.WithMachineName()
    .Enrich.WithThreadId()
    .Enrich.WithEnvironmentName()
    .WriteTo.Console()
    .WriteTo.Elasticsearch(new Serilog.Sinks.Elasticsearch.ElasticsearchSinkOptions(
        new Uri(builder.Configuration["Elasticsearch:Uri"] ?? "http://localhost:9200"))
    {
        AutoRegisterTemplate = true,
        AutoRegisterTemplateVersion = Serilog.Sinks.Elasticsearch.AutoRegisterTemplateVersion.ESv8,
        IndexFormat = $"quickreserve-logs-{DateTime.UtcNow:yyyy.MM.dd}"
    })
    .CreateLogger();

builder.Host.UseSerilog();

// Add services
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerDocumentation();
builder.Services.AddCorsPolicy();
builder.Services.AddHealthChecksConfiguration(builder.Configuration);

// Add Application & Infrastructure layers
builder.Services.AddApplication();
builder.Services.AddInfrastructure(builder.Configuration);

var app = builder.Build();

// Configure pipeline
app.UseCorrelationId();
app.UseGlobalExceptionHandler();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "QuickReserve API v1");
        c.RoutePrefix = string.Empty;
    });
}

app.UseHttpsRedirection();
app.UseCors("AllowAngular");
app.UseAuthorization();
app.MapControllers();
app.MapHealthChecks("/health");

app.Run();

// Para tests de integracion
public partial class Program { }
```

### 5.6 appsettings.json

```json
{
  "Logging": {
    "LogLevel": {
      "Default": "Information",
      "Microsoft.AspNetCore": "Warning"
    }
  },
  "AllowedHosts": "*",
  "ConnectionStrings": {
    "Redis": "localhost:6379"
  },
  "TecnomApi": {
    "BaseUrl": "https://dev.tecnomcrm.com/api/v1/",
    "Username": "REDACTED_USERNAME",
    "Password": "REDACTED_PASSWORD",
    "CacheExpirationMinutes": 5,
    "TimeoutSeconds": 30,
    "RetryCount": 3
  },
  "Elasticsearch": {
    "Uri": "http://localhost:9200"
  },
  "Serilog": {
    "MinimumLevel": {
      "Default": "Information",
      "Override": {
        "Microsoft": "Warning",
        "System": "Warning",
        "Microsoft.Hosting.Lifetime": "Information"
      }
    }
  }
}
```

### 5.7 appsettings.Development.json

```json
{
  "Logging": {
    "LogLevel": {
      "Default": "Debug",
      "Microsoft.AspNetCore": "Information"
    }
  },
  "Serilog": {
    "MinimumLevel": {
      "Default": "Debug"
    }
  }
}
```

### 5.8 Commit Fase 5

```bash
git add .
git commit -m "feat(api): add controllers, middleware, health checks and Serilog

- Add AppointmentsController with GET and POST endpoints
- Add WorkshopsController with GET endpoint
- Add GlobalExceptionMiddleware for centralized error handling
- Add CorrelationIdMiddleware for request tracking
- Add Swagger documentation with XML comments
- Add CORS policy for Angular frontend
- Add Health Checks for Redis and Tecnom API
- Configure Serilog with Elasticsearch sink
- Add Program.cs with full DI configuration"
```

---

## Fase 6 - Tests

> Contenido detallado de tests unitarios y de integracion...
> (Se incluiran tests para Value Objects, Entities, Validators, Services y Controllers)

---

## Fase 7 - Docker

### 7.1 Dockerfile

```dockerfile
# Build stage
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

# Copy csproj files and restore
COPY ["src/QuickReserve.Domain/QuickReserve.Domain.csproj", "src/QuickReserve.Domain/"]
COPY ["src/QuickReserve.Application/QuickReserve.Application.csproj", "src/QuickReserve.Application/"]
COPY ["src/QuickReserve.Infrastructure/QuickReserve.Infrastructure.csproj", "src/QuickReserve.Infrastructure/"]
COPY ["src/QuickReserve.API/QuickReserve.API.csproj", "src/QuickReserve.API/"]
RUN dotnet restore "src/QuickReserve.API/QuickReserve.API.csproj"

# Copy everything and build
COPY . .
WORKDIR "/src/src/QuickReserve.API"
RUN dotnet build "QuickReserve.API.csproj" -c Release -o /app/build

# Publish stage
FROM build AS publish
RUN dotnet publish "QuickReserve.API.csproj" -c Release -o /app/publish /p:UseAppHost=false

# Runtime stage
FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS final
WORKDIR /app
EXPOSE 8080
EXPOSE 8081

COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "QuickReserve.API.dll"]
```

### 7.2 docker-compose.yml

```yaml
version: '3.8'

services:
  quickreserve-api:
    build:
      context: .
      dockerfile: Dockerfile
    container_name: quickreserve-api
    ports:
      - "5000:8080"
      - "5001:8081"
    environment:
      - ASPNETCORE_ENVIRONMENT=Development
      - ConnectionStrings__Redis=redis:6379
      - Elasticsearch__Uri=http://elasticsearch:9200
    depends_on:
      - redis
      - elasticsearch
    networks:
      - quickreserve-network

  redis:
    image: redis:7-alpine
    container_name: quickreserve-redis
    ports:
      - "6379:6379"
    volumes:
      - redis-data:/data
    networks:
      - quickreserve-network

  elasticsearch:
    image: docker.elastic.co/elasticsearch/elasticsearch:8.12.0
    container_name: quickreserve-elasticsearch
    environment:
      - discovery.type=single-node
      - xpack.security.enabled=false
      - "ES_JAVA_OPTS=-Xms512m -Xmx512m"
    ports:
      - "9200:9200"
    volumes:
      - elasticsearch-data:/usr/share/elasticsearch/data
    networks:
      - quickreserve-network

  kibana:
    image: docker.elastic.co/kibana/kibana:8.12.0
    container_name: quickreserve-kibana
    environment:
      - ELASTICSEARCH_HOSTS=http://elasticsearch:9200
    ports:
      - "5601:5601"
    depends_on:
      - elasticsearch
    networks:
      - quickreserve-network

  sonarqube:
    image: sonarqube:community
    container_name: quickreserve-sonarqube
    environment:
      - SONAR_JDBC_URL=jdbc:postgresql://sonar-db:5432/sonar
      - SONAR_JDBC_USERNAME=sonar
      - SONAR_JDBC_PASSWORD=sonar
    ports:
      - "9000:9000"
    depends_on:
      - sonar-db
    volumes:
      - sonarqube-data:/opt/sonarqube/data
      - sonarqube-extensions:/opt/sonarqube/extensions
      - sonarqube-logs:/opt/sonarqube/logs
    networks:
      - quickreserve-network

  sonar-db:
    image: postgres:16-alpine
    container_name: quickreserve-sonar-db
    environment:
      - POSTGRES_USER=sonar
      - POSTGRES_PASSWORD=sonar
      - POSTGRES_DB=sonar
    volumes:
      - sonar-db-data:/var/lib/postgresql/data
    networks:
      - quickreserve-network

networks:
  quickreserve-network:
    driver: bridge

volumes:
  redis-data:
  elasticsearch-data:
  sonarqube-data:
  sonarqube-extensions:
  sonarqube-logs:
  sonar-db-data:
```

### 7.3 .dockerignore

```
**/.git
**/.gitignore
**/.vs
**/.vscode
**/.idea
**/bin
**/obj
**/node_modules
**/.dockerignore
**/Dockerfile*
**/docker-compose*
**/*.md
**/*.log
**/coverage
**/.sonarqube
```

### 7.4 Commit Fase 7

```bash
git add .
git commit -m "feat(docker): add Dockerfile and docker-compose with all services

- Add multi-stage Dockerfile for optimized builds
- Add docker-compose with API, Redis, ELK stack, SonarQube
- Add .dockerignore for faster builds"
```

---

## Fase 8 - SonarQube

> Configuracion de sonar-project.properties y scripts de analisis...

---

## Fase 9 - CI/CD GitHub Actions

> Workflows de build, test y analisis...

---

## Fase 10 - HTTP Files y Documentacion

### 10.1 requests.http

```http
### Variables
@baseUrl = http://localhost:5000/api

### Health Check
GET {{baseUrl}}/../health
Accept: application/json

### Get All Workshops
GET {{baseUrl}}/workshops
Accept: application/json

### Get All Appointments
GET {{baseUrl}}/appointments
Accept: application/json

### Create Appointment - Valid
POST {{baseUrl}}/appointments
Content-Type: application/json

{
  "place_id": 2222,
  "appointment_at": "2026-12-01T10:00:00Z",
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

### Create Appointment - Without Vehicle
POST {{baseUrl}}/appointments
Content-Type: application/json

{
  "place_id": 2222,
  "appointment_at": "2026-12-01T14:00:00Z",
  "service_type": "Revision",
  "contact": {
    "name": "Maria Garcia",
    "email": "maria@email.com",
    "phone": "+5491166662222"
  }
}

### Create Appointment - Invalid (missing fields)
POST {{baseUrl}}/appointments
Content-Type: application/json

{
  "place_id": 0,
  "appointment_at": "2020-01-01T10:00:00Z",
  "service_type": "",
  "contact": {
    "name": "",
    "email": "invalid-email",
    "phone": "123"
  }
}
```

---

## Fase 11 - Validacion Final

### Checklist de Validacion

- [ ] `dotnet build` sin warnings
- [ ] `dotnet test` todos los tests pasan
- [ ] `docker-compose up` levanta todos los servicios
- [ ] Swagger UI accesible en http://localhost:5000
- [ ] Health Check responde OK en http://localhost:5000/health
- [ ] Kibana accesible en http://localhost:5601
- [ ] SonarQube accesible en http://localhost:9000
- [ ] POST /api/appointments crea turno correctamente
- [ ] GET /api/appointments lista turnos
- [ ] GET /api/workshops retorna talleres de Tecnom
- [ ] Logs visibles en Kibana
- [ ] SonarQube muestra 0 bugs, 0 vulnerabilities
- [ ] Commits siguen Conventional Commits

---

## Comandos Utiles

```bash
# Build
dotnet build

# Run tests
dotnet test

# Run tests with coverage
dotnet test --collect:"XPlat Code Coverage"

# Run API locally
dotnet run --project src/QuickReserve.API

# Docker
docker-compose up -d
docker-compose down
docker-compose logs -f quickreserve-api

# SonarQube analysis
dotnet sonarscanner begin /k:"QuickReserve" /d:sonar.host.url="http://localhost:9000"
dotnet build
dotnet sonarscanner end
```

---

## Tags

#QuickReserve #Backend #Implementacion #DotNet #CleanArchitecture #DDD

