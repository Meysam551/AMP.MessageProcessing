# 🧠 Message Processing System — .NET 9 | gRPC | Clean Architecture

> A modular distributed system that processes streaming messages via gRPC, following DDD and Clean Architecture principles.

---

## 📜 Overview

This project implements the **Message Processing Challenge** designed by **AmnPardaz** to evaluate:
- Clean and maintainable architecture  
- Correct use of .NET features  
- Testability and reliability  
- Proper documentation and code clarity  

---

## 🧩 System Architecture

The system is composed of **three main modules**, each running independently and communicating via **gRPC** and **HTTP (REST)**.

### 1️⃣ Management Server (`MPS.MessageProcessing.Management`)
- A simple **ASP.NET Core API** that exposes a single endpoint for health checks.
- Receives system health reports from the distributor every 30 seconds.

**API:**
```
POST /api/module/health
```
**Request:**
```json
{
  "Id": "Temp GUID",
  "SystemTime": "2025-10-05T03:42:00Z",
  "NumberOfConnectedClients": 5
}
```
**Response:**
```json
{
  "IsEnabled": true,
  "NumberOfActiveClients": 3,
  "ExpirationTime": "2025-10-05T03:52:00Z"
}
```

---

### 2️⃣ Message Distributor (`MPS.MessageProcessing.Dispatcher`)
This is the **core module** that:
- Reads random messages from a simulated queue every 200ms.
- Streams them to connected processors via **bi-directional gRPC**.
- Receives processed results from processors.
- Periodically reports system health to the Management API.
- Stores processed results in memory (for now).

**Responsibilities:**
1. Read messages from simulated queue  
2. Stream messages to connected processors  
3. Receive and log processed results  
4. Perform periodic HealthChecks  
5. Disable processing if Management server reports `IsEnabled = false`

---

### 3️⃣ Message Processor (`MPS.MessageProcessing.Processor`)
Each processor instance connects to the Distributor via **gRPC**, identifies itself, and:
- Receives messages to analyze.
- Executes regex-based dynamic rules.
- Sends processed message results back.

**Example of a processed result:**
```json
{
  "Id": 123,
  "Engine": "RegexEngine",
  "MessageLength": 10,
  "IsValid": true
}
```

---

## 🧪 Testing

The solution includes a comprehensive **xUnit test suite** under `MessageProcessing.Tests`.

Run all tests:
```bash
dotnet test
```

### Test Coverage
| Project | Tested Features |
|----------|----------------|
| `Management` | Health API response validation |
| `Dispatcher` | Message flow, result storage, health loop |
| `Processor`  | Regex processing logic |

---

## ⚙️ Setup and Run

### Prerequisites
- .NET 9 SDK
- Visual Studio 2022 or VS Code

### Step-by-step

1. Clone the repository:
   ```bash
   git clone https://github.com/YourUsername/MessageProcessingSystem.git
   cd MessageProcessingSystem
   ```

2. Build the solution:
   ```bash
   dotnet build
   ```

3. Run the Management API:
   ```bash
   dotnet run --project MPS.MessageProcessing.Management
   ```
   Swagger UI → [https://localhost:5001/swagger](https://localhost:5001/swagger)

4. Run the Message Distributor:
   ```bash
   dotnet run --project MPS.MessageProcessing.Dispatcher
   ```

5. Run the Message Processor:
   ```bash
   dotnet run --project MPS.MessageProcessing.Processor
   ```

---

## 🧠 Technical Highlights

- **.NET 9 + gRPC + ASP.NET Core**
- **DDD + CQRS Principles**
- **In-memory simulation of message queues**
- **Dynamic regex-driven message analysis**
- **Periodic health monitoring & auto-disable**
- **xUnit Integration + Unit Tests**
- **Extensible and easily containerized**

---

## 🧰 Solution Structure

```
AMP.MessageProcessing/
├── MPS.Shared/
│   ├── Models/
│   └── Protos/
├── MPS.MessageProcessing.Management/
│   ├── Controllers/
│   └── Program.cs
├── MPS.MessageProcessing.Dispatcher/
│   ├── Services/
│   ├── MessageQueueSimulator.cs
│   └── HealthCheckLoop.cs
├── MPS.MessageProcessing.Processor/
│   ├── ProcessorClient.cs
│   └── ProcessorConfig.cs
├── MessageProcessing.Tests/
│   ├── HealthControllerTests.cs
│   ├── DispatcherIntegrationTests.cs
│   └── ProcessorTests.cs
└── README.md
```

---

## 👨‍💻 Author

**Nikola**  
> Developer • Clean Architecture Enthusiast • gRPC & DDD Fan

---

## 📄 License

MIT License © 2025 — Meysam Agha Ahmadi
