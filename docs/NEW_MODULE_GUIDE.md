# Guía para agregar un nuevo módulo

Esta guía describe el flujo completo para añadir una nueva entidad de negocio
(Producto, Cliente, Factura, etc.) siguiendo la arquitectura del demoapp.

---

## Principios generales

- **SOLID** — Son principios, no leyes. Úsalos como guía, no como dogma.
  - *S* — Una clase, una responsabilidad.
  - *O* — Abierto a extensión, cerrado a modificación.
  - *L* — Los subtipos deben poder reemplazar a sus bases.
  - *I* — Interfaces pequeñas y específicas.
  - *D* — Depende de abstracciones, no de concretos.
- **KISS** — Prefiere la solución más simple que funcione.
- **DRY** — No repitas lógica. Si ves el mismo patrón 3 veces, extraélo.
- **YAGNI** — No agregues funcionalidad "por si acaso". Agrégala cuando la necesites.

---

## Arquitectura general

```
┌──────────┐     ┌──────────┐     ┌──────────┐     ┌──────────────┐
│  View    │ ──▶ │ Controller│ ──▶ │ Service  │ ──▶ │  Repository  │ ──▶ DB
│ (Razor)  │     │ (thin)   │     │ (Result) │     │  (EF Core)   │
└──────────┘     └──────────┘     └──────────┘     └──────────────┘
                       │                │
                       │                ▼
                       │         ┌──────────────┐
                       │         │  BaseService  │
                       │         │  (logging,    │
                       │         │   username)   │
                       │         └──────────────┘
                       ▼
                 ┌──────────┐
                 │  Filters  │
                 │(Auth/Perm)│
                 └──────────┘
```

### Capas

| Capa | Responsabilidad | NO debe hacer |
|------|----------------|---------------|
| **Model** | Datos + reglas de validación básicas | Llamar a BD o servicios |
| **Repository** | CRUD contra la BD | Lógica de negocio |
| **Service** | Lógica de negocio, validaciones, Result | Acceder a `HttpContext` o vistas |
| **Controller** | Orquestar request/response, validar ModelState | Lógica de negocio |
| **View** | Renderizar HTML | Llamar a repositorios o servicios |

### Flujo de datos

```
Request → Controller → Service → Repository → DB
                          ↓
                    Result<T> / Result
                          ↓
               Controller verifica IsSuccess
                          ↓
                    View / Redirect
```

---

## Paso a paso: Agregar "Product"

### 1. Modelo

Crea `Models/Product.cs`. Si quieres auditoría automática, hereda de
`AuditableEntity`. Si no quieres que se audite, implementa `INotAuditableEntity`.

```csharp
using DemoApp.Web.Models.Shared;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace DemoApp.Web.Models
{
    public class Product : AuditableEntity
    {
        [Key]
        public int ProductId { get; set; }

        [DisplayName("Nombre")]
        [StringLength(100)]
        [Required(ErrorMessage = "El nombre es obligatorio.")]
        public string Name { get; set; } = string.Empty;

        [DisplayName("Precio")]
        [Required(ErrorMessage = "El precio es obligatorio.")]
        [Range(0.01, 999999, ErrorMessage = "El precio debe ser mayor a 0.")]
        public decimal Price { get; set; }

        [DisplayName("Activo")]
        public bool IsActive { get; set; } = true;
    }
}
```

> Si heredas de `AuditableEntity`, obtienes automáticamente:
> `InsUser`, `InsDate`, `UpdUser`, `UpdDate`. El `AuditInterceptor` los asigna
> solos al guardar — no necesitas tocarlos en tu código.

> Si usas `INotAuditableEntity`, el interceptor salta tu entidad y debes
> asignar `InsDate`/`UpdDate` manualmente en el repositorio o servicio.

