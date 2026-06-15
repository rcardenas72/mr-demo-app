# Architecture & Conventions

## Cómo usar este documento con IA

Si estás usando asistentes de IA (como opencode, ChatGPT, etc.) para generar
código en este proyecto, indícales:

> Lee `docs/ARCHITECTURE.md` antes de escribir cualquier código para entender
> las convenciones del proyecto.

Esto asegura que el código generado siga los patrones establecidos.

---

## Stack

| Componente | Tecnología |
|---|---|
| Runtime | .NET 8 |
| ORM | Entity Framework Core |
| DB | MySQL / SQL Server / InMemory (configurable) |
| Auth | Azure AD (OpenIdConnect) o Cookie Auth |
| Mapping | Riok.Mapperly |
| Logging | Serilog |
| Metrics | Prometheus |
| DemoApp engine | dotnet new |

---

## ⚠️ Advertencia de seguridad: Cookie Auth (modo demo)

Cuando se usa `--useAzureAd false`, la aplicación utiliza autenticación por cookie
**sin verificación de contraseña**. Cualquier usuario existente y activo en la BD
puede autenticarse solo con su nombre de usuario.

**Este modo está pensado exclusivamente para desarrollo/demo local con
InMemory database.** No es apto para producción.

### Pasos para producción con autenticación propia

1. Agregar campo `PasswordHash` a `AppUser`
2. Usar BCrypt (o similar) para verificar contraseña en `AccountController.Login`
3. Agregar campo de contraseña al formulario de login
4. Implementar registro de usuarios con hash de contraseña

---

## Arquitectura de capas (obligatorio)

```
Controller → Service → Repository → DbContext (EF Core)
```

### Reglas de dependencia

| Capa | Depende de | NO depende de |
|---|---|---|
| Controller | Service interfaces, ViewModels | Repositorios, DbContext |
| Service | Repository interfaces, Models | HttpContext, Views |
| Repository | DbContext, Models | Services, HttpContext |
| Model | Nada (POCO) | Ninguna otra capa |

### Controller

- Solo orquesta: recibe request, llama al servicio, verifica `Result.IsSuccess`,
  retorna View o Redirect.
- NO contiene lógica de negocio ni acceso a repositorios.
- NO accede a `HttpContext.RequestServices` (eso es Service Locator).

### Service

- Contiene toda la lógica de negocio y validaciones.
- Hereda de `BaseService<TService>` (obtiene `LogInfo`, `LogError`, `GetUsername()`,
  `ExecuteAsync`).
- Todos los métodos devuelven `Result<T>` o `Result` — nunca lanzan excepciones
  al controlador.
- Las operaciones que acceden al repositorio se envuelven con `ExecuteAsync()` en
  lugar de try/catch manual. Esto registra el error y retorna `Result.Failure` automáticamente.
- Solo se usa try/catch manual cuando se necesita capturar una excepción específica
  (ej. `DbUpdateException` para errores de integridad referencial).

### Repository

- Solo CRUD y consultas con EF Core.
- NO tiene lógica de negocio.
- Usa `FindAsync` para búsqueda por ID, `ToListAsync` para listas.
- Para consultas paginadas: `Skip`/`Take` con `OrderBy`.

---

## Patrón Result

```csharp
// Ubicación: Services/Utils/Result.cs

// Sin datos
public static Result Success();
public static Result Failure(string message);

// Con datos
public static Result<T> Success(T data);
public static Result<T> Failure(string message);
```

### Reglas

- El servicio **nunca** lanza `Exception` al controlador.
- El servicio **siempre** devuelve `Result` o `Result<T>`.
- El controlador verifica `result.IsSuccess` antes de continuar.
- Mensajes de error en español, orientados al usuario final.
- `Result<T>` tiene `implicit operator bool` → permite `if (result)`.

---

## Modelos y auditoría

### AuditableEntity (default)

Toda entidad de negocio hereda de `Models/Shared/AuditableEntity`:

```csharp
public abstract class AuditableEntity
{
    public string InsUser { get; set; }   // Quién creó
    public DateTime InsDate { get; set; } // Cuándo creó (UTC)
    public string? UpdUser { get; set; }  // Quién modificó
    public DateTime? UpdDate { get; set; } // Cuándo modificó (UTC)
}
```

Estos campos los asigna automáticamente el `AuditInterceptor` al hacer
`SaveChangesAsync`. **No se asignan manualmente** en servicios ni repositorios.

### INotAuditableEntity (opt-out)

Si una entidad no debe auditarse:

```csharp
public class Foo : AuditableEntity, INotAuditableEntity
```

El interceptor la salta. En ese caso debes asignar `InsDate`/`UpdDate`
**manualmente** en el repositorio o servicio.

### [NotAudited]

Marca propiedades específicas para excluirlas del log de auditoría:

```csharp
[NotAudited]
public string InternalNotes { get; set; }
```

---

## Vistas

