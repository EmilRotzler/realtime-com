# realtime-com

[![Tests](https://github.com/EmilRotzler/realtime-com/actions/workflows/test.yml/badge.svg)](https://github.com/EmilRotzler/realtime-com/actions/workflows/test.yml)

- `back/`: .NET 10 backend with a SignalR hub at `/hub`
- `front/`: Vue + Vite frontend
- `.github/workflows/`: automated tests that run in GitHub Actions on every pull request

## Setup

```sh
cp .env.example .env
cp back/Api/appsettings.example.json back/Api/appsettings.json
```

## Run with Docker

```sh
docker compose up --build
```

- Frontend: http://localhost:8081
- Backend: http://localhost:5000

Ports and the API URL are set in `.env`.

## Run locally

```sh
cd back/Api && dotnet run
cd front && npm install && npm run dev
```

Allowed frontend origins (CORS) are set in `back/Api/appsettings.json`.

## Tests

Backend (xUnit), from the repo root:

```sh
dotnet test --solution back/realtime-com.slnx
```

Frontend e2e (Playwright). Install Chromium once, then run the tests. The backend and the Vite dev server start automatically.

```sh
cd front
npx playwright install chromium
npm run test:e2e
```

- Open the last HTML report: `npx playwright show-report`
- Run in interactive UI mode: `npx playwright test --ui`

### Automated testing (GitHub Actions)

The project also shows automated testing in CI. [`.github/workflows/test.yml`](.github/workflows/test.yml) runs both test suites on every pull request and on every push to `main`:

- **Backend (xUnit):** runs the backend tests against an in-memory server, including a real SignalR connection to the hub.
- **Frontend e2e (Playwright):** builds the frontend, starts the real backend and a production preview of the frontend, then runs the browser tests in headless Chromium. If the job fails, it uploads the Playwright report, with traces, as a downloadable artifact on the run.

`main` is protected by a branch ruleset: changes go through pull requests, and a pull request can only be merged once both jobs pass. Broken code can't reach `main`.
