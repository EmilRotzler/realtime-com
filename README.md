# realtime-com

[![Tests](https://github.com/EmilRotzler/realtime-com/actions/workflows/test.yml/badge.svg)](https://github.com/EmilRotzler/realtime-com/actions/workflows/test.yml)

> **Proof of concept.** This project is intentionally limited in scope. There are no accounts, no database (everything is kept in memory and cleared on restart), and only one chat room.

A real-time chat. Each browser tab picks a display name and chats with every other connected tab. Above the chat, a live bar chart shows how many messages were sent in each of the last 15 minutes, and it is the same for every user.

- `back/`: .NET 10 backend with a SignalR hub at `/hub`
- `front/`: Vue + Vite frontend
- `.github/workflows/`: automated tests that run in GitHub Actions on every pull request

## Setup

```sh
cp .env.example .env
cp back/Api/appsettings.example.json back/Api/appsettings.json
echo VITE_API_URL=http://localhost:5222 > front/.env.local
```

`front/.env.local` (gitignored) tells the Vite dev server where the backend is. Docker and the Playwright tests set `VITE_API_URL` themselves.

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

Then open http://localhost:5173 in two or more tabs. The backend runs on http://localhost:5222. Allowed frontend origins (CORS) are set in `back/Api/appsettings.json`.

## Tests

Backend (xUnit), from the repo root. These cover the per-minute counting and history, plus the hub itself: broadcasts, input checks, what a new connection receives, and the minute-boundary update. Hub tests use a fake clock, so timing is exact.

```sh
dotnet test --solution back/realtime-com.slnx
```

Frontend e2e (Playwright). These open separate browser contexts as different users and check that a message, and the chart's count, reach every tab, and that a tab opened later sees earlier messages. Install Chromium once, then run the tests. The backend and the Vite dev server start automatically.

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
