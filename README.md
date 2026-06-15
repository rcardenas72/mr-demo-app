# MR Demo App

Aplicaci&oacute;n demo de **MR Soluciones Integrales** construida con .NET 8.

Demostraci&oacute;n de sistemas de gesti&oacute;n empresarial a medida:
autenticaci&oacute;n por cookies, CRUD completo, auditor&iacute;a de cambios y
control de acceso por roles.

## Tecnolog&iacute;as

- .NET 8 (ASP.NET Core MVC)
- Bootstrap 5
- Entity Framework Core (InMemory DB)
- Cookie Authentication

## Despliegue

La app est&aacute; dise&ntilde;ada para desplegarse en **Render.com** o **Azure App Service**.

### Render

Conectar el repo en [render.com](https://render.com) y usar:

- **Runtime**: .NET 8
- **Build Command**: `dotnet publish -c Release -o out`
- **Start Command**: `dotnet out/DemoApp.Monolitica.Web.dll`

### Credenciales de prueba

- **Usuario**: `demo`
- **Contrase&ntilde;a**: (vac&iacute;o)
