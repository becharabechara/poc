# SmartNotes

SmartNotes is a web application that allows users to create, edit, search, and categorize notes efficiently.

## Features

- **Create Notes**: Add new notes with title, content, and tags.
- **Edit Notes**: Modify existing notes.
- **Search Notes**: Find notes by keywords or tags.
- **Categorize Notes**: Organize notes using tags and categories.

## Architecture

The application follows a hexagonal architecture for the backend, ensuring separation of concerns and testability.

- **Backend**: .NET 8 with ASP.NET Core, structured in layers: Domain, Application, Infrastructure, Presentation.
- **Frontend**: React application for the user interface.
- **Database**: PostgreSQL running in a Docker container.
- **Deployment**: All components orchestrated via a single Docker Compose file.

## Execution Plan

### ✅ Completed Steps

1. **✅ Preparation of Workspace and Initial Structure** (1-2 hours) - **COMPLETED**:
   - ✅ Create project directories and basic files for .NET solution and React project.
   - ✅ Initialize Git repository and add README.md.

2. **✅ Implementation of Backend Domain** (2-3 hours) - **COMPLETED**:
   - ✅ Define domain entities and business rules (Note entity with validation).
   - ✅ Implement Tag value object with business rules.
   - ✅ Add domain events (NoteCreated, NoteUpdated).
   - ✅ Add unit tests for the domain layer (NoteTests.cs).

3. **✅ Implementation of Backend Application** (3-4 hours) - **COMPLETED**:
   - ✅ Create use cases and ports (INoteRepository interface).
   - ✅ Implement business logic for CRUD operations and search.
   - ✅ Add comprehensive unit tests for all use cases.

4. **✅ Implementation of Backend Infrastructure** (4-5 hours) - **COMPLETED**:
   - ✅ Configure Entity Framework Core for PostgreSQL.
   - ✅ Implement repositories and database migrations.
   - ✅ Add integration tests.

### 🔄 Remaining Steps

5. **🟡 Implementation of Backend Presentation** (2-3 hours) - **MINIMAL**:
   - ✅ Basic Program.cs setup with Swagger.
   - ❌ Create REST API controllers.
   - ❌ Configure CORS for frontend communication.
   - ❌ Test API endpoints.

6. **🟡 Implementation of React Frontend** (4-5 hours) - **MINIMAL**:
   - ✅ Basic React project structure and dependencies.
   - ❌ Build UI components (note list, form, search).
   - ❌ Integrate API calls.
   - ❌ Add unit tests.

7. **🔴 Docker Configuration and Docker Compose** (2-3 hours) - **NOT STARTED**:
   - ❌ Create Dockerfiles for backend, frontend, and database.
   - ❌ Set up docker-compose.yml for the entire stack.
   - ❌ Test the full deployment.

8. **🔴 Integrated Tests and Finalization** (2-3 hours) - **NOT STARTED**:
   - ❌ Perform end-to-end tests.
   - ❌ Optimize performance (pagination, caching).
   - ❌ Final documentation.

**Total Estimated Time**: 20-30 hours  
**Progress**: ~60% complete (12/20 hours estimated)  
**Remaining**: ~8-10 hours

## Code Quality & Testing

### 📊 Test Coverage Summary
- **Overall Coverage**: 59.7% (379/634 coverable lines)
- **Domain Layer**: 97.9% ✅ (Excellent)
- **Application Layer**: 98.2% ✅ (Excellent)  
- **Infrastructure Layer**: 40.8% ⚠️ (Affected by config classes and auto-generated migrations)

### 🧪 Test Statistics
- **Total Tests**: 48 (All Passing ✅)
  - Domain Tests: 22
  - Application Tests: 14  
  - Infrastructure Tests: 12
- **Method Coverage**: 80.9% (98/121 methods)
- **Branch Coverage**: 80.1% (93/116 branches)

*Note: Infrastructure coverage is lower due to configuration POCOs (0% coverage) and auto-generated EF migrations (0% coverage), but core business logic maintains excellent coverage.*

## Prerequisites

- .NET 8 SDK
- Node.js and npm
- Docker and Docker Compose

## Getting Started

1. Clone the repository.
2. Navigate to the project directory.
3. Run `docker-compose up` to start all services.

## Contributing

Please follow the execution plan for contributions.