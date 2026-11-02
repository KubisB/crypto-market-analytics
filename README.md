# crypto-market-analytics

A web application that fetches daily BNB/USDT market data from the public Binance API, runs statistical analysis on it and presents the results as charts and summaries, using bitcoin (BTC/USDT) as the market benchmark. Built as part of the **Group Practical Project** (*Grupowy projekt praktyczny*) course.

> **Status:** early stage — requirements and analysis methods are documented; the application code has not been started yet.
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
| Backend | C# / .NET (version: [TBD]) | **[TBD]** |
| Frontend | Vue.js | Decided |
| Data source | Binance public REST API (`https://data-api.binance.vision`, `GET /api/v3/klines`), daily candles, no API key | Decided |
| Data storage | Local storage of downloaded candles; form (e.g. SQLite or files) _[TBD]_ | **[TBD]** |
| Backend tests | _[e.g. xUnit]_ | **[TBD]** |
| E2E tests | _[TBD]_ | **[TBD]** |
| Data analysis and verification | Python, Jupyter notebooks (pandas, numpy, scipy, matplotlib) | Decided |
| Version control | Git | |


## 5. Architecture overview

Daily candles for `BNBUSDT` and `BTCUSDT` are downloaded from the public Binance API once a day, after the daily candle closes at midnight UTC. Only closed candles are kept; they are validated and stored locally, so the application can still show the last saved data when the API is unavailable. Returns and statistics are calculated from the stored data and presented in the Vue.js interface as charts and tables. Results are cross-checked against reference calculations in Python. Where the calculations run (backend or frontend) **[TBD]**; a diagram will be added later.

Details and technical decisions will be kept in `docs/architecture/` _(planned)_.


## 6. Repository structure

Target structure. Entries marked _(planned)_ will be added together with the application skeleton.

```
crypto-market-analytics/
├── docs/                          # project and analysis documentation
│   ├── analysis/                  # statistical methods, pair selection, results
│   │   └── notebooks/             # Jupyter notebooks for analysis and verification (planned)
│   ├── architecture/              # architecture, diagrams, technical decisions (planned)
│   └── requirements/              # requirements, scope, user stories
├── src/                           # (planned)
│   ├── backend/                   # backend application (C# / .NET)
│   └── frontend/                  # frontend application (Vue.js)
├── tests/                         # (planned)
│   ├── backend/                   # unit and integration tests
│   └── e2e/                       # end-to-end tests
├── .editorconfig                  # shared code formatting rules (planned)
├── .gitignore
├── AGENTS.md                      # guidelines for AI assistants working on the repository (planned)
├── CLAUDE.md                      # points Claude Code to AGENTS.md (planned)
├── CryptoMarketAnalytics.sln      # .NET solution (planned)
└── README.md
```


## 7. Getting started

### Prerequisites

- Git
- .NET SDK _[version]_
- Node.js _[version]_ and npm (frontend)
- Python 3 with Jupyter, pandas, numpy, scipy and matplotlib (analysis notebooks only)

### Steps

```bash
# 1. Clone the repository
git clone <repository-url>
cd crypto-market-analytics

# 2. Backend
dotnet restore
dotnet build
dotnet run --project src/backend/<project-name>

# 3. Frontend
cd src/frontend
npm install
npm run dev

# 4. Backend tests
dotnet test
```

_[Commands to be verified and completed once the first code is in place.]_


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
and documentation process. Shared instructions for AI tools will live in
`AGENTS.md` _(planned)_.

Ground rules:
- AI output is a draft. The author of a change is responsible for reviewing,
  understanding and testing it before opening a pull request.
- Statistical results and conclusions are verified by a human against the
  source data before they go into the documentation.
- No secrets (API keys, `.env` contents) are ever pasted into AI tools.
- Code review applies equally to AI-assisted and hand-written changes.

---

_Educational project. Market data and analysis results are for demonstration purposes only._
