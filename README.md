# CoParenting

App de coparentalidade (calendário partilhado, chat e despesas para pais separados), construída em C#/.NET + React. Ver [Docs/Plano.docx](Docs/Plano.docx) para o plano de execução completo (validação, MVP, arquitetura, roadmap, riscos e modelo de negócio) e [Docs/MANUAL_UTILIZADOR.md](Docs/MANUAL_UTILIZADOR.md) para o manual do utilizador.

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
- Docker (para o Postgres local via `docker-compose`)

## Correr localmente

Base de dados:

```bash
docker compose up -d
dotnet ef database update \
  --project src/CoParenting.Infrastructure/CoParenting.Infrastructure.csproj \
  --startup-project src/CoParenting.Api/CoParenting.Api.csproj
```

Backend:

```bash
dotnet build
dotnet run --project src/CoParenting.Api
```

Frontend:

```bash
cd client
npm install
cp .env.example .env   # ajustar se necessário
npm run dev
```

## Testes

```bash
dotnet test
```

## Configuração de email e login com Google (opcional)

Sem configuração adicional, a app funciona em modo de desenvolvimento: emails de confirmação/convite ficam só nos logs da API, e o botão "Iniciar sessão com o Google" não funciona.

Para emails reais (via [Resend](https://resend.com), tem tier gratuito sem cartão de crédito):

```bash
dotnet user-secrets set "Resend:ApiKey" "re_..." --project src/CoParenting.Api
```

Sem domínio verificado no Resend, só chegam emails ao endereço com que criaste a conta Resend (modo sandbox).

Para login com Google, cria um OAuth Client ID (tipo "Web application") na [Google Cloud Console](https://console.cloud.google.com/) e define:
- `Google:ClientId` em `src/CoParenting.Api/appsettings.json` (não é secreto)
- `VITE_GOOGLE_CLIENT_ID` em `client/.env`

## Estado atual

**Concluído:** autenticação (registo/login com email+password e com Google), confirmação de email, criação de unidade familiar e convite do segundo progenitor por email — primeira funcionalidade do MVP (secção 6 do plano, Semanas 4-5). Um utilizador pode pertencer a mais do que uma família. Ver [Docs/MANUAL_UTILIZADOR.md](Docs/MANUAL_UTILIZADOR.md) para o detalhe funcional.

**A seguir:** calendário partilhado (Semanas 6-7 do plano).