> **¿Tu entidad es una tabla maestra con `IsActive`?**
>
> Si otras entidades referencian esta vía FK y tiene campo `IsActive`,
> debes implementar el patrón de "Tablas maestras con IsActive":
>
> **Repositorio** — agrega estos métodos:
> ```csharp
> public async Task<List<Product>> GetActiveAsync() =>
>     await _context.Products.Where(p => p.IsActive).AsNoTracking().ToListAsync();
>
> public async Task<bool> IsActiveAsync(int id) =>
>     await _context.Products.AnyAsync(p => p.ProductId == id && p.IsActive);
> ```
>
> **Interfaz del repositorio** — agrega las firmas:
> ```csharp
> Task<List<Product>> GetActiveAsync();
> Task<bool> IsActiveAsync(int id);
> ```
>
> **Servicio** — expone `GetActiveAsync()` para dropdowns y valida el
> FK activo en Create/Update:
> ```csharp
> public async Task<Result<List<Product>>> GetActiveAsync()
>     => await ExecuteAsync(() => _repository.GetActiveAsync(),
>         "Error al obtener productos activos", "Error al obtener productos activos.");
> ```
>
> En el método `ValidateAsync`, antes de crear/editar:
> ```csharp
> if (!await _repository.IsActiveAsync(product.CategoryId))
>     return Result.Failure("La categoría seleccionada no está activa.");
> ```
>
> **Controller** — usa `GetActiveAsync()` para los dropdowns en lugar
> de `GetAllAsync()`:
> ```csharp
> var productosResult = await _productService.GetActiveAsync();
> ```
>
> Si además necesitas bloquear la desactivación cuando haya dependencias,
> agrega en el repositorio:
> ```csharp
> public async Task<bool> HasActiveDependentsAsync(int id) =>
>     await _context.Products.AnyAsync(p => p.RoleId == id && p.IsActive);
> ```
>
> Y en el servicio, antes de desactivar (UpdateAsync):
> ```csharp
> if (existing.IsActive && !product.IsActive
>     && await _repository.HasActiveDependentsAsync(product.ProductId))
>     return Result.Failure("No se puede desactivar porque tiene dependencias activas.");
> ```
>
> Ver `docs/ARCHITECTURE.md` sección "Tablas maestras con IsActive" para
> el detalle de las reglas generales.

### 2. DbContext

Agrega el `DbSet` en `Data/AppDbContext.cs`:

```csharp
public DbSet<Product> Products { get; set; }
```

Y en `OnModelCreating`:

```csharp
modelBuilder.Entity<Product>().ToTable("Products");
```

### 3. Interfaz del repositorio

Crea `Repositories/Interfaces/IProductRepository.cs`:

```csharp
using DemoApp.Web.Models;

namespace DemoApp.Web.Repositories.Interfaces
{
    public interface IProductRepository
    {
        Task<List<Product>> GetAllAsync();
        Task<Product?> GetByIdAsync(int id);
        Task CreateAsync(Product product);
        Task UpdateAsync(Product product);
        Task DeleteAsync(int id);
        Task<bool> ExistsByNameAsync(string name);
        Task<bool> ExistsByNameAsync(string name, int excludeId);
        Task<List<Product>> GetPagedAsync(int pageNumber, int pageSize);
        Task<int> CountAsync();
    }
}
```

Incluye solo los métodos que realmente necesites. No agregues `ExistsBy...` si
tu entidad no requiere validación de unicidad.

### 4. Repositorio

Crea `Repositories/ProductRepository.cs`:

```csharp
using Microsoft.EntityFrameworkCore;
using DemoApp.Web.Data;
using DemoApp.Web.Models;
using DemoApp.Web.Repositories.Interfaces;

namespace DemoApp.Web.Repositories
{
    public class ProductRepository : IProductRepository
    {
        private readonly AppDbContext _context;

        public ProductRepository(AppDbContext context)
        {
            _context = context;
        }

        public async Task<List<Product>> GetAllAsync() =>
            await _context.Products.AsNoTracking().ToListAsync();

        public async Task<Product?> GetByIdAsync(int id) =>
            await _context.Products.FindAsync(id);

        public async Task CreateAsync(Product product)
        {
            _context.Products.Add(product);
            await _context.SaveChangesAsync();
        }

        public async Task UpdateAsync(Product product)
        {
            _context.Products.Update(product);
            await _context.SaveChangesAsync();
        }

        public async Task DeleteAsync(int id)
        {
            var product = await _context.Products.FindAsync(id);
            if (product == null) return;

            _context.Products.Remove(product);
            await _context.SaveChangesAsync();
        }

        public async Task<bool> ExistsByNameAsync(string name) =>
            await _context.Products.AnyAsync(x =>
                x.Name.ToUpper().Trim() == name.ToUpper().Trim());

        public async Task<bool> ExistsByNameAsync(string name, int excludeId) =>
            await _context.Products.AnyAsync(x =>
                x.Name.ToUpper().Trim() == name.ToUpper().Trim()
                && x.ProductId != excludeId);

        public async Task<List<Product>> GetPagedAsync(int pageNumber, int pageSize)
        {
            var skip = (pageNumber - 1) * pageSize;
            return await _context.Products
                .OrderBy(x => x.Name)
                .Skip(skip)
                .Take(pageSize)
                .ToListAsync();
        }

        public async Task<int> CountAsync() =>
            await _context.Products.CountAsync();
    }
}
```

