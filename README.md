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

- Node.js >= 18
- .NET SDK >= 10.0
- Git

## Getting Started

### 1. Clone the repository

```bash
git clone https://github.com/mussesseiniris/WorkerDataBlockchain.git
cd worker-data-blockchain
```

### 2. Frontend

```bash
cd wdb-frontend
npm install
npm run dev
```

Open http://localhost:3000 in your browser.

### 3. Backend

```bash
cd wdb-backend
dotnet restore
dotnet run
```
Before running, create `appsettings.Development.json` by copying the connection string from `appsettings.json` and replacing `[YOUR-PASSWORD]` with the real database password. The password is in the slack chatbox. This file is gitignored and should not be committed.

Then run:

```bash
dotnet run
```

API runs at http://localhost:5258

Swagger UI is available at http://localhost:5258/swagger (development only)

### 4. Blockchain

## Testing

### 1. Frontend

```bash
cd wdb-frontend
npm test
```
### 2. Backend

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


