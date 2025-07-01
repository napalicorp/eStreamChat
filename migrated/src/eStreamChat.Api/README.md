# eStreamChat API

A modern .NET 9 REST API for the eStreamChat system, migrated from the original WCF service.

## Features

- RESTful API endpoints
- Token-based authentication
- Input validation
- OpenAPI/Swagger documentation
- Error handling
- Async operations
- Logging

## API Endpoints

### Chat Room Operations

- `POST /api/chat/rooms/{chatRoomId}/join` - Join a chat room
- `POST /api/chat/rooms/{chatRoomId}/leave` - Leave a chat room
- `GET /api/chat/rooms/{chatRoomId}/events` - Get chat room events
- `POST /api/chat/rooms/{chatRoomId}/messages` - Send a message
- `POST /api/chat/rooms/{chatRoomId}/commands` - Execute a chat command
- `POST /api/chat/rooms/{chatRoomId}/broadcasts` - Start video broadcast
- `DELETE /api/chat/rooms/{chatRoomId}/broadcasts` - Stop video broadcast

## Authentication

The API uses token-based authentication. Include the token in the Authorization header:

```
Authorization: Bearer <token>
```

The token is obtained from the `/api/chat/rooms/{chatRoomId}/join` endpoint.

## Configuration

Key settings in appsettings.json:

```json
{
  "PollingInterval": 1000,
  "Chat": {
    "EnableFileTransfer": true,
    "EnableVideoChat": true,
    "FlashMediaServer": "rtmp://localhost/live"
  }
}
```

## Development

1. Install .NET 9 SDK
2. Clone the repository
3. Navigate to the API project directory
4. Run the project:
   ```
   dotnet run
   ```
5. Access Swagger UI at `http://localhost:5000/swagger`

## Error Handling

The API returns standard HTTP status codes and JSON error responses:

```json
{
  "error": "Error message",
  "details": "Additional error details (optional)"
}
```

Common status codes:
- 200: Success
- 400: Bad Request (validation errors)
- 401: Unauthorized
- 404: Not Found
- 500: Internal Server Error