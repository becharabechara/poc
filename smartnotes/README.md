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

1. **Preparation of Workspace and Initial Structure** (1-2 hours):
   - Create project directories and basic files for .NET solution and React project.
   - Initialize Git repository and add README.md.

2. **Implementation of Backend Domain** (2-3 hours):
   - Define domain entities and business rules (e.g., Note entity).
   - Add unit tests for the domain layer.

3. **Implementation of Backend Application** (3-4 hours):
   - Create use cases and ports (interfaces).
   - Implement business logic for CRUD operations and search.

4. **Implementation of Backend Infrastructure** (4-5 hours):
   - Configure Entity Framework Core for PostgreSQL.
   - Implement repositories and database migrations.
   - Add integration tests.

5. **Implementation of Backend Presentation** (2-3 hours):
   - Create REST API controllers.
   - Configure CORS for frontend communication.
   - Test API endpoints.

6. **Implementation of React Frontend** (4-5 hours):
   - Build UI components (note list, form, search).
   - Integrate API calls.
   - Add unit tests.

7. **Docker Configuration and Docker Compose** (2-3 hours):
   - Create Dockerfiles for backend, frontend, and database.
   - Set up docker-compose.yml for the entire stack.
   - Test the full deployment.

8. **Integrated Tests and Finalization** (2-3 hours):
   - Perform end-to-end tests.
   - Optimize performance (pagination, caching).
   - Final documentation.

**Total Estimated Time**: 20-30 hours.

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