- **Create y Edit comparten** una sola vista `Form.cshtml`.
  La diferencia se determina por `Model.Id > 0`.
- **Index** usa `PagedResultViewModel<T>` con paginación.
- **Delete** muestra confirmación con los datos de la entidad.
- Los permisos de botones se controlan con `HasPermission("CanAdd")`,
  `HasPermission("CanEdit")`, `HasPermission("CanDelete")`.

---

## Sidebar

Archivo: `Views/Shared/_Sidebar.cshtml`

- Cada sección es un dropdown que se muestra si el usuario tiene **al menos un**
  permiso del grupo.
- Cada link se muestra si el usuario tiene **ese permiso específico**.
- Si `IsAdmin = true`, se muestran todos los links.

---

## Mapping con Riok.Mapperly

`AppMapper` es la fachada pública; `AppMapperImpl` es la clase partial que Mapperly
usa como source generator para crear las implementaciones en build-time (sin reflection).
Ver `Mappings/AppMapper.cs`.

### Cuándo usar mapper

| Situación | Recomendación |
|---|---|
| Entidad con **5+ campos** o ViewModel con listas de selección | Usar `AppMapper` |
| Entidad con **2-3 campos** | Binding directo de la entidad (sin ViewModel ni mapper) |

### Métodos existentes

| Método | Origen → Destino |
|--------|-----------------|
| `UserFormToEntity` | `UserFormViewModel` → `AppUser` |
| `EntityToUserForm` | `AppUser` → `UserFormViewModel` |
| `Map` | `UserFormViewModel` + `AppUser` existente (merge) |
| `PermissionFormToEntity` | `PermissionFormViewModel` → `Permission` |
| `EntityToPermissionForm` | `Permission` → `PermissionFormViewModel` |

### Registro

```csharp
builder.Services.AddSingleton<AppMapper>();
```

---

## DI Registration (Program.cs)

- Todos los repositorios y servicios se registran como `AddScoped`.
- Orden: repositorios primero, luego servicios (aunque el orden no importa para DI).

```csharp
builder.Services.AddScoped<IProductRepository, ProductRepository>();
builder.Services.AddScoped<IProductService, ProductService>();
```

---

## Decisiones arquitectónicas clave

### ¿Por qué Service Locator en PermissionFilterAttribute?

**Contexto:** Los atributos de C# solo aceptan constantes en el constructor.
`PermissionFilterAttribute` necesita `OperationType` (constante) y servicios
de DI (no constantes). Hay 28 anotaciones `[PermissionFilter(OperationType.X)]`
en 5 controladores que usan esta sintaxis.

**Decisión actual:** Se resuelven los servicios directamente desde
`HttpContext.RequestServices.GetRequiredService<T>()` dentro del método
`OnActionExecutionAsync`. Es Service Locator, pero es un trade-off aceptado:
la alternativa (IFilterFactory + filtro separado con DI) añade complejidad
estructural para un beneficio marginal, ya que el filtro se ejecuta una vez
por request y las dependencias son estables.

**Alternativa evaluada (no implementada):** Refactorizar a `IFilterFactory`
con un `PermissionFilter` separado que reciba las dependencias por constructor.
El atributo se mantendría como fachada que solo construye el filtro en
`CreateInstance`. Se descartó por ser un cambio puramente mecánico sin
beneficio funcional real.

### ¿Por qué DateTime.UtcNow y no DateTime.Now?

**Contexto:** Servidores en distintas zonas horarias.

**Decisión:** Todas las marcas de tiempo usan `DateTime.UtcNow`. La conversión
a hora local se hace solo en la capa de presentación (vistas) si es necesario.

### ¿Por qué Result<T> y no excepciones?

**Contexto:** Flujo de control con excepciones es difícil de rastrear y genera
try/catch en cada controlador.

**Decisión:** Patrón Result explícito. El servicio captura todas las excepciones
y devuelve `Result.Failure`. El controlador verifica `IsSuccess` con un `if`.

### ¿Por qué Dependabot con ignore rules?

**Contexto:** El demoapp distribuye packages NuGet con versiones fijas. Sin
actualización automática, los proyectos generados nacen con dependencias
potencialmente desactualizadas.

**Decisión:** Dependabot semanal con ignore rules para paquetes acoplados al
runtime (Microsoft.EntityFrameworkCore*, Microsoft.AspNetCore*, Pomelo* ≥ 9.x).
Esto mantiene actualizados los packages que soportan múltiples targets
(Serilog, prometheus-net, Mapperly) sin forzar migraciones de runtime.
Ver `.github/dependabot.yml`.

**Cuándo revisar:** Al migrar a .NET 10 LTS, actualizar los version ranges
de los ignore rules.

### ¿Por qué un smoke test automático?

**Contexto:** El demoapp usa directivas `#if (simbolo)` que el demoapp engine
evalúa al generar un proyecto. El build del demoapp NO evalúa estas directivas,
por lo que cambios en ellas pueden romper combinaciones sin que se detecten
hasta que alguien ejecuta `dotnet new`.

