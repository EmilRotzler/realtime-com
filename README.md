# realtime-com

- `back/`: .NET 10 backend with a SignalR hub at `/hub`
- `front/`: Vue + Vite frontend

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