> **Importante:** Si tu entidad hereda de `AuditableEntity`, el `AuditInterceptor`
> asigna `InsDate`, `InsUser`, `UpdDate`, `UpdUser` automáticamente al llamar a
> `SaveChangesAsync`. No los asignes manualmente.

> Si tu entidad implementa `INotAuditableEntity`, el interceptor la salta y
> debes asignar `InsDate`/`UpdDate` en el repositorio.

### 5. Interfaz del servicio

Crea `Services/Interfaces/IProductService.cs`:

```csharp
using DemoApp.Web.Models;
using DemoApp.Web.Services.Utils;

namespace DemoApp.Web.Services.Interfaces
{
    public interface IProductService
    {
        Task<Result<List<Product>>> GetAllAsync();
        Task<Result<Product?>> GetByIdAsync(int id);
        Task<Result> CreateAsync(Product product);
        Task<Result> UpdateAsync(Product product);
        Task<Result> DeleteAsync(int id);
        Task<Result<List<Product>>> GetPagedAsync(int page, int pageSize);
        Task<Result<int>> CountAsync();
    }
}
```

### 6. Servicio

Crea `Services/ProductService.cs`:

```csharp
using Microsoft.EntityFrameworkCore;
using DemoApp.Web.Models;
using DemoApp.Web.Repositories.Interfaces;
using DemoApp.Web.Services.Interfaces;
using DemoApp.Web.Services.Utils;

namespace DemoApp.Web.Services
{
    public class ProductService : BaseService<ProductService>, IProductService
    {
        private readonly IProductRepository _repository;

        public ProductService(
            IProductRepository repository,
            ICurrentUserService currentUserService,
            ILogger<ProductService> logger)
            : base(logger, currentUserService)
        {
            _repository = repository;
        }

        public async Task<Result<List<Product>>> GetAllAsync()
            => await ExecuteAsync(() => _repository.GetAllAsync(), "Error al obtener productos", "Error al obtener productos.");

        public async Task<Result<Product?>> GetByIdAsync(int id)
            => await ExecuteAsync(() => _repository.GetByIdAsync(id), $"Error al obtener producto ID {id}", "Error al obtener el producto.");

        public async Task<Result> CreateAsync(Product product)
        {
            var validation = await ValidateAsync(product, isEdit: false);
            if (!validation.IsSuccess)
                return validation;

            product.Name = product.Name.Trim();

            return await ExecuteAsync(() => _repository.CreateAsync(product), "Error al crear producto", "Error inesperado al crear el producto.");
        }

        public async Task<Result> UpdateAsync(Product product)
        {
            var validation = await ValidateAsync(product, isEdit: true);
            if (!validation.IsSuccess)
                return validation;

            var existingResult = await GetByIdAsync(product.ProductId);
            if (!existingResult.IsSuccess || existingResult.Data == null)
                return Result.Failure("Producto no encontrado.");

            var existing = existingResult.Data;
            existing.Name = product.Name.Trim();
            existing.Price = product.Price;
            existing.IsActive = product.IsActive;

            return await ExecuteAsync(() => _repository.UpdateAsync(existing), $"Error al actualizar producto ID {product.ProductId}", "Error inesperado al actualizar el producto.");
        }

        public async Task<Result> DeleteAsync(int id)
        {
            try
            {
                await _repository.DeleteAsync(id);
                return Result.Success();
            }
            catch (DbUpdateException ex)
            {
                LogError($"Error de integridad al eliminar producto ID {id}", ex);
                return Result.Failure("No se puede eliminar porque está asociado a otros datos.");
            }
            catch (Exception ex)
            {
                LogError($"Error al eliminar producto ID {id}", ex);
                return Result.Failure("Error inesperado al eliminar.");
            }
        }

        public async Task<Result<List<Product>>> GetPagedAsync(int page, int pageSize)
        {
            page = page < 1 ? 1 : page;
            pageSize = pageSize <= 0 ? 10 : pageSize;

            return await ExecuteAsync(() => _repository.GetPagedAsync(page, pageSize), $"Error al paginar productos P:{page} S:{pageSize}", "Error al obtener productos paginados.");
        }

        public async Task<Result<int>> CountAsync()
            => await ExecuteAsync(() => _repository.CountAsync(), "Error al contar productos", "Error al contar productos.");

        private async Task<Result> ValidateAsync(Product product, bool isEdit)
        {
            if (string.IsNullOrWhiteSpace(product.Name))
                return Result.Failure("El nombre no puede estar vacío.");

            if (product.Name.Trim().Length > 100)
                return Result.Failure("El nombre no debe superar 100 caracteres.");

            try
            {
                bool exists = isEdit
                    ? await _repository.ExistsByNameAsync(product.Name, product.ProductId)
                    : await _repository.ExistsByNameAsync(product.Name);

                if (exists)
                    return Result.Failure("Ya existe un producto con ese nombre.");

                return Result.Success();
            }
            catch (Exception ex)
            {
                LogError("Error al validar producto", ex);
                return Result.Failure("Error inesperado al validar.");
            }
        }
    }
}
```

