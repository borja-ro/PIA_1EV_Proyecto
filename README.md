# 🎬 MovieManager
***Por Borja Ramos para la asignatura PIA***

Sistema de gestión de películas con consultas en lenguaje natural usando IA.

---

## 📦 Instalación y Configuración

### 1️⃣ Requisitos Previos

Antes de empezar, asegúrate de tener instalado:
- **.NET 8 SDK**: [Descargar aquí](https://dotnet.microsoft.com/download/dotnet/8.0)

**Verificar instalación:**
```bash
dotnet --version
# Debe mostrar: 8.0.x o superior
```

---

### 2️⃣ Descargar el Proyecto

**Opción A - Clonar con Git:**
```bash
git clone https://github.com/borja-ro/PIA_1EV_Proyecto.git
cd PIA_1EV_Proyecto
```

**Opción B - Descargar ZIP:**
1. Click en el botón verde **"Code"** arriba
2. Click en **"Download ZIP"**
3. Extraer y abrir la carpeta en terminal

---

### 3️⃣ API Key de OpenRouter

**⚠️ IMPORTANTE:** El proyecto incluye una API key funcional con límite de 1€ para facilitar la evaluación.

**Si quieres usar tu propia key:**
1. Regístrate en: https://openrouter.ai/
2. Obtén tu API key
3. Edita `Backend/MovieManager.MCP/appsettings.json`:
```json
{
  "OpenRouter": {
    "ApiKey": "TU_API_KEY_AQUI",
    "Model": "anthropic/claude-sonnet-4-20250514"
  }
}
```

---

## 🚀 Cómo Ejecutar el Proyecto

Tienes **3 formas** de usar el sistema:

### 🎯 OPCIÓN 1: Aplicación Web (Blazor) - RECOMENDADO

**Paso 1 - Iniciar el servidor MCP:**
```bash
cd Backend/MovieManager.MCP
dotnet run
```
✅ Debe mostrar: `Now listening on: http://localhost:5001`

**Paso 2 - Abrir NUEVA terminal y ejecutar Blazor:**
```bash
cd Frontend/MovieManager.Blazor
dotnet run
```
✅ Debe mostrar una URL como: `http://localhost:5281`

**Paso 3 - Abrir en el navegador:**
```
http://localhost:5281
```

🎉 **¡Ya puedes usar la aplicación web!**

---

### 🖥️ OPCIÓN 2: Aplicación de Consola

**Paso 1 - Iniciar el servidor MCP** (igual que antes):
```bash
cd Backend/MovieManager.MCP
dotnet run
```

**Paso 2 - Abrir NUEVA terminal y ejecutar la app de consola:**
```bash
cd Frontend/MovieManager.Console
dotnet run
```

🎉 **Usa el menú interactivo en la terminal**

---

### 🔌 OPCIÓN 3: Solo API REST (sin MCP)

Si solo quieres probar la API REST sin lenguaje natural:

```bash
cd Backend/MovieManager.API
dotnet run
```

Abre en el navegador:
```
http://localhost:5000/swagger
```

🎉 **Interfaz Swagger para probar endpoints**

---

## 📖 Guía de Uso Rápida

### 1. Cargar Datos de Prueba

**En Blazor (Web):**
1. Click en **"Consultas"** (menú lateral)
2. Click en **"📥 Cargar datos de prueba (5 películas)"**
3. ✅ Debe mostrar: "5 películas cargadas"

**En Consola:**
1. Seleccionar opción **"1. Cargar datos de prueba"**
2. ✅ Debe mostrar: "Datos cargados correctamente"

---

### 2. Hacer Consultas en Lenguaje Natural

Escribe consultas como estas:

#### 📌 Consultas Simples (procesadas por Reglas - rápidas y gratis):
```
películas de 2010
películas de Nolan
películas de acción
películas de ciencia ficción
```

#### 🤖 Consultas Complejas (procesadas por IA - más lentas pero inteligentes):
```
películas de ciencia ficción con más de 8.5 de rating
películas dramáticas de menos de 2 horas
películas de Tarantino con más de 8 de rating
películas dirigidas por Nolan después de 2010
```

---

## 🏗️ Arquitectura del Sistema

```
┌─────────────────────────────────────────────────────┐
│                    FRONTENDS                        │
├─────────────────┬───────────────────┬───────────────┤
│  Blazor Web     │  Console CLI      │  API REST     │
│  (Port 5281)    │  (Terminal)       │  (Port 5000)  │
└────────┬────────┴────────┬──────────┴───────┬───────┘
         │                 │                  │
         └─────────────────┼──────────────────┘
                           ▼
                  ┌─────────────────┐
                  │   MCP SERVER    │
                  │   (Port 5001)   │
                  └────────┬────────┘
                           │
              ┌────────────┴────────────┐
              ▼                         ▼
       ┌─────────────┐          ┌─────────────┐
       │ RuleRouter  │          │ LLMRouter   │
       │ (Regex)     │          │ (Claude AI) │
       │ <5ms - $0   │          │ ~1s - $0.002│
       └─────────────┘          └─────────────┘
```

### 🔍 Sistema de Doble Enrutamiento

1. **RuleRouter**: Detecta patrones simples con expresiones regulares
   - Ventajas: Instantáneo (<5ms), gratuito, 100% determinista
   - Ejemplos: "películas de 2010", "películas de Nolan"

2. **LLMRouter**: Usa Claude AI para consultas complejas
   - Ventajas: Entiende lenguaje natural complejo
   - Ejemplos: "películas dramáticas cortas", "sci-fi con buen rating"
   - Costo: ~$0.002 por consulta

**Optimización de Costes:**
- 70% de consultas → RuleRouter (gratis)
- 30% de consultas → LLMRouter (con IA)
- **Ahorro del 80%** vs usar solo IA

---

## 🛠️ Stack Tecnológico

| Componente | Tecnología |
|-----------|-----------|
| Backend | .NET 8, C# 12 |
| API REST | ASP.NET Core Web API |
| Frontend Web | Blazor Server |
| Frontend CLI | .NET Console App |
| Base de Datos | SQLite + In-Memory |
| IA Provider | OpenRouter (Claude Sonnet 4) |
| Autenticación | JWT (JSON Web Tokens) |
| Arquitectura | Clean Architecture (3 capas) |

---

## 📁 Estructura del Proyecto

```
MovieManager/
├── Backend/
│   ├── MovieManager.Core/          # 🎯 Capa de Dominio (Modelos, Interfaces)
│   ├── MovieManager.Infrastructure/ # 💾 Capa de Datos (Repositorios, BD)
│   ├── MovieManager.API/           # 🔌 API REST con Swagger
│   └── MovieManager.MCP/           # 🤖 Servidor MCP (Lenguaje Natural)
├── Frontend/
│   ├── MovieManager.Console/       # 🖥️  Aplicación de Terminal
│   └── MovieManager.Blazor/        # 🌐 Aplicación Web
├── Data/
│   └── movies.csv                  # 📊 Datos de películas de prueba
├── CLAUDE.md                       # 📚 Documentación técnica detallada
└── README.md                       # 📖 Este archivo
```

---

## 🐛 Solución de Problemas

### ❌ Error: "dotnet: command not found"
**Solución:** Instala .NET 8 SDK desde https://dotnet.microsoft.com/download

### ❌ Error: "El servidor MCP no está disponible"
**Solución:** Asegúrate de que el servidor MCP esté corriendo en otra terminal:
```bash
cd Backend/MovieManager.MCP
dotnet run
```

### ❌ Error: "Could not find a part of the path"
**Solución:** Verifica que estás en la carpeta correcta del proyecto

### ❌ Error en la API key de OpenRouter
**Solución:** La API key incluida tiene límite de 1€. Si se agotó:
1. Crea cuenta en https://openrouter.ai/
2. Obtén tu propia API key
3. Edita `Backend/MovieManager.MCP/appsettings.json`

---

## 📚 Documentación Adicional

- **CLAUDE.md**: Documentación técnica completa, decisiones de arquitectura
- **Swagger UI**: http://localhost:5000/swagger (cuando API REST esté corriendo)

---

## 👨‍💻 Autor

**Borja Ramos Oliva**  


---


## 📄 Licencia

**Uso Académico y Educativo**

Este proyecto fue desarrollado como parte del curso PIA (Programación de Inteligencia Artificial) en la Universidad Carlos III de Madrid.

Permisos:
- ✅ Uso para fines educativos y de aprendizaje
- ✅ Referencia en trabajos académicos (con cita apropiada)
- ✅ Estudio del código fuente

Restricciones:
- ❌ Uso comercial sin permiso explícito del autor
- ❌ Presentación como trabajo propio sin atribución
- ❌ Redistribución sin modificaciones significativas

© 2025 Borja Ramos Oliva - Todos los derechos reservados