# crypto-market-analytics

A web application that fetches daily BNB/USDT market data from the public Binance API, runs statistical analysis on it and presents the results as charts and summaries, using bitcoin (BTC/USDT) as the market benchmark. Built as part of the **Group Practical Project** (*Grupowy projekt praktyczny*) course.

> **Status:** early stage — requirements and analysis methods are documented; the application skeleton (backend, frontend, tests) is in place, features are not implemented yet.
> Items marked **[TBD]** have not been decided by the team yet. Items marked _(planned)_ do not exist in the repository yet.

---

## Table of contents

1. [Project context](#1-project-context)
2. [Goal and scope](#2-goal-and-scope)
3. [Team and responsibilities](#3-team-and-responsibilities)
4. [Tech stack](#4-tech-stack)
5. [Architecture overview](#5-architecture-overview)
6. [Repository structure](#6-repository-structure)
7. [Getting started](#7-getting-started)
8. [Configuration](#8-configuration)
9. [Documentation](#9-documentation)
10. [AI usage policy](#10-ai-usage-policy)
11. [Contributing](#11-contributing)

---

## 1. Project context

The project is carried out at the request of the course supervisor and is the basis for passing the course.


## 2. Goal and scope

**Goal:** build an application that retrieves daily BNB/USDT candles from Binance, analyses how BNB behaves on its own and against the market (BTC/USDT), and presents the results to the user.

**In scope:**
- fetching daily OHLCV candles for `BNBUSDT` and `BTCUSDT` from the public Binance REST API (no account or API key) and storing them locally,
- data validation and calculation of daily returns,
- statistical analysis: descriptive statistics, volatility, drawdowns, return distribution, correlation and beta against BTC, event study around Binance-related events and volume analysis — see [`docs/analysis/metody-statystyczne.md`](docs/analysis/metody-statystyczne.md),
- charts and tables in a Vue.js web interface with a selectable date range (from 1 January 2023 to yesterday),
- project and analysis documentation,
- application testing.

**Out of scope:** trading, price forecasting and investment recommendations, real-time data, user accounts, exchanges other than Binance. Full scope: [`docs/requirements/wymagania-i-zakres.md`](docs/requirements/wymagania-i-zakres.md).

> This is an educational project. Results of the analyses are not investment advice.

## 3. Team and responsibilities

| Member | Area | Responsibilities |
|---|---|---|
| [@KubisB](https://github.com/KubisB) | Development | Backend; split with frontend |
| [@maciekk707](https://github.com/maciekk707) | Development | Frontend; split with backend |
| [@elkrix](https://github.com/elkrix) | Documentation and analysis | Project documentation, statistical analysis |
| [@mmichalskii1](https://github.com/mmichalskii1) | Documentation and analysis | Project documentation, statistical analysis |
| [@KusmierekKuba](https://github.com/KusmierekKuba) | QA | Test strategy, testing, quality verification |


## 4. Tech stack

| Layer | Technology | Status |
|---|---|---|
| Backend | C# / ASP.NET Core Web API (controllers, OpenAPI) on .NET 10 LTS (`net10.0`) | Decided |
| Frontend | Vue 3 + TypeScript, Vite; ESLint, Oxlint and Prettier | Decided |
| Data source | Binance public REST API (`https://data-api.binance.vision`, `GET /api/v3/klines`), daily candles, no API key | Decided |
| Data storage | Local storage of downloaded candles; form (e.g. SQLite or files) _[TBD]_ | **[TBD]** |
| Backend tests | xUnit, integration tests with `WebApplicationFactory` | Decided |
| Frontend unit tests | Vitest, Vue Test Utils | Decided |
| E2E tests | _[TBD]_ | **[TBD]** |
| Data analysis and verification | Python, Jupyter notebooks (pandas, numpy, scipy, matplotlib) | Decided |
| Version control | Git | |


## 5. Architecture overview

Daily candles for `BNBUSDT` and `BTCUSDT` are downloaded from the public Binance API once a day, after the daily candle closes at midnight UTC. Only closed candles are kept; they are validated and stored locally, so the application can still show the last saved data when the API is unavailable. Returns and statistics are calculated from the stored data and presented in the Vue.js interface as charts and tables. Results are cross-checked against reference calculations in Python. Where the calculations run (backend or frontend) **[TBD]**; a diagram will be added later.

Details and technical decisions will be kept in `docs/architecture/` _(planned)_.


## 6. Repository structure

Entries marked _(planned)_ do not exist yet.

```
crypto-market-analytics/
├── .vscode/
│   ├── extensions.json                    # recommended VS Code extensions
│   └── settings.json                      # shared VS Code settings (format on save, ESLint for src/frontend)
├── docs/                                  # project and analysis documentation (Polish)
│   ├── analysis/                          # statistical methods, pair selection, results
│   │   └── notebooks/                     # Jupyter notebooks for analysis and verification (planned)
│   ├── architecture/                      # architecture, diagrams, technical decisions (planned)
│   └── requirements/                      # requirements, scope, user stories
├── src/
│   ├── backend/
│   │   └── CryptoMarketAnalytics.Api/     # ASP.NET Core Web API (.NET 10)
│   └── frontend/                          # Vue 3 + TypeScript + Vite application
├── tests/
│   ├── backend/
│   │   └── CryptoMarketAnalytics.Api.Tests/  # xUnit unit and integration tests
│   └── e2e/                               # end-to-end tests (planned)
├── .editorconfig                          # shared code formatting rules (C# and frontend)
├── .gitattributes                         # LF line endings on every OS
├── .gitignore                             # single ignore file for the whole repository
├── AGENTS.md                              # guidelines for AI assistants working on the repository
├── CLAUDE.md                              # points Claude Code to AGENTS.md
├── CryptoMarketAnalytics.slnx             # .NET solution (new XML solution format)
├── global.json                            # pins .NET SDK 10 (newer 10.x feature bands allowed)
└── README.md
```

Frontend unit tests live next to the code, in `src/frontend/src/**/__tests__/`.


## 7. Getting started

### Prerequisites

- Git
- [.NET 10 SDK](https://dotnet.microsoft.com/download/dotnet/10.0) (LTS), e.g. `winget install Microsoft.DotNet.SDK.10`
- [Node.js](https://nodejs.org/) `^22.18.0` or `>=24.12.0` (the project was set up with Node 26 LTS) and npm
- Python 3 with Jupyter, pandas, numpy, scipy and matplotlib (analysis notebooks only)
- Editor: any. Recommended: VS Code with the extensions from `.vscode/extensions.json` (C# Dev Kit, Vue - Official, ESLint, Prettier). Visual Studio 2022 does not support .NET 10 — use Visual Studio 2026 or JetBrains Rider instead.

### Steps

```bash
# 1. Clone the repository
git clone https://github.com/KubisB/crypto-market-analytics.git
cd crypto-market-analytics

# 2. Backend: build, test and run (http://localhost:5299)
dotnet build
dotnet test
dotnet run --project src/backend/CryptoMarketAnalytics.Api

# 3. Frontend (in a second terminal): install and run the dev server (http://localhost:5173)
cd src/frontend
npm install
npm run dev
```

Check that the backend is running: `GET http://localhost:5299/api/health` returns `Healthy`. In development the Vite dev server forwards every `/api/*` request to the backend, so the frontend calls the API with relative URLs (e.g. `fetch('/api/health')`). The OpenAPI document is available at `http://localhost:5299/openapi/v1.json` in the Development environment.

### Useful commands

| Where | Command | What it does |
|---|---|---|
| root | `dotnet build` | build the solution |
| root | `dotnet test` | run backend tests |
| `src/frontend` | `npm run dev` | dev server with hot reload |
| `src/frontend` | `npm run test:unit` | run Vitest in watch mode (`npx vitest run` for a single run) |
| `src/frontend` | `npm run lint` | Oxlint and ESLint with auto-fix |
| `src/frontend` | `npm run format` | format `src/` with Prettier |
| `src/frontend` | `npm run build` | type-check and production build to `dist/` |

### How the skeleton was created

For reference, the project was generated with these commands (.NET SDK 10.0.403, create-vue 3.24.0):

```bash
dotnet new sln -n CryptoMarketAnalytics
dotnet new webapi -n CryptoMarketAnalytics.Api -o src/backend/CryptoMarketAnalytics.Api --use-controllers -f net10.0
dotnet new xunit -n CryptoMarketAnalytics.Api.Tests -o tests/backend/CryptoMarketAnalytics.Api.Tests -f net10.0
dotnet sln CryptoMarketAnalytics.slnx add src/backend/CryptoMarketAnalytics.Api/CryptoMarketAnalytics.Api.csproj --solution-folder src
dotnet sln CryptoMarketAnalytics.slnx add tests/backend/CryptoMarketAnalytics.Api.Tests/CryptoMarketAnalytics.Api.Tests.csproj --solution-folder tests
dotnet add tests/backend/CryptoMarketAnalytics.Api.Tests reference src/backend/CryptoMarketAnalytics.Api
dotnet add tests/backend/CryptoMarketAnalytics.Api.Tests package Microsoft.AspNetCore.Mvc.Testing
dotnet new gitignore
dotnet new editorconfig

cd src
npm create vue@latest -- --ts --vitest --eslint --prettier --bare frontend
```

Afterwards:
- the template samples (`WeatherForecast`, `UnitTest1`) were replaced with a `/api/health` endpoint and its integration test,
- a dev proxy for `/api` was added to `vite.config.ts`,
- the frontend's own `.gitignore`, `.gitattributes`, `.editorconfig` and `.vscode/` were merged into the root files, and the generated `.gitignore` was trimmed to the tools the project actually uses,
- `.gitattributes` (LF everywhere) and `global.json` (.NET SDK 10) were added.


## 8. Configuration

- Application settings: `appsettings.json` and `appsettings.Development.json`.
- The Binance market data API is public, so **no API key is needed** to run the application at the start.
- If secrets are added later, they are **never committed to the repository**.
  The required values will be listed in `.env.example` _(planned)_; copy it to `.env` and fill in your own values.
- `.env` and any other secrets are excluded in `.gitignore`.


## 9. Documentation

Project documentation lives in `docs/` and is written in Polish.

| Document | Location |
|---|---|
| Requirements and scope, data source (Binance API) | [`docs/requirements/wymagania-i-zakres.md`](docs/requirements/wymagania-i-zakres.md) |
| Statistical methods, formulas and test data | [`docs/analysis/metody-statystyczne.md`](docs/analysis/metody-statystyczne.md) |
| Choice of trading pairs | [`docs/analysis/wybor-par-walutowych.md`](docs/analysis/wybor-par-walutowych.md) |
| Architecture | `docs/architecture/` _(planned)_ |


## 10. AI usage policy

The team uses AI assistants (e.g. Claude, Copilot) as part of the development
and documentation process. Shared instructions for AI tools live in
[`AGENTS.md`](AGENTS.md).

Ground rules:
- AI output is a draft. The author of a change is responsible for reviewing,
  understanding and testing it before opening a pull request.
- Statistical results and conclusions are verified by a human against the
  source data before they go into the documentation.
- No secrets (API keys, `.env` contents) are ever pasted into AI tools.
- Code review applies equally to AI-assisted and hand-written changes.


## 11. Contributing

- Every change goes through a pull request; nobody pushes directly to `main`.
- One branch = one logical change = one pull request. Keep pull requests small.
- **Branch names:** `<type>/<short-description>`, in English, lowercase, words separated by hyphens, describing *what* changes (not *where*). Examples: `feat/binance-klines-client`, `fix/drawdown-start-date`, `docs/event-study-method`.
- **Commit messages:** [Conventional Commits](https://www.conventionalcommits.org/), imperative mood, e.g. `feat: add Binance klines client`, `fix: exclude unclosed candle`.
- **Types** (for both branches and commits): `feat` (new functionality), `fix` (bug fix), `docs` (documentation), `test` (tests only), `refactor` (no behaviour change), `chore` (setup, dependencies, configuration), `ci` (pipelines).
- Before opening a pull request: `dotnet build`, `dotnet test`, and in `src/frontend` `npm run lint`, `npx vitest run` and `npm run build` must pass.
- Code, comments, commit messages and this README are in English; documentation in `docs/` is in Polish.

---

_Educational project. Market data and analysis results are for demonstration purposes only._
