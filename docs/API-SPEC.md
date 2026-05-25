# Central Park — API Specification

All endpoints are prefixed with `/api/v1`.  
All requests and responses are `application/json`.  
Protected endpoints require `Authorization: Bearer <jwt>`.

---

## Endpoint Summary

| Method | Path | Auth | Description |
|---|---|---|---|
| `GET` | `/users/{email}` | Required | Get user profile with suburb, favourites, and orders |
| `POST` | `/users` | None | Create user on first Google sign-in |
| `PUT` | `/users/{email}` | Required | Update user's home suburb |
| `GET` | `/suburbs` | Required | List all suburbs (for profile setup) |
| `GET` | `/stores/{suburbId}` | Required | List stores in a suburb |
| `POST` | `/store-views` | Required | Record a store view (fire-and-forget) |
| `POST` | `/orders` | Required | Place a new order |
| `PUT` | `/orders/{orderId}` | Required | Update order status |
| `POST` | `/favourites` | Required | Add a store to favourites |
| `DELETE` | `/favourites/{storeId}` | Required | Remove a store from favourites |

---

## Users

### `GET /users/{email}`

Retrieves the full user profile for a signed-in user. Email is sourced from the Google OAuth token.

**Auth:** Required (user can only fetch their own profile)

**Path params**

| Param | Type | Description |
|---|---|---|
| `email` | string | URL-encoded email address from Google sign-in |

**Response `200 OK`**
```json
{
  "userId": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
  "email": "user@example.com",
  "firstName": "Jane",
  "lastName": "Smith",
  "phoneNumber": "+61412345678",
  "avatarUrl": "https://...",
  "homeSuburb": {
    "id": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
    "name": "Bondi Beach",
    "postCode": "2026",
    "state": "NSW"
  },
  "favouriteStores": [
    {
      "storeId": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
      "name": "Bondi Farmers Market",
      "suburbName": "Bondi Beach",
      "isAcceptingOrders": true
    }
  ],
  "recentOrders": [
    {
      "orderId": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
      "storeName": "Bondi Farmers Market",
      "status": "Completed",
      "totalAmount": 42.50,
      "itemCount": 3,
      "placedAt": "2026-05-20T09:15:00Z"
    }
  ]
}
```

**Error responses**

| Status | Code | When |
|---|---|---|
| `404` | `User.NotFound` | No user exists for this email |
| `403` | `Auth.Forbidden` | Authenticated user is not the owner of this profile |

---

### `POST /users`

Creates a new user on first Google sign-in. Idempotent on email — returns existing user if already registered.

**Auth:** None (called immediately after OAuth, before a JWT is issued)

**Request body**
```json
{
  "email": "user@example.com",
  "firstName": "Jane",
  "lastName": "Smith",
  "avatarUrl": "https://..."
}
```

| Field | Type | Required | Notes |
|---|---|---|---|
| `email` | string | Yes | From Google OAuth token |
| `firstName` | string | Yes | From Google OAuth token |
| `lastName` | string | Yes | From Google OAuth token |
| `avatarUrl` | string | No | Google profile picture URL |

**Response `201 Created`**
```json
{
  "userId": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
  "email": "user@example.com",
  "isNewUser": true
}
```

`isNewUser: false` when the user already existed — client uses this to decide whether to show the onboarding suburb-picker flow.

**Error responses**

| Status | Code | When |
|---|---|---|
| `400` | `Validation.*` | Missing or invalid fields |

---

### `PUT /users/{email}`

Updates the authenticated user's home suburb preference.

**Auth:** Required

**Path params**

| Param | Type | Description |
|---|---|---|
| `email` | string | URL-encoded email address |

**Request body**
```json
{
  "homeSuburbId": "3fa85f64-5717-4562-b3fc-2c963f66afa6"
}
```

**Response `200 OK`**
```json
{
  "homeSuburb": {
    "id": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
    "name": "Surry Hills",
    "postCode": "2010",
    "state": "NSW"
  }
}
```

**Error responses**

| Status | Code | When |
|---|---|---|
| `404` | `Suburb.NotFound` | `homeSuburbId` does not exist |
| `403` | `Auth.Forbidden` | Authenticated user is not the profile owner |

---

## Suburbs

### `GET /suburbs`

Returns all suburbs. Used to populate the suburb picker on first sign-in and profile update screens.

**Auth:** Required

**Query params**

| Param | Type | Required | Description |
|---|---|---|---|
| `search` | string | No | Filter by suburb name or postcode (min 2 chars) |
| `state` | string | No | Filter by state code e.g. `NSW` |

**Response `200 OK`**
```json
{
  "suburbs": [
    {
      "id": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
      "name": "Bondi Beach",
      "postCode": "2026",
      "state": "NSW"
    }
  ]
}
```

> `GeoBoundaries` and centroid coordinates are excluded from list responses — fetch individual suburb detail if needed in future.

---

## Stores

### `GET /stores/{suburbId}`

Returns all active stores in the given suburb.

**Auth:** Required

**Path params**

| Param | Type | Description |
|---|---|---|
| `suburbId` | Guid | The suburb to query |

**Query params**

