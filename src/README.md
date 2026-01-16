# FiveamTechCv

This repository contains the source code for FiveamTechCv, a full-stack application built with a **.NET 8** backend and a **Vue 3** frontend.

The project is designed to manage a professional CV using a graph database (Neo4j) to simplify analysis from multiple perspectives, and leverages AI for intelligent querying.

## Features

- **Graph Database**: Uses Neo4j to model relationships between people, companies, projects, and skills.
- **GraphQL API**: Powered by HotChocolate, providing a flexible query interface for the frontend.
- **AI Integration**: Integrates with Google Gemini for AI-powered CV analysis and Q&A.
- **Vector Search**: Implements vector search for semantic querying of CV content.
- **Authentication**: JWT-based authentication.
- **Frontend**: Built with Vue 3, Vuetify, and Pinia.

## Prerequisites

Before starting, ensure you have the following installed:

- **[.NET 8 SDK](https://dotnet.microsoft.com/download/dotnet/8.0)**
- **[Node.js](https://nodejs.org/)** (v20 or later)
- **[Neo4j Database](https://neo4j.com/download/)** (Community or Enterprise Edition)

## Getting Started

### Backend

The backend entry point is located in the `FiveamTechCv.Server` project.

1.  **Configure Settings**: 
    - Ensure your Neo4j instance is running.
    - Update connection strings in `FiveamTechCv.Server/appsettings.json`.
    - Add your Google Gemini API key to the `Gemini` section in `appsettings.json`.
    - Configure JWT settings if necessary.

2.  **Navigate to the Server project**:
    ```bash
    cd FiveamTechCv.Server
    ```
3.  **Run the application**:
    ```bash
    dotnet run
    ```
    The API will start and serve requests.
    - Swagger UI: `https://localhost:7178/swagger` (or similar port)
    - GraphQL Banana Cake Pop: `https://localhost:7178/graphql`

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

- **`FiveamTechCv.Server`**: The main startup project. Configures DI, Middleware, GraphQL, and Authentication.
- **`FiveamTechCv.Api`**: Defines REST API controllers.
- **`FiveamTechCv.Core`**: Contains core business logic, AI services (`AiCVService`, `GeminiService`), and other services.
- **`FiveamTechCv.Entities`**: Defines the data models (Nodes) used across the application.
- **`FiveamTechCv.Graph`**: Neo4j driver configuration.
- **`FiveamTechCv.Abstract`**: Contains interfaces and abstractions.
- **`fiveamtechcv-web`**: The Vue 3 frontend application source code.

## Notes

- **GraphQL**: The project uses HotChocolate for GraphQL. You can explore the schema and run queries using the built-in Banana Cake Pop IDE at `/graphql`.
- **AI & Vector Search**: The `AiCVService` uses vector embeddings to find relevant context from the CV and feeds it to Gemini to answer natural language questions.
