# WorkDataBlockchain

A blockchain-based proof-of-concept platform that gives workers ownership and control over their personal and professional data. Employers must submit access requests to view worker data, and all authorization events are recorded on the blockchain as an immutable audit trail.

## Tech Stack

**Frontend:** React, Next.js, TypeScript, Tailwind CSS

**Backend:** ASP.NET Core (C#), PostgreSQL (hosted on Supabase), Supabase Auth

**Blockchain:** 

## Project Structure

```
worker-data-blockchain/
  wdb-frontend/       ← Next.js frontend
  wdb-backend/        ← ASP.NET Core backend
  wdb-blockchain/      ← to be initialized
```

## Prerequisites

To run the application:
- [Docker Desktop](https://www.docker.com/products/docker-desktop/)

To run tests locally:
- .NET SDK >= 10.0
- Node.js >= 18

## Getting Started

### 1. Clone the repository

```bash
git clone https://github.com/mussesseiniris/WorkerDataBlockchain.git
cd worker-data-blockchain
```

### 2. Set up environment variables

Create a `.env` file in the root directory:

```bash
cp .env.example .env
```

Open `.env` and fill in the database password (find it in the Slack chatbox):

```
CONNECTION_STRING=Host=aws-1-ap-northeast-2.pooler.supabase.com;Database=postgres;Username=postgres.kkrkhfzsoudhgjnyxxuv;Password=YOUR-PASSWORD;SSL Mode=Require;Trust Server Certificate=true
```

### 3. Start the application

```bash
docker compose up
```

- Frontend: http://localhost:3000
- Backend: http://localhost:5258
- Swagger UI: http://localhost:5258/swagger (development only)

#### Useful Docker commands

| Command | Description |
|---|---|
| `docker compose up` | Start all services |
| `docker compose up --build` | Rebuild and start (use this after pulling new changes) |
| `docker compose down` | Stop and remove containers |

## Testing

Tests run locally and do not require Docker or a database connection.

### 1. Install test dependencies

**Mac** (requires [Homebrew](https://brew.sh)):

```bash
brew install node
brew install --cask dotnet-sdk
```

**Windows** (winget is built into Windows 10/11):

```bash
winget install OpenJS.NodeJS
winget install Microsoft.DotNet.SDK.10
```

### 2. Run tests

**Frontend:**

```bash
cd wdb-frontend
npm test
```

**Backend:**

```bash
cd worker-data-blockchain
dotnet test
```

## CI

GitHub Actions automatically runs backend and frontend tests when code is pushed to `main` or a pull request is opened against `main`.

## Code Style

This project uses Prettier and EditorConfig to enforce consistent formatting. Your IDE will pick up the rules automatically.

- Indent: 2 spaces (4 spaces for C#)
- Quotes: single quotes (frontend)
- Semicolons: yes

**Required VS Code extensions:**

- Prettier - Code formatter (enable `Format On Save`)
- EditorConfig for VS Code
- ES7+ React/Redux/React-Native snippets
- Tailwind CSS IntelliSense
