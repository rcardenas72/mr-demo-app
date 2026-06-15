# Despliegue en Azure App Service

## Requisitos
- Cuenta Azure activa (Free tier incluido)
- [Azure CLI](https://aka.ms/azure-cli) instalado en tu PC

## 1. Crear App Service (Azure Portal o CLI)

### Opci\u00f3n A: Azure Portal
1. Ve a [portal.azure.com](https://portal.azure.com) \u2192 **App Services** \u2192 **Crear**
2. Datos:
   - **Suscripci\u00f3n**: la tuya
   - **Grupo de recursos**: `rg-demo-mr` (o el que prefieras)
   - **Nombre**: `demo-mr-app` (ser\u00e1 `demo-mr-app.azurewebsites.net`)
   - **Publicar**: **C\u00f3digo**
   - **Runtime stack**: **.NET 8 (LTS)**
   - **Sistema operativo**: **Windows** (m\u00e1s f\u00e1cil para deploy ZIP)
   - **Regi\u00f3n**: **East US** (o la m\u00e1s cercana)
   - **Plan**: **Free F1** (gratis)
3. Revisar + Crear

### Opci\u00f3n B: Azure CLI
```bash
az group create --name rg-demo-mr --location eastus
az appservice plan create --name plan-demo-mr --resource-group rg-demo-mr --sku F1 --is-linux false
az webapp create --name demo-mr-app --resource-group rg-demo-mr --plan plan-demo-mr --runtime "DOTNET|8.0"
```

## 2. Desplegar (ZIP Deploy)

### Opci\u00f3n A: Azure CLI (r\u00e1pido)
```bash
az webapp deployment source config-zip --resource-group rg-demo-mr --name demo-mr-app --src "C:\Sistemas\DemoApp\publish.zip"
```

### Opci\u00f3n B: ZIP manual desde Portal
1. Ve a `C:\Sistemas\DemoApp\publish\`
2. Selecciona **todo el contenido** (Ctrl+E) \u2192 Enviar a \u2192 **Carpeta comprimida**
3. Nombra el ZIP (ej. `demoapp.zip`)
4. En Azure Portal: App Service \u2192 **demo-mr-app** \u2192 **Centro de implementaci\u00f3n** \u2192 **ZIP Deploy**
5. Sube el archivo y haz clic en **Desplegar**

## 3. Verificar
- Abre `https://demo-mr-app.azurewebsites.net`
- Inicia sesi\u00f3n con: **demo** / (sin contrase\u00f1a)

## Notas
- Tier **Free F1**: 60 min CPU/d\u00eda, se duerme por inactividad, sin dominio personalizado
- Si quieres `demo.mrsolucionesint.com` en el futuro, cambia a plan **B1** (~$13/mes)
- Los datos InMemory se pierden al reiniciar el servidor (normal en demo)