**Decisión:** Smoke test automático en CI (GitHub Action) que genera proyectos
con todas las combinaciones de parámetros y verifica que compilen. El script
`scripts/smoke-test.sh` se ejecuta en cada PR y push a main.

**Variantes evaluadas:**
| Parámetros | Propósito |
|---|---|
| `--useAzureAd true --dbProvider mysql` | Default del demoapp |
| `--useAzureAd false --dbProvider inmemory` | Modo demo sin BD externa |
| `--useAzureAd false --dbProvider sqlserver` | Sin Azure AD con SQL Server |
| `--useAzureAd false --dbProvider mysql` | Sin Azure AD con MySQL |

**Nota:** Los nombres de variante usan guiones bajos (`no_ad_mysql`) porque
`dotnet new` sanitiza los guiones en nombres de proyecto.

### ¿Por qué AllowAnonymous en métricas en vez de RequireAuthorization?

**Contexto:** Prometheus no puede autenticarse contra Azure AD.

**Decisión:** `MetricsAuthMiddleware` protege `/metrics` con API Key
(header `X-Api-Key`). `RequireAuthorization("AllowAnonymous")` evita que el
middleware de autorización interfiera. En producción se configura
`Metrics:ApiKey`. Ver `Middleware/MetricsAuthMiddleware.cs`.

---

## Cache

### Cache de permisos (IMemoryCache)

| Cache Key | Duración | Propósito |
|---|---|---|
| `permissions_{username}` | 5 min abs | Permisos para el filtro de acción |
| `{username}_AccessibleMenus` | 10 min | Menús visibles en el sidebar |

Invalidación manual:

```csharp
_cache.Remove($"permissions_{username}");
_cache.Remove($"{username}_AccessibleMenus");
```

---

## Tablas maestras con IsActive

Toda tabla maestra que otras entidades referencian vía FK y tenga campo
`IsActive` (ej. Roles) debe implementar este patrón:

### Reglas

| Situación | Comportamiento |
|---|---|
| Dropdown de selección (crear/editar) | Mostrar solo registros activos |
| Dropdown de filtro (listados) | Mostrar solo registros activos |
| Display de valor existente (columnas, vistas) | Mostrar el nombre siempre, aunque esté inactivo |
| POST crear/editar | Validar servidor-side que el FK referencie un registro activo |
| Desactivar registro | Bloquear si existen dependencias activas (hijos que lo referencien y estén activos) |

### API requerida en el repositorio

| Método | Propósito |
|---|---|
| `Task<List<T>> GetActiveAsync()` | Listar solo registros con `IsActive == true` |
| `Task<bool> IsActiveAsync(int id)` | Verificar si un registro específico está activo |
| `Task<bool> HasActiveDependentsAsync(int id)` | Verificar si hay entidades relacionadas activas que impidan desactivar/eliminar |

### Implementación en servicio

El servicio expone `GetActiveAsync()` para los dropdowns y valida en
Create/Update que el FK corresponda a un registro activo. Para el bloqueo
por dependencias, se verifica antes de cambiar `IsActive` a `false` o eliminar.

### Dónde aplicar

Cada vez que crees una entidad que:
- Tenga campo `IsActive`, y
- Sea referenciada vía FK desde otras entidades

El paso a paso con código concreto está en `docs/NEW_MODULE_GUIDE.md`
(paso 1 — Modelo, callout "¿Tu entidad es una tabla maestra con IsActive?").

## Convenciones de código

- **Nulos:** `Nullable` habilitado. Strings inicializados con `= string.Empty`.
- **Errores:** Mensajes en español, consistentes: "Error al {acción} {entidad}".
- **Logging:** Usar métodos de `BaseService`: `LogInfo`, `LogWarning`, `LogError`.
- **Error handling:** Usar `ExecuteAsync()` de `BaseService` en lugar de try/catch manual.
  Solo usa try/catch explícito cuando necesites capturar excepciones específicas
  (ej. `DbUpdateException`).
- **Excepciones esperadas:** `DbUpdateException` para errores de integridad referencial.
- **Anti-forgery:** `[ValidateAntiForgeryToken]` en todos los POST.
- **AsNoTracking:** Toda consulta de solo lectura (listados, paginados, filtros)
  debe usar `.AsNoTracking()` para evitar que EF Core rastree entidades
  innecesariamente. Excepciones: consultas usadas en flujos de edición
  (`GetByIdAsync`) y `AnyAsync`/`CountAsync` (no devuelven entidades).
- **Namespaces:** Modelos en `DemoApp.Web.Models`, Repositorios en `DemoApp.Web.Repositories`,
  Servicios en `DemoApp.Web.Services`.

---

## Guía paso a paso para nuevos módulos

Ver `docs/NEW_MODULE_GUIDE.md` para el tutorial completo con código demoapp.