> **¿Necesitas un mapper?** Si tu entidad tiene muchos campos (>5) o requiere
> un ViewModel con listas de selección, usa `AppMapper` en lugar de mapear
> manualmente campo por campo. Los pasos son:
>
> 1. Agrega el método `partial` con anotaciones en `AppMapperImpl`
>    (`Mappings/AppMapper.cs`).
> 2. Agrega el wrapper `public` en `AppMapper` que delegue al impl.
> 3. Usa `[MapperIgnoreSource]`/`[MapperIgnoreTarget]` para propiedades que
>    no deban copiarse (auditoría, navegaciones, listas de selección).
> 4. Inyecta `AppMapper` en el constructor del servicio.
>
> Ver `docs/ARCHITECTURE.md` sección Mapping para los métodos existentes.

> **Patrón Result:** Toda operación devuelve `Result<T>` (con datos) o `Result`
> (sin datos). El controlador solo verifica `IsSuccess`. Nunca lances excepciones
> hacia el controlador — usa `ExecuteAsync()` de `BaseService` que atrapa la
> excepción, la registra con `LogError` y devuelve `Result.Failure` automáticamente.
> Solo usa try/catch manual cuando necesites capturar excepciones específicas
> (ej. `DbUpdateException` en `DeleteAsync`).

### 7. Controlador

Crea `Controllers/ProductsController.cs`. **Mantenlo delgado** — solo orquesta:

