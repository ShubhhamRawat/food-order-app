# Food Order Management System

A full-stack, enterprise-grade food ordering and real-time tracking application built with **ASP.NET Core (.NET 9)**, **Entity Framework Core**, **SignalR**, and **React**.

[![.NET 9](https://img.shields.io/badge/.NET-9.0-purple.svg)](https://dotnet.microsoft.com/)
[![React](https://img.shields.io/badge/React-18-blue.svg)](https://reactjs.org/)
[![SignalR](https://img.shields.io/badge/Real--Time-SignalR%20WebSockets-green.svg)](https://learn.microsoft.com/en-us/aspnet/core/signalr/)
[![Tests](https://img.shields.io/badge/xUnit-12%20Passed-brightgreen.svg)]()

---

## Architecture Overview

This project implements a **Clean Layered Architecture** adhering to SOLID principles:

```
[ React Client (Vite + TypeScript) ]
            │ HTTP (REST)
            ▼
 ┌──────────────────────┐
 │   OrdersController   │  <-- Thin Controller (HTTP Request Mapping & Status Codes)
 └──────────┬───────────┘
            │ Injected via DI (Scoped)
            ▼
 ┌──────────────────────┐
 │     OrderService     │  <-- Business Logic Layer (Validation, Price Calc, EF Core)
 └─────┬──────────┬─────┘
       │          │
Queries│          │ Broadcasts Status via WebSockets
       ▼          ▼
┌────────────┐  ┌─────────────────────────┐
│ AppDbContext│  │    OrderHub (SignalR)   │
│ (EF Core)  │  │ (Group-based WebSockets)│
└────────────┘  └─────────────────────────┘
```

- **Separation of Concerns:** Controllers handle strictly HTTP status codes (`CreatedAtAction`, `Ok`, `BadRequest`, `NotFound`). Database transactions, price aggregations, and business logic reside entirely within `IOrderService`.
- **Real-Time Communication:** SignalR WebSockets replace HTTP polling. Clients subscribe to order-specific groups (`order-{id}`) and receive push updates when status transitions occur (`Received` -> `Preparing` -> `OutForDelivery` -> `Delivered`).
- **Resilient Data Layer:** Entity Framework Core configured for both SQLite (zero-config cloud & local) and SQL Server. Database migrations auto-seed on application startup.
- **Automated Testing:** 12 xUnit unit tests verifying service business logic, SignalR group dispatch via `Moq`, and controller HTTP contracts.

---

## Tech Stack

- **Backend:** ASP.NET Core Web API (.NET 9)
- **Architecture:** Layered Architecture / Service Layer (`IOrderService`)
- **Real-Time Engine:** Microsoft SignalR (WebSockets with automatic reconnection)
- **Data Access:** Entity Framework Core 9 (LINQ, `AsNoTracking`, eager loading)
- **Database:** SQLite (default / cloud) & Microsoft SQL Server
- **Testing:** xUnit, Moq, EF Core InMemory Provider
- **Frontend:** React 18, TypeScript, Vite, `@microsoft/signalr`
- **Deployment:** Docker, Render (Backend API), Vercel (Frontend SPA)

---

## API Endpoints

| Method | Endpoint | Description |
| :--- | :--- | :--- |
| `GET` | `/` | Health check endpoint |
| `GET` | `/api/menu` | Retrieve all available menu items |
| `GET` | `/api/orders` | Retrieve order history (descending by timestamp) |
| `GET` | `/api/orders/{id}` | Retrieve order details with line items |
| `POST` | `/api/orders` | Create a new order with server-side price calculation |
| `PATCH` | `/api/orders/{id}/status` | Update status & trigger real-time SignalR broadcast |
| `WS` | `/hubs/orders` | SignalR WebSocket Hub for real-time tracking |

---

## Running Locally

### Prerequisites
- [.NET 9 SDK](https://dotnet.microsoft.com/download/dotnet/9.0)
- [Node.js 18+](https://nodejs.org/)

### 1. Backend

```bash
cd backend
dotnet restore
dotnet run --urls "http://localhost:5000"
```
The API starts at `http://localhost:5000` (SQLite database initializes and seeds automatically).

### 2. Run Tests

```bash
dotnet test tests/FoodOrderApi.Tests/FoodOrderApi.Tests.csproj
```

### 3. Frontend

```bash
cd frontend
npm install
npm run dev
```
Open `http://localhost:5173` in your browser.

---

## Cloud Deployment

### Backend (Render / Docker)
1. Fork or push to GitHub.
2. Sign in to [Render.com](https://render.com) and click **New > Web Service** (or use Blueprint with `render.yaml`).
3. Set:
   - **Root Directory:** `backend`
   - **Runtime:** `Docker`
   - **Instance Type:** `Free`
4. Copy your live Render URL (e.g., `https://food-order-api.onrender.com`).

### Frontend (Vercel)
1. In your Vercel project dashboard, go to **Settings > Environment Variables**.
2. Add:
   - **Key:** `VITE_API_BASE_URL`
   - **Value:** `https://your-backend-api.onrender.com`
3. Trigger a redeploy.
