# Database Integration for Installments & Payment Plans Module

## Database Connection

The connection string is configured in `DatabaseHelper.cs`. By default, it uses:
- Server: localhost
- Database: RealEstateDB
- Authentication: Integrated Security (Windows Authentication)
- Trust Server Certificate: true

**To change the connection string**, edit the `ConnectionString` property in `Data/DatabaseHelper.cs`:

```csharp
private static readonly string ConnectionString = 
    "Server=YOUR_SERVER;Database=RealEstateDB;Integrated Security=true;TrustServerCertificate=true;";
```

For SQL Server Authentication, use:
```csharp
private static readonly string ConnectionString = 
    "Server=YOUR_SERVER;Database=RealEstateDB;User Id=YOUR_USER;Password=YOUR_PASSWORD;TrustServerCertificate=true;";
```

## Database Schema

The module uses the following tables:
- **PaymentPlans** - Stores payment plan details
- **Sales** - Links buyers, plots, and projects
- **Parties** - Stores buyer/seller information
- **Plots** - Plot information
- **Projects** - Project information
- **Installments** - Individual installment records

## Features

1. **CRUD Operations**: Create, Read, Update, Delete payment plans
2. **Automatic Installment Creation**: When a payment plan is created, installments are automatically generated
3. **Buyer and Plot Loading**: Dropdown lists are populated from the database
4. **Data Validation**: All operations include error handling and validation

## Usage

The `Page48Page` (Installments & Payment Plans) now loads data from the database instead of using sample data. Ensure:
1. The database is set up using the provided SQL script
2. The connection string is configured correctly
3. There is at least one Buyer (Party with Type='Buyer') and one Plot in the database