```csharp
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using DemoApp.Web.Models;
using DemoApp.Web.Models.Enums;
using DemoApp.Web.Services.Interfaces;
using DemoApp.Web.Filters;
using DemoApp.Web.ViewModels;

namespace DemoApp.Web.Controllers
{
    [Authorize]
    [PermissionFilter(OperationType.Consultar)]
    public class ProductsController : Controller
    {
        private readonly IProductService _productService;

        public ProductsController(IProductService productService)
        {
            _productService = productService;
        }

        [HttpGet]
        public async Task<IActionResult> Index(int page = 1, int pageSize = 7)
        {
            var countResult = await _productService.CountAsync();
            if (!countResult.IsSuccess)
                return View("Error", countResult.ErrorMessage);

            if (countResult.Data == 0)
                return View(new PagedResultViewModel<Product> { Items = Enumerable.Empty<Product>() });

            var totalPages = (int)Math.Ceiling(countResult.Data / (double)pageSize);
            if (page < 1) page = 1;
            if (page > totalPages) page = totalPages;

            var itemsResult = await _productService.GetPagedAsync(page, pageSize);
            if (!itemsResult.IsSuccess)
                return View("Error", itemsResult.ErrorMessage);

            var viewModel = new PagedResultViewModel<Product>
            {
                Items = itemsResult.Data!,
                CurrentPage = page,
                TotalPages = totalPages
            };

            return View(viewModel);
        }

        [HttpGet]
        [PermissionFilter(OperationType.Agregar)]
        public IActionResult Create()
        {
            return View("Form", new Product());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [PermissionFilter(OperationType.Agregar)]
        public async Task<IActionResult> Create(Product product)
        {
            if (!ModelState.IsValid)
                return View("Form", product);

            var result = await _productService.CreateAsync(product);
            if (!result.IsSuccess)
            {
                ModelState.AddModelError(string.Empty, result.ErrorMessage ?? "No se pudo crear.");
                return View("Form", product);
            }

            return RedirectToAction(nameof(Index));
        }

        [HttpGet]
        [PermissionFilter(OperationType.Editar)]
        public async Task<IActionResult> Edit(int id)
        {
            var result = await _productService.GetByIdAsync(id);
            if (!result.IsSuccess)
                return View("Error", result.ErrorMessage);

            if (result.Data == null)
                return NotFound();

            return View("Form", result.Data);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [PermissionFilter(OperationType.Editar)]
        public async Task<IActionResult> Edit(Product product)
        {
            if (!ModelState.IsValid)
                return View("Form", product);

            var result = await _productService.UpdateAsync(product);
            if (!result.IsSuccess)
            {
                ModelState.AddModelError(string.Empty, result.ErrorMessage ?? "No se pudo actualizar.");
                return View("Form", product);
            }

            return RedirectToAction(nameof(Index));
        }

        [HttpGet]
        [PermissionFilter(OperationType.Eliminar)]
        public async Task<IActionResult> Delete(int id)
        {
            var result = await _productService.GetByIdAsync(id);
            if (!result.IsSuccess)
                return View("Error", result.ErrorMessage);

            if (result.Data == null)
                return NotFound();

            return View(result.Data);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        [PermissionFilter(OperationType.Eliminar)]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var result = await _productService.DeleteAsync(id);
            if (!result.IsSuccess)
            {
                ModelState.AddModelError(string.Empty, result.ErrorMessage ?? "No se pudo eliminar.");
                var itemResult = await _productService.GetByIdAsync(id);
                return View("Delete", itemResult.IsSuccess ? itemResult.Data : null);
            }

            return RedirectToAction(nameof(Index));
        }
    }
}
```

> **Regla de oro del controlador:** Si tienes más de 3 líneas de lógica en
> una acción, pregúntate si eso debería estar en el servicio.

### 8. Vistas

#### Form.cshtml (Create y Edit comparten la misma vista)

Crea `Views/Products/Form.cshtml`:

```cshtml
@model DemoApp.Web.Models.Product

@{
    bool isEdit = Model.ProductId > 0;
    ViewData["Title"] = isEdit ? "Editar Producto" : "Crear Producto";
    Layout = "~/Views/Shared/_Layout.cshtml";
}

<h2>@ViewData["Title"]</h2>

<form asp-action="@(isEdit ? "Edit" : "Create")" method="post">
    @Html.AntiForgeryToken()
    @if (!ViewData.ModelState.IsValid)
    {
        @Html.ValidationSummary(false, "", new { @class = "text-danger" })
    }

    @if (isEdit)
    {
        <input type="hidden" asp-for="ProductId" />
    }

    <div class="mb-3 row">
        <div class="col-md-4">
            <label asp-for="Name" class="form-label"></label>
            <input asp-for="Name" class="form-control" maxlength="100" />
            <span asp-validation-for="Name" class="text-danger"></span>
        </div>
    </div>

    <div class="mb-3 row">
        <div class="col-md-2">
            <label asp-for="Price" class="form-label"></label>
            <input asp-for="Price" class="form-control" />
            <span asp-validation-for="Price" class="text-danger"></span>
        </div>
    </div>

    <div class="mb-3 row">
        <div class="col-md-2">
            <div class="form-check">
                <input asp-for="IsActive" class="form-check-input" />
                <label asp-for="IsActive" class="form-check-label"></label>
            </div>
        </div>
    </div>

    <div class="mb-3">
        <button type="submit" class="btn btn-primary">Guardar</button>
        <a asp-action="Index" class="btn btn-secondary">Cancelar</a>
    </div>
</form>

@section Scripts {
    @{
        await Html.RenderPartialAsync("_ValidationScriptsPartial");
    }
}
```