| Param | Type | Required | Description |
|---|---|---|---|
| `acceptingOrders` | bool | No | Filter to stores currently accepting orders |

**Response `200 OK`**
```json
{
  "stores": [
    {
      "storeId": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
      "name": "Bondi Farmers Market",
      "description": "Fresh produce every Saturday",
      "streetAddress": "Queen Elizabeth Drive",
      "postCode": "2026",
      "isAcceptingOrders": true,
      "ownerDisplayName": "John Grower",
      "ownerContactPhone": "+61411111111",
      "openHours": [
        {
          "dayOfWeek": 6,
          "openTime": "07:00",
          "closeTime": "13:00",
          "isClosed": false
        }
      ]
    }
  ]
}
```

**Error responses**

| Status | Code | When |
|---|---|---|
| `404` | `Suburb.NotFound` | `suburbId` does not exist |

---

## Store Views

### `POST /store-views`

Records that the authenticated user viewed a store. Processed asynchronously — client should fire-and-forget.

**Auth:** Required

**Request body**
```json
{
  "storeId": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
  "source": "search"
}
```

| Field | Type | Required | Notes |
|---|---|---|---|
| `storeId` | Guid | Yes | |
| `source` | string | No | Entry point: `search`, `map`, `favourites`, `suburb` |

**Response `202 Accepted`** _(no body)_

> Returns `202` not `201` — the write is queued, not immediately committed.

**Error responses**

| Status | Code | When |
|---|---|---|
| `404` | `Store.NotFound` | `storeId` does not exist |

---

## Orders

### `POST /orders`

Places a new order at a store on behalf of the authenticated customer.

**Auth:** Required

**Request body**
```json
{
  "storeId": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
  "notes": "No nuts please",
  "items": [
    {
      "productName": "Heirloom Tomatoes",
      "quantity": 2,
      "unitPrice": 8.50
    }
  ]
}
```

| Field | Type | Required | Notes |
|---|---|---|---|
| `storeId` | Guid | Yes | |
| `notes` | string | No | Max 500 chars |
| `items` | array | Yes | Min 1 item |
| `items[].productName` | string | Yes | |
| `items[].quantity` | int | Yes | Min 1 |
| `items[].unitPrice` | decimal | Yes | |

**Response `201 Created`**
```json
{
  "orderId": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
  "status": "Pending",
  "totalAmount": 17.00,
  "placedAt": "2026-05-25T08:00:00Z"
}
```

**Error responses**

| Status | Code | When |
|---|---|---|
| `404` | `Store.NotFound` | `storeId` does not exist |
| `409` | `Store.NotAcceptingOrders` | Store has `isAcceptingOrders: false` |
| `400` | `Validation.*` | Missing or invalid fields |

---

### `PUT /orders/{orderId}`

Updates the status of an existing order. Customers can only cancel (`Cancelled`). Store owners can progress through all statuses.

**Auth:** Required

**Path params**

| Param | Type | Description |
|---|---|---|
| `orderId` | Guid | |

**Request body**
```json
{
  "status": "Confirmed"
}
```

**Allowed status transitions**

| From | To | Who |
|---|---|---|
| `Pending` | `Confirmed` | Store owner |
| `Confirmed` | `Preparing` | Store owner |
| `Preparing` | `Ready` | Store owner |
| `Ready` | `Completed` | Store owner |
| `Pending` | `Cancelled` | Customer or store owner |
| `Confirmed` | `Cancelled` | Store owner only |

**Response `200 OK`**
```json
{
  "orderId": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
  "status": "Confirmed",
  "updatedAt": "2026-05-25T08:05:00Z"
}
```

**Error responses**

| Status | Code | When |
|---|---|---|
| `404` | `Order.NotFound` | Order does not exist |
| `409` | `Order.InvalidTransition` | Status transition is not permitted |
| `403` | `Auth.Forbidden` | Caller is not the customer or store owner |

---

## Favourites

### `POST /favourites`

Adds a store to the authenticated customer's favourites.

**Auth:** Required

**Request body**
```json
{
  "storeId": "3fa85f64-5717-4562-b3fc-2c963f66afa6"
}
```

**Response `201 Created`**
```json
{
  "favouriteId": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
  "storeId": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
  "createdAt": "2026-05-25T08:00:00Z"
}
```

**Error responses**

| Status | Code | When |
|---|---|---|
| `404` | `Store.NotFound` | `storeId` does not exist |
| `409` | `Favourite.AlreadyExists` | Store already favourited |

---

### `DELETE /favourites/{storeId}`

Removes a store from the authenticated customer's favourites.

**Auth:** Required

**Path params**

| Param | Type | Description |
|---|---|---|
| `storeId` | Guid | |

**Response `204 No Content`** _(no body)_

**Error responses**

| Status | Code | When |
|---|---|---|
| `404` | `Favourite.NotFound` | Store was not in favourites |

---

## Order Status Reference

| Status | Meaning |
|---|---|
| `Pending` | Placed by customer, awaiting store confirmation |
| `Confirmed` | Store has accepted the order |
| `Preparing` | Store is preparing the order |
| `Ready` | Order is ready for pickup |
| `Completed` | Order collected |
| `Cancelled` | Cancelled by customer or store |
