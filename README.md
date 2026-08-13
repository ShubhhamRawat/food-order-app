# Food Order Management App

A full-stack food order management feature for a food-delivery application.

## Tech stack

- Frontend: React, Vite, TypeScript
- Backend: ASP.NET Core Web API (.NET 9)
- Database: SQL Server with Entity Framework Core
- Testing: xUnit with EF Core InMemory provider

## Features

- Browse a database-backed food menu
- Add items to a cart and change quantities
- Submit delivery details at checkout
- Server-side total calculation
- Create and retrieve orders
- Update order status: Received, Preparing, OutForDelivery, Delivered
- Order status polling every 5 seconds
- API request validation
- Automated controller test

## Project structure

```text
backend/   ASP.NET Core API and SQL Server migrations
frontend/  React user interface

```

## Run locally

### Backend

1. Open `backend/appsettings.Development.json`.
2. Add your local SQL Server connection string:

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=localhost;Database=FoodOrderDb;Trusted_Connection=True;TrustServerCertificate=True;"
  }
}
```

3. Run:

```powershell
cd backend
dotnet ef database update
dotnet run --urls http://localhost:5000
```

API: `http://localhost:5000`

### Frontend

```powershell
cd frontend
npm install
npm run dev
```

Frontend: `http://localhost:5173`

## API endpoints

| Method | Endpoint | Purpose |
| --- | --- | --- |
| GET | `/api/menu` | Get all menu items |
| GET | `/api/orders` | Get all orders |
| GET | `/api/orders/{id}` | Get one order |
| POST | `/api/orders` | Create an order |
| PATCH | `/api/orders/{id}/status` | Update order status |

## Design decisions

- The client sends menu IDs and quantities only.
- The backend fetches authoritative menu prices and calculates the total.
- `OrderItem.UnitPrice` preserves the purchase price even when a menu price changes.
- Request DTOs prevent clients from setting server-controlled fields.
- React uses polling to simulate real-time status updates; SignalR would be a production upgrade.