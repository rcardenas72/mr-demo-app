# Ideas para futuras versiones

## Internacionalización (i18n)
Soporte multiidioma con archivos .resx, permitiendo seleccionar idioma
al crear el proyecto (--language es, --language en).

## Permisos batch
Al crear un permiso, mostrar checkboxes para seleccionar varias
operaciones (Consultar, Agregar, Editar, Eliminar) de una sola vez
y crear todos los registros en una transacción.

## Exportar a CSV/Excel
Botón en cada vista de lista (Usuarios, Roles, Menús, etc.) que
descargue los datos visibles con los filtros aplicados, similar al
que ya existe en Auditoría.

## Modo oscuro
Toggle dark/light con CSS variables, respetando prefers-color-scheme.

## Tests unitarios
DemoApp de tests (xUnit + Moq) para Services y Controllers,
generados junto con el proyecto base.

## Impersonación
El administrador puede iniciar sesión como otro usuario para
troubleshooting sin compartir contraseñas.
