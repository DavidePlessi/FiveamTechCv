# 📄 FiveamTechCv

![.NET](https://img.shields.io/badge/.NET-8.0-512BD4?style=flat&logo=dotnet)
![Vue.js](https://img.shields.io/badge/Vue.js-3.x-4FC08D?style=flat&logo=vue.js)
![Neo4j](https://img.shields.io/badge/Neo4j-Database-008CC1?style=flat&logo=neo4j)
![Status](https://img.shields.io/badge/Status-WIP-orange)

**FiveamTechCv** is a full-stack application designed for CV management. By leveraging a **graph database**, it simplifies the analysis of professional profiles from multiple perspectives, uncovering connections and insights that traditional formats miss.

---

## 🚧 Status: Work in Progress

> **Note:** This project is currently under active development and is **not ready for production**.

### 🚀 Upcoming Features
- [ ] **Work Experience**: Advanced tracking and visualization of career history.
- [ ] **Studies**: Comprehensive education mapping.
- [ ] **Person**: Detailed personal profile management and multi-person management.
- [ ] **AI Integration**: AI-powered data interaction and analysis.

---

## 🛠️ Tech Stack

- **Backend**: .NET 8
- **Frontend**: Vue 3
- **Database**: Neo4j (Graph Database)

---

## 📋 Prerequisites

Before you begin, ensure you have the following installed:

- **[.NET 8 SDK](https://dotnet.microsoft.com/download/dotnet/8.0)**
- **[Node.js](https://nodejs.org/)** (v20 or later)
- **[Neo4j Database](https://neo4j.com/download/)** (Community or Enterprise Edition)

---

## 🚀 Getting Started

### 🔙 Backend Setup

The backend entry point is located in the `FiveamTechCv.Server` project.

1.  **Configure Database**:
    Ensure your Neo4j instance is running. Update the connection strings in `FiveamTechCv.Server/appsettings.json`.

2.  **Run the Server**:
    ```bash
    cd FiveamTechCv.Server
    dotnet run
    ```
    The API will start and be ready to serve requests.

### 🖥️ Frontend Setup

The frontend is a Vue 3 application located in the `fiveamtechcv-web` directory.

1.  **Install Dependencies**:
    ```bash
    cd fiveamtechcv-web
    npm install
    ```

2.  **Start Development Server**:
    ```bash
    npm run dev
    ```
    The application will be accessible at `http://localhost:3000` (or the URL provided in the terminal).

---

## 📂 Project Structure

| Project | Description |
| :--- | :--- |
| **`FiveamTechCv.Server`** | Main startup project. Contains `Program.cs` and configuration. |
| **`FiveamTechCv.Api`** | Defines API controllers and HTTP endpoints. |
| **`FiveamTechCv.Core`** | Core business logic and services. |
| **`FiveamTechCv.Entities`** | Data models used across the application. |
| **`FiveamTechCv.Abstract`** | Interfaces and abstractions. |
| **`fiveamtechcv-web`** | Vue 3 frontend application source code. |

---

## 📝 Notes

- **Dependency Injection**: If you encounter issues, check `FiveamTechCv.Server/Program.cs`. Ensure extensions from `Builder.cs` are correctly called.
- **Swagger**: In development mode, use the Swagger UI to test API endpoints directly.
