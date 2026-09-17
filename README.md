# CoParenting

App de coparentalidade (calendário partilhado, chat e despesas para pais separados), construída em C#/.NET + React. Ver [Plano.docx](Plano.docx) para o plano de execução completo (validação, MVP, arquitetura, roadmap, riscos e modelo de negócio).

## Estrutura do projeto

Clean Architecture, monólito modular:

```
src/
  CoParenting.Domain          # entidades e regras de negócio puras
  CoParenting.Application     # casos de uso, DTOs, interfaces
  CoParenting.Infrastructure  # EF Core (PostgreSQL), Identity
  CoParenting.Api             # ASP.NET Core Web API + SignalR
tests/
  CoParenting.UnitTests
client/                       # React + TypeScript (Vite)
```

## Pré-requisitos

- .NET SDK 10
- Node.js 22+
- PostgreSQL (local ou container) — connection string em `src/CoParenting.Api/appsettings.json`

## Correr localmente

Backend:

```bash
dotnet build
dotnet run --project src/CoParenting.Api
```

Frontend:

```bash
cd client
npm install
npm run dev
```

## Testes

```bash
dotnet test
```

## Estado atual

Setup técnico inicial (Semana 0 do plano): estrutura da solution, referências entre camadas, DbContext com Identity, cliente React e CI. As entidades do modelo de dados (secção 5 do plano) estão modeladas em `CoParenting.Domain`, mas as migrations EF Core, autenticação e funcionalidades do MVP ainda não foram implementadas — ficam para depois de fechar a fase de validação (secção 2 do plano).
