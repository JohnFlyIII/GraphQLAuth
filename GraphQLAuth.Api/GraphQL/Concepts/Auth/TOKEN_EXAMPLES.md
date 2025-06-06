# Token Generation Examples

## Get Test Client IDs First

```graphql
query GetTestClientIds {
  testClientIds {
    lizProBloggers
    davidsDiscountContent
    paulsPremiumBlogs
  }
}
```

## Generate Tokens for Different Roles

### 1. SystemAdmin Token
```graphql
mutation GenerateSystemAdminToken {
  generateAuthToken(role: "systemadmin") 
}
```

### 2. ClientOwner Token (for Liz's Pro Bloggers)
```graphql
mutation GenerateClientOwnerToken {
  generateAuthToken(
    role: "clientowner", 
    clientId: "11111111-1111-1111-1111-111111111111"
  )
}
```

### 3. ClientUser Token (for David's Discount Content)
```graphql
mutation GenerateClientUserToken {
  generateAuthToken(
    role: "clientuser", 
    clientId: "22222222-2222-2222-2222-222222222222"
  )
}
```

### 4. ClientOwner Token (for Paul's Premium Blogs)
```graphql
mutation GenerateClientOwnerTokenPaul {
  generateAuthToken(
    role: "clientowner", 
    clientId: "33333333-3333-3333-3333-333333333333"
  )
}
```

## Using the Token

Add the returned token to your request headers:
```
Authorization: Bearer <your-token-here>
```

## Test Queries After Getting Token

### SystemAdmin - Can see all data
```graphql
query GetAllBlogs {
  blogs {
    id
    title
    clientId
    blogOwnerNotes
  }
}
```

### ClientOwner - Can see their client's data + sensitive fields
```graphql
query GetMyBlogsAsOwner {
  blogs {
    id
    title
    clientId
    blogOwnerNotes  # Only ClientOwners can see this
  }
  assetLibrary {
    id
    name
    type
    base64Data  # Only ClientOwners can see actual asset data
    url         # Only ClientOwners can see actual asset URLs
  }
}
```

### ClientUser - Can see their client's data (no sensitive fields)
```graphql
query GetMyBlogsAsUser {
  blogs {
    id
    title
    clientId
    # blogOwnerNotes - This will be null for ClientUser
  }
  assetLibrary {
    id
    name
    type
    # base64Data - This will be null for ClientUser
    # url        - This will be null for ClientUser
  }
}
```

### Status (Any authenticated user with client roles)
```graphql
query GetStatus {
  status {
    health
    lastUpdated
    supportContact
    systemVersion
    # notes - Only SystemAdmin can see this field
  }
}
```

### Update Status (SystemAdmin only)
```graphql
mutation UpdateStatus {
  updateSystemStatus(input: {
    health: HEALTHY
    supportContact: "support@mycompany.com"
    systemVersion: "2.0.0"
    notes: "All systems operational"
  }) {
    health
    lastUpdated
    supportContact
    systemVersion
    notes
  }
}
```