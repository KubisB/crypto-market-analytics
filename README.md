# crypto-market-analytics

A web application that fetches cryptocurrency market data from an external API, processes it, runs statistical analysis and presents the results as charts and summaries. Built as part of the **Group Practical Project** (*Grupowy projekt praktyczny*) course.

> **Status:** early stage — project assumptions and the base repository structure are being defined.
> Items marked **[TBD]** have not been decided by the team yet.

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

**Goal:** build an application that retrieves market data from a cryptocurrency exchange API, performs statistical analysis on it and presents the results to the user.

**In scope:**
- fetching market data from an external API **[which API: TBD]**,
- data processing and transformation (cleaning, aggregation, reshaping),
- statistical calculations **[list of methods: TBD]**,
- charts and result presentation in a web interface,
- project and analysis documentation,
- application testing.

> This is an educational project. Results of the analyses are not investment advice.


## 3. Team and responsibilities

| Member | Area | Responsibilities |
|---|---|---|
| [@KubisB](https://github.com/KubisB) | Development | Backend; split with frontend |
| [@maciekk707](https://github.com/maciekk707) | Development | Frontend; split with backend |
| [@elkrix](https://github.com/elkrix) | Documentation and analysis | Project documentation, statistical analysis |
| [@mmichalskii1](https://github.com/mmichalskii1) | Documentation and analysis | Project documentation, statistical analysis |
| K | QA | Test strategy, testing, quality verification |


## 4. Tech stack

| Layer | Technology | Status |
|---|---|---|
| Backend | C# / .NET (version: [TBD]) | **[TBD]** |
| Frontend | Vue.js | **[TBD]** |
| Data source | External cryptocurrency exchange API _[which one: TBD]_ | |
| Data storage | In-memory cache; no database assumed at the start or simply SQLite | **[TBD]** |
| Backend tests | _[e.g. xUnit]_ | **[TBD]** |
| E2E tests | _[TBD]_ | |
| Data analysis | _[e.g. notebooks, scripts, spreadsheets]_ | **[TBD]** |
| Version control | Git | |


## 5. Architecture overview

_[Short description, 3–5 sentences: where the data comes from, where it is processed, how it reaches the user. A simple diagram will be added later.]_

Details and technical decisions will be kept in `docs/architecture/`.


## 6. Repository structure

```
crypto-market-analytics/
├── docs/                          # project and analysis documentation
│   ├── analysis/                  # statistical analysis, results, conclusions
│   ├── architecture/              # architecture, diagrams, technical decisions
│   └── requirements/              # requirements, scope, user stories
├── src/
│   ├── backend/                   # backend application (C# / .NET)
│   └── frontend/                  # frontend application (Vue.js)
├── tests/
│   ├── backend/                   # unit and integration tests
│   └── e2e/                       # end-to-end tests
├── .editorconfig                  # shared code formatting rules
├── .gitignore
├── AGENTS.md                      # guidelines for AI assistants working on the repository
├── CLAUDE.md                      # points Claude Code to AGENTS.md
├── CryptoMarketAnalytics.sln      # .NET solution
└── README.md
```


## 7. Getting started

### Prerequisites

- Git
- .NET SDK _[version]_
- Node.js _[version]_ and npm (frontend)

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
- API keys and other secrets (e.g. for the exchange API) are **never committed to the repository**.
  The required values are listed in `.env.example` _[to be created]_; copy it to `.env` and fill in your own values.
- `.env` and any other secrets are excluded in `.gitignore`.


## 9. Documentation

Project documentation lives in `docs/` and is written in Polish.

| Document | Location |
|---|---|
| Requirements and scope | `docs/requirements/` |
| Architecture | `docs/architecture/` |
| Statistical analysis | `docs/analysis/` |


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

---

_Educational project. Market data and analysis results are for demonstration purposes only._
