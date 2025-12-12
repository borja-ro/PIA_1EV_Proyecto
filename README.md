# 🎬 MovieManager

Sistema de gestión de películas con consultas en lenguaje natural usando IA.

## 🏗️ Arquitectura

- **Backend API REST** (port 5000): JWT authentication, dual persistence (Memory/SQLite)
- **Backend MCP Server** (port 5001): Natural language query processing
- **Frontend Console**: Terminal-based interface
- **Frontend Blazor Server**: Web application

## 🚀 Quick Start

### Prerequisites
- .NET 8 SDK
- OpenRouter API key (for MCP server)

### Running the project

1. **Start MCP Server**
```bash
cd Backend/MovieManager.MCP
# Add your OpenRouter API key to appsettings.json
dotnet run
```

## 🔐 API Key Note

The OpenRouter API key is included in `appsettings.json` for evaluation purposes only.
- **Credit limit**: 1€ (current usage: ~0.004€)
- This is intentional to facilitate testing by the instructor
- **Not recommended for production environments**

2. **Run Console App**
```bash
cd Frontend/MovieManager.Console
dotnet run
```

3. **Run Blazor App**
```bash
cd Frontend/MovieManager.Blazor
dotnet run
# Open browser at http://localhost:5281
```

## 🔍 Features

### Dual Routing System
- **RuleRouter**: Regex-based pattern matching (<5ms, $0)
- **LLMRouter**: AI-powered LINQ generation via Claude (~1s, $0.002/query)

### Query Examples
- "películas de Nolan"
- "películas de ciencia ficción con más de 8.5 de rating"
- "películas de 2008 a 2014"

## 🛠️ Stack

- .NET 8
- C# 12
- Blazor Server
- SQLite
- OpenRouter (Claude AI)
- JWT Authentication

## 📁 Project Structure
```
MovieManager/
├── Backend/
│   ├── MovieManager.Core/          # Domain layer
│   ├── MovieManager.Infrastructure/ # Data access
│   ├── MovieManager.API/           # REST API
│   └── MovieManager.MCP/           # MCP Server
├── Frontend/
│   ├── MovieManager.Console/       # CLI app
│   └── MovieManager.Blazor/        # Web app
└── Data/
    └── movies.csv
```

## 📄 License

MIT License