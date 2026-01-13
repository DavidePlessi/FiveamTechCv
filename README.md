# FiveamTechCv

This repository contains the source code for FiveamTechCv, a full-stack application built with a **.NET 8** backend and a **Vue 3** frontend.

The project is designed to manage the CV using a graph database to simplify analysis from multiple perspectives.

## Prerequisites

Before starting, ensure you have the following installed:

- **[.NET 8 SDK](https://dotnet.microsoft.com/download/dotnet/8.0)**
- **[Node.js](https://nodejs.org/)** (v20 or later)
- **[Neo4j Database](https://neo4j.com/download/)** (Community or Enterprise Edition)

## Getting Started

### Backend

The backend entry point is located in the `FiveamTechCv.Server` project.

1.  **Configure Database**: Ensure your Neo4j instance is running and updated connection strings are present in `FiveamTechCv.Server/appsettings.json`.
2.  **Navigate to the Server project**:
    ```bash
    cd FiveamTechCv.Server
    ```
3.  **Run the application**:
    ```bash
    dotnet run
    ```
    The API will start and serve requests.

### Frontend

The frontend is a Vue 3 application located in the `fiveamtechcv-web` directory.

1.  **Navigate to the web project**:
    ```bash
    cd fiveamtechcv-web
    ```
2.  **Install dependencies**:
    ```bash
    npm install
    ```
3.  **Start the development server**:
    ```bash
    npm run dev
    ```
    The application will be accessible at the URL provided in the terminal (typically `http://localhost:3000`).

## Project Structure

- **`FiveamTechCv.Server`**: The main startup project for the backend application. Contains `Program.cs` and configuration.
- **`FiveamTechCv.Api`**: Defines API controllers and HTTP endpoints.
- **`FiveamTechCv.Core`**: Contains core business logic and services.
- **`FiveamTechCv.Entities`**: Defines the data models used across the application.
- **`FiveamTechCv.Abstract`**: Contains interfaces and abstractions.
- **`fiveamtechcv-web`**: The Vue 3 frontend application source code.

## Notes

- **Dependency Injection**: If you encounter issues with controllers or services, check `FiveamTechCv.Server/Program.cs`. Ensure that extensions from `Builder.cs` are correctly called to register all necessary dependencies.
- **Swagger**: If enabled in development, you can access the Swagger UI to test API endpoints directly after starting the backend.
