# Central Park — Entity Relationship Diagram

```mermaid
erDiagram

    %% ─── Identity / User ───────────────────────────────────────────────
    ApplicationUser {
        Guid        Id          PK
        string      Email
        string      PasswordHash
        DateTime    CreatedAt
    }

    UserProfile {
        Guid        Id              PK
        Guid        UserId          FK
        Guid        HomeSuburbId    FK
        string      FirstName
        string      LastName
        string      PhoneNumber
        string      AvatarUrl
        DateTime    CreatedAt
        DateTime    UpdatedAt
    }

    CustomerProfile {
        Guid        Id          PK
        Guid        UserId      FK
        DateTime    CreatedAt
    }

    BusinessOwnerProfile {
        Guid        Id          PK
        Guid        UserId      FK
        string      BusinessName
        string      BusinessEmail
        string      BusinessPhone
        DateTime    CreatedAt
        DateTime    UpdatedAt
    }

    %% ─── Geography ──────────────────────────────────────────────────────
    Suburb {
        Guid        Id          PK
        string      Name
        string      PostCode
        string      State
        string      Country
        decimal     CentroidLat
        decimal     CentroidLng
        string      GeoBoundaries
    }

    %% ─── Store ──────────────────────────────────────────────────────────
    Store {
        Guid        Id                      PK
        Guid        BusinessOwnerProfileId  FK
        Guid        SuburbId                FK
        string      Name
        string      Description
        string      StreetAddress
        string      PostCode
        decimal     Latitude
        decimal     Longitude
        bool        IsAcceptingOrders
        bool        IsActive
        string      OwnerDisplayName
        string      OwnerContactEmail
        string      OwnerContactPhone
        DateTime    CreatedAt
        DateTime    UpdatedAt
    }

    StoreHours {
        Guid        Id          PK
        Guid        StoreId     FK
        int         DayOfWeek
        TimeOnly    OpenTime
        TimeOnly    CloseTime
        bool        IsClosed
    }

    %% ─── Analytics ──────────────────────────────────────────────────────
    StoreView {
        Guid        Id          PK
        Guid        StoreId     FK
        Guid        UserId      FK
        string      Source
        DateTime    ViewedAt
    }

    %% ─── Social ─────────────────────────────────────────────────────────
    Favourite {
        Guid        Id                  PK
        Guid        CustomerProfileId   FK
        Guid        StoreId             FK
        DateTime    CreatedAt
    }

    %% ─── Orders ─────────────────────────────────────────────────────────
    Order {
        Guid        Id                  PK
        Guid        CustomerProfileId   FK
        Guid        StoreId             FK
        string      Status
        decimal     TotalAmount
        string      Notes
        DateTime    PlacedAt
        DateTime    UpdatedAt
    }

    OrderItem {
        Guid        Id          PK
        Guid        OrderId     FK
        string      ProductName
        int         Quantity
        decimal     UnitPrice
        decimal     LineTotal
    }

    %% ─── Relationships ──────────────────────────────────────────────────
    ApplicationUser     ||--||      UserProfile             : "has"
    ApplicationUser     ||--o|      CustomerProfile          : "may have"
    ApplicationUser     ||--o|      BusinessOwnerProfile     : "may have"

    UserProfile         }o--o|      Suburb                  : "home suburb"

    BusinessOwnerProfile||--o{      Store                   : "owns"

    Store               }|--||      Suburb                  : "located in"
    Store               ||--o{      StoreHours              : "open hours"
    Store               ||--o{      Favourite               : "favourited by"
    Store               ||--o{      Order                   : "receives"

    Store               ||--o{      StoreView               : "viewed via"
    ApplicationUser     ||--o{      StoreView               : "generates"

    CustomerProfile     ||--o{      Favourite               : "has"
    CustomerProfile     ||--o{      Order                   : "places"

    Order               ||--o{      OrderItem               : "contains"
```

## Notes

| Entity | Key decisions |
|---|---|
| `UserProfile` | Separates app data from ASP.NET Identity. One per user always. |
| `CustomerProfile` | Created when user adopts the customer persona. Absence = not a customer. |
| `BusinessOwnerProfile` | Created when user adopts the business owner persona. Same user can hold both. |
| `Suburb` | Seed / reference data. `GeoBoundaries` stored as GeoJSON — upgrade to SQL Server `geography` + NetTopologySuite for proximity queries. |
| `StoreHours` | One row per day. Unique on `(StoreId, DayOfWeek)`. |
| `Favourite` | First-class entity (not a pure junction) — room to add metadata. Unique on `(CustomerProfileId, StoreId)`. |
| `OrderItem` | Denormalises `ProductName` and `UnitPrice` at order time to protect history from future product changes. |
| `StoreView` | Append-only analytics table. `UserId` references `ApplicationUser` directly (not `CustomerProfile`) so guest/anonymous views can be added later by making it nullable. `Source` captures entry point — e.g. `"search"`, `"map"`, `"favourites"`. Never updated, only inserted. |

## Reserved for future design
- `GarageSale` — linked to `Store` or `BusinessOwnerProfile`
- `Activity` — linked to `Suburb` or `Store`
- `Product` — when store inventory becomes first-class
- `Review` — Customer reviews a Store, scoped to a completed Order
