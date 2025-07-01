# eStreamChat

A modern, containerized chat application built with .NET 9.

## Prerequisites

- [.NET 9 SDK](https://dotnet.microsoft.com/download/dotnet/9.0)
- [Docker](https://www.docker.com/products/docker-desktop)
- [Docker Compose](https://docs.docker.com/compose/install/)

## Solution Structure

- `eStreamChat.Interfaces`: Core interfaces and models
- `eStreamChat.Common`: Shared utilities and services
- `eStreamChat.Api`: REST API and real-time chat endpoints

## Getting Started

1. Clone the repository:
```bash
git clone https://github.com/yourusername/eStreamChat.git
cd eStreamChat/migrated
```

2. Build the solution:
```bash
dotnet build
```

3. Start the application using Docker Compose:
```bash
docker-compose up -d
```

This will start:
- SQL Server on port 1433
- Redis on port 6379
- API on port 5000

4. Access the API:
- Swagger UI: http://localhost:5000/swagger
- API Endpoint: http://localhost:5000/api

## Development

### Database Connection
The application uses SQL Server with the following default connection string:
```
Server=localhost,1433;Database=eStreamChat;User Id=sa;Password=YourStrong@Passw0rd;TrustServerCertificate=True;
```

### Redis Cache
Redis is configured at:
```
localhost:6379
```

### Running Tests
```bash
dotnet test
```

### Building Docker Images
```bash
docker-compose build
```

## Configuration

Key configuration files:
- `docker-compose.yml`: Container orchestration
- `global.json`: .NET SDK version
- `Directory.Build.props`: Common project properties

## Contributing

1. Fork the repository
2. Create a feature branch
3. Commit your changes
4. Push to the branch
5. Create a Pull Request

## License

This project is licensed under the MIT License - see the LICENSE file for details.