> **¿Por qué compartir Form?** Create y Edit son casi idénticos. La única
> diferencia es el `hidden` del ID y el título. DRY — no dupliques la vista.

#### Index.cshtml (lista con paginación)

Crea `Views/Products/Index.cshtml`:

```cshtml
@using DemoApp.Web.Models
@using DemoApp.Web.ViewModels
@model PagedResultViewModel<Product>
@inject Microsoft.AspNetCore.Http.IHttpContextAccessor HttpContextAccessor

@{
    ViewData["Title"] = "Productos";
    Layout = "~/Views/Shared/_Layout.cshtml";

    var context = HttpContextAccessor.HttpContext;
    var isAdmin = context?.Session.GetString("IsAdmin") == "true";

    bool HasPermission(string key) =>
        (context?.Items[key] as bool? == true) || isAdmin;
}

<h2>Productos</h2>

<div class="row">
    <div class="col-md-12">
        <div class="table-responsive">
            <table class="table table-striped">
                <thead>
                    <tr>
                        <th>Nombre</th>
                        <th>Precio</th>
                        <th>Activo</th>
                        <th>Acciones</th>
                    </tr>
                </thead>
                <tbody>
                    @foreach (var item in Model.Items)
                    {
                        <tr>
                            <td>@item.Name</td>
                            <td>@item.Price.ToString("C")</td>
                            <td>@(item.IsActive ? "Sí" : "No")</td>
                            <td>
                                @if (HasPermission("CanEdit"))
                                {
                                    <a asp-action="Edit" asp-route-id="@item.ProductId"
                                       class="btn btn-warning btn-sm">Editar</a>
                                }
                                @if (HasPermission("CanDelete"))
                                {
                                    <a asp-action="Delete" asp-route-id="@item.ProductId"
                                       class="btn btn-danger btn-sm">Eliminar</a>
                                }
                            </td>
                        </tr>
                    }
                </tbody>
            </table>
        </div>

        <nav aria-label="Paginación">
            <ul class="pagination">
                @for (int i = 1; i <= Model.TotalPages; i++)
                {
                    <li class="page-item @(Model.CurrentPage == i ? "active" : "")">
                        <a class="page-link" asp-action="Index" asp-route-page="@i">@i</a>
                    </li>
                }
            </ul>
        </nav>
    </div>
</div>

@if (HasPermission("CanAdd"))
{
    <a asp-action="Create" class="btn btn-primary">Crear Producto</a>
}
```

#### Delete.cshtml (confirmación)

Crea `Views/Products/Delete.cshtml`:

```cshtml
@model DemoApp.Web.Models.Product

@{
    ViewData["Title"] = "Eliminar Producto";
    Layout = "~/Views/Shared/_Layout.cshtml";
}

<h2>Eliminar Producto</h2>

<div class="alert alert-warning mb-4">
    <p><strong>¿Está seguro de que desea eliminar este producto?</strong></p>
    <dl class="row mb-0">
        <dt class="col-sm-3">Nombre:</dt>
        <dd class="col-sm-9">@Model.Name</dd>
        <dt class="col-sm-3">Precio:</dt>
        <dd class="col-sm-9">@Model.Price.ToString("C")</dd>
    </dl>
</div>

<form asp-action="Delete" method="post">
    @Html.AntiForgeryToken()
    @Html.ValidationSummary(false, "", new { @class = "text-danger" })
    <input type="hidden" asp-for="ProductId" />

    <div class="mb-3">
        <button type="submit" class="btn btn-danger">Eliminar</button>
        <a asp-action="Index" class="btn btn-secondary">Cancelar</a>
    </div>
</form>
```

### 9. Sidebar

Edita `Views/Shared/_Sidebar.cshtml`. Agrega el controlador al array de
permisos del grupo que corresponda y el link con su propio permiso:

```cshtml
@if ((permissions?.Any(p => new[] {
    // ... existentes ...
    "Products"
}.Contains(p)) ?? false) || isAdmin)
{
    @if ((permissions?.Contains("Products") ?? false) || isAdmin)
    {
        <li><a class="dropdown-item" href="@Url.Action("Index", "Products")">Productos</a></li>
    }
}
```

Si es un grupo nuevo, replica la estructura completa de dropdown.

### 10. Registro en DI

Agrega en `Program.cs` junto con los demás registros:

```csharp
builder.Services.AddScoped<IProductRepository, ProductRepository>();
builder.Services.AddScoped<IProductService, ProductService>();
```

### 11. Permisos

1. Ve a **Menús** y crea una entrada con `ControllerName = "Products"`.
2. Ve a **Roles**, selecciona un rol y asígnale los permisos necesarios
   (Consultar, Agregar, Editar, Eliminar).

El `PermissionFilterAttribute` resuelve automáticamente el nombre del
controlador desde la ruta y verifica los permisos configurados.

---

## Patrón Result<T>

```csharp
// Sin datos de retorno
public static Result Success();
public static Result Failure(string message);

// Con datos de retorno
public static Result<T> Success(T data);
public static Result<T> Failure(string message);
```

**Reglas de uso:**
- El servicio **nunca** lanza excepciones hacia el controlador.
- Toda operación devuelve `Result` o `Result<T>`.
- El controlador verifica `result.IsSuccess` antes de continuar.
- Los mensajes de error son en español y orientados al usuario final.

---

## Auditoría automática

### AuditableEntity

```csharp
public abstract class AuditableEntity
{
    public string InsUser { get; set; }   // Quién creó
    public DateTime InsDate { get; set; } // Cuándo creó (UTC)
    public string? UpdUser { get; set; }  // Quién modificó
    public DateTime? UpdDate { get; set; } // Cuándo modificó (UTC)
}
```

El `AuditInterceptor` (registrado en EF Core) asigna estos campos
automáticamente al hacer `SaveChangesAsync`. No los toques manualmente.

También crea un registro en `AuditLogs` por cada cambio (Insert/Update/Delete)
con los valores nuevos y antiguos serializados como JSON.

### INotAuditableEntity

Implementa esta interfaz si no quieres que una entidad sea auditada:

```csharp
public class Menu : AuditableEntity, INotAuditableEntity
```

En ese caso el interceptor **salta** la entidad y debes asignar
`InsDate`/`UpdDate` manualmente.

### [NotAudited]

Marca propiedades específicas para excluirlas del log de auditoría:

```csharp
[NotAudited]
public string InsUser { get; set; }
```

Útil para campos sensibles o campos de auditoría propios.

---

## Cache de permisos

El sistema usa dos niveles de cache:

### En el PermissionFilter (por acción)

```csharp
var cacheKey = $"permissions_{username}";
```

- Duración: 5 minutos (absoluta).
- Se crea al primer request del usuario.
- Al expirar, se recarga desde la BD.

### En el PermissionService (navegación sidebar)

```csharp
var cacheKey = $"{userName}_AccessibleMenus";
```

- Duración: 10 minutos.
- Almacena la lista de controladores a los que el usuario tiene acceso.

### Invalidación manual

Ambos expiran solos por TTL. Si necesitas invalidar manualmente (ej. cuando
un administrador cambia permisos en caliente):

```csharp
_cache.Remove($"permissions_{username}");
_cache.Remove($"{username}_AccessibleMenus");
```

Inyecta `IMemoryCache` donde lo necesites.

---

## Resumen / Checklist

- [ ] **Modelo** — `Models/Product.cs` (¿hereda de `AuditableEntity`?)
- [ ] **DbContext** — `DbSet<Product>` + `ToTable`
- [ ] **Interface repositorio** — `Repositories/Interfaces/IProductRepository.cs`
- [ ] **Repositorio** — `Repositories/ProductRepository.cs`
- [ ] **Interface servicio** — `Services/Interfaces/IProductService.cs`
- [ ] **Servicio** — `Services/ProductService.cs` (¿hereda de `BaseService`?)
- [ ] **Controlador** — `Controllers/ProductsController.cs` (¿delgado?)
- [ ] **Vista Form** — `Views/Products/Form.cshtml` (¿compartida Create/Edit?)
- [ ] **Vista Index** — `Views/Products/Index.cshtml` (¿paginación?)
- [ ] **Vista Delete** — `Views/Products/Delete.cshtml` (¿confirmación?)
- [ ] **Sidebar** — `_Sidebar.cshtml` (¿link con permission check?)
- [ ] **DI** — `Program.cs` (`AddScoped<IProductRepository, ProductRepository>()`, etc.)
- [ ] **Permisos** — ¿Menú creado? ¿Permisos asignados al rol?
