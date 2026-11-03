# AGENTS.md

Guidelines for AI assistants (and humans) working on this repository.

## Project

Web application that analyses daily BNB/USDT candles from the public Binance API, with BTC/USDT as the market benchmark. Educational project — results are not investment advice.

- Requirements and data source: `docs/requirements/wymagania-i-zakres.md`
- Statistical methods, formulas and test data: `docs/analysis/metody-statystyczne.md`

Documentation in `docs/` is written in Polish; code, comments, commit messages and the README are in English.

## Layout

- `src/backend/CryptoMarketAnalytics.Api` — ASP.NET Core Web API, .NET 10 (`net10.0`), controllers
- `src/frontend` — Vue 3 + TypeScript + Vite
- `tests/backend/CryptoMarketAnalytics.Api.Tests` — xUnit, integration tests via `WebApplicationFactory`
- `tests/frontend` — Vitest unit tests for the frontend; TypeScript settings in `tests/frontend/tsconfig.json`, other tooling configured in `src/frontend` (`vitest.config.ts`, `eslint.config.ts`), run via its npm scripts
- `CryptoMarketAnalytics.slnx` — .NET solution
- `docs/analysis/notebooks` — Jupyter notebooks (Python 3.12+) for analysis and reference calculations; dependencies pinned in `requirements.txt`, setup in its `README.md`

## Commands

Run the app in two terminals from the repository root:

```bash
# Terminal 1 — backend on http://localhost:5299
dotnet run --project src/backend/CryptoMarketAnalytics.Api

# Terminal 2 — frontend dev server on http://localhost:5173, proxies /api to the backend
npm install --prefix src/frontend
npm run dev --prefix src/frontend
```

Check: `GET http://localhost:5299/api/health` returns `Healthy`. The frontend calls the API with relative URLs (`fetch('/api/...')`).

Build, test and lint:

```bash
dotnet build
dotnet test

cd src/frontend
npm run test:unit   # watch mode; `npx vitest run` for a single run
npm run lint
npm run format
npm run build
```

## Rules

- Statistical calculations must follow the variant defined in `docs/analysis/metody-statystyczne.md` (e.g. n−1 divisor, √365 annualisation, log vs simple returns). Use its test data (chapter 11) in unit tests.
- Notebooks: keep dependency versions pinned in `docs/analysis/notebooks/requirements.txt`; do not commit downloaded market data; run a notebook top to bottom before committing it.
- Never add API keys or other secrets to the repository. The Binance market data API needs no key.
- Keep changes small and focused; one branch = one logical change = one pull request.
- Branch names: `<type>/<short-description>` in English kebab-case, e.g. `feat/binance-klines-client`.
- Commit messages: Conventional Commits, imperative mood, e.g. `feat: add Binance klines client`.
  Types: `feat`, `fix`, `docs`, `test`, `refactor`, `chore`, `ci`.
- All tests go in `tests/`, never next to the application code in `src/`.
- Build, tests and lint must pass before opening a pull request.
- AI output is a draft: the author of a change reviews, understands and tests it.
