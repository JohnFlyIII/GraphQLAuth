# GraphQL Authentication & Authorization Demo

This project demonstrates JWT authentication and role-based authorization patterns using:
- .NET 9
- HotChocolate GraphQL v15
- Entity Framework Core with PostgreSQL
- Docker containerization

## Authorization Model

### JWT Structure
JWTs should contain a `client_roles` claim with an array of objects:
```json
{
  "client_roles": [
    { "ClientId": "guid", "RoleId": "ClientOwner" },
    { "ClientId": "guid", "RoleId": "ClientUser" }
  ]
}
```

### Roles
- **SystemAdmin**: Full access to all client data
- **ClientOwner**: Full access to blogs for their client, including BlogOwnerNotes
- **ClientUser**: Read access to blogs for their client, excluding BlogOwnerNotes

## Running the Application

### Using Docker Compose
```bash
docker-compose up
```

### Local Development
1. Ensure PostgreSQL is running
2. Update connection string in appsettings.json
3. Run migrations: `dotnet ef database update`
4. Run the API: `dotnet run`

## GraphQL Queries

### Get all blogs (filtered by user's client roles)
```graphql
query {
  blogs {
    id
    title
    content
    author
    blogOwnerNotes  # Only visible to ClientOwner or SystemAdmin
  }
}
```

### Get specific blog
```graphql
query {
  blog(id: "guid") {
    id
    title
    content
    blogOwnerNotes
  }
}
```

## Testing Authorization
1. Generate JWTs with different role combinations
2. Test access patterns:
   - Unauthenticated users: No access
   - ClientUser: Can see blogs for their client, no BlogOwnerNotes
   - ClientOwner: Can see blogs for their client, including BlogOwnerNotes
   - SystemAdmin: Can see all blogs with BlogOwnerNotes