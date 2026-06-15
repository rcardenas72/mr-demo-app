# Changelog

## 1.1.5 - 2026-06-13

### Fixed
- `PermissionsController.Create` retornaba la vista `Create` (inexistente) en vez de `Form`, causando `InvalidOperationException` al crear un permiso.

## 1.1.4 - 2026-06-13

### Fixed
- `PermissionsController.Index` retornaba la vista `Form` en vez de `Index`, causando `InvalidOperationException` al buscar permisos por rol.

## 1.1.3 - 2026-06-13

### Added
- Documentado patrón "Tablas maestras con IsActive" en ARCHITECTURE.md y NEW_MODULE_GUIDE.md.

### Changed
- Roles: dropdowns filtran solo activos, validación server-side al asignar, bloqueo al desactivar con dependencias.
- `RoleRepository`: nuevos métodos `GetActiveAsync`, `IsActiveAsync`, `HasActiveDependentsAsync`.
- `UserService` y `PermissionService`: validan que el rol asignado esté activo.
- `RoleService`: bloquea desactivación si hay usuarios o permisos activos asociados.

## 1.1.2 - 2026-06-13

### Changed
- Mapeo de Permission movido de `PermissionsController` a `PermissionService`, consistente con el patrón de User.
- Agregado `AppMapper.Map(PermissionFormViewModel, Permission)` para merge en updates.
- `IPermissionService.CreateAsync`/`UpdateAsync` ahora reciben `PermissionFormViewModel` en vez de `Permission`.

## 1.1.1 - 2026-06-13

### Changed
- `AppMapper` refactorizado: separado en clase pública (`AppMapper`) y clase Mapperly (`AppMapperImpl`) para eliminar falsos positivos del IDE en métodos partial con source generators.

## 1.1.0 - 2026-06-13

### Changed
- Riok.Mapperly 4.1.1 → 4.3.1
- Microsoft.Identity.Web 3.9.3 → 3.15.1 (Web, UI, MicrosoftGraph)
- EF Core packages 8.0.13 → 8.0.28 (Design, InMemory, SqlServer, Tools)

### Fixed
- Guiones reemplazados por guiones bajos en `scripts/smoke-test.sh` (`dotnet new` sanitiza nombres)
- Label `dependencies` eliminado de Dependabot (no existe en el repo)

### Security
- Ignore rule agregada para Serilog.AspNetCore (>= 10) e Identity.Web (>= 4.0.0) en Dependabot

## 1.0.0 - 2026-06-13

### Added
- DemoApp inicial para aplicaciones monolíticas con .NET 8
- Autenticación Azure AD (OpenIdConnect) o Cookie Auth (demo)
- Soporte MySQL / SQL Server / InMemory (configurable con `--dbProvider`)
- Patrón `Result<T>` en servicios con `ExecuteAsync()` centralizado
- `PermissionFilterAttribute` con caché de permisos (IMemoryCache)
- Auditoría automática con `AuditInterceptor` y `AuditableEntity`
- Health checks con `DatabaseHealthCheck`
- Métricas Prometheus con `MetricsAuthMiddleware` (API Key)
- Logging con Serilog (archivo + Grafana Loki)
- Rate limiting (login y formularios)
- CSP con nonce y cabeceras de seguridad
- Sidebar dinámico con permisos por usuario
- Documentación: `docs/ARCHITECTURE.md`, `docs/NEW_MODULE_GUIDE.md`

### Changed
- Seguridad en `/metrics`: deny-by-default cuando `Metrics:ApiKey` no está configurado
- Seguridad en `/health`: response minimalista (solo "Healthy"/"Unhealthy"), sin detalles de BD
- `DatabaseHealthCheck` ahora loguea errores internamente con `ILogger`
- `AsNoTracking()` agregado en todas las consultas de solo lectura de repositorios
- Eliminado símbolo `useSerilog` del demoapp (siempre incluido)
- Limpieza de directivas `#if (useSerilog)` huérfanas en `appsettings.json`

### Fixed
- `MetricsAuthMiddleware` fail-open: ahora retorna 401 si ApiKey está vacío
- `#if (useSerilog)` en `appsettings.json` excluía la configuración de Serilog en proyectos generados
- Parámetro `useSerilog` listado en README pero no definido en `demoapp.json`

### Security
- Advertencia documentada sobre modo Cookie Auth (sin verificación de contraseña)
- Dependabot configurado con ignore rules para mantener política LTS (.NET 8)

### Infrastructure
- Smoke test automático en GitHub Actions (4 combinaciones de parámetros)
- Script `scripts/smoke-test.sh` para validación local
- Excluidos `scripts/` y `.github/workflows/smoke-test.yml` del demoapp generado
