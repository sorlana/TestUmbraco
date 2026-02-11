# Database Migration: FormSubmissions Table

## Overview

This migration creates the `FormSubmissions` table for storing form submissions from the Umbraco Form Builder.

## Migration Details

**Migration Name:** `AddFormSubmissionsTable`  
**Migration ID:** `20260210120000_AddFormSubmissionsTable`  
**Created:** 2026-02-10

## Table Structure

### FormSubmissions

| Column | Type | Constraints | Description |
|--------|------|-------------|-------------|
| Id | INT | PRIMARY KEY, IDENTITY(1,1) | Unique identifier |
| FormId | INT | NOT NULL, INDEXED | ID of the form (formBuilderBlock) |
| FormTitle | NVARCHAR(255) | NOT NULL | Title of the form |
| SubmittedAt | DATETIME2 | NOT NULL, INDEXED | Date and time of submission |
| IpAddress | NVARCHAR(50) | NOT NULL | IP address of the submitter |
| FieldValuesJson | NVARCHAR(MAX) | NOT NULL | JSON string containing all field values |
| CreatedAt | DATETIME2 | NOT NULL, DEFAULT GETUTCDATE() | Record creation timestamp |
| UpdatedAt | DATETIME2 | NULL | Record update timestamp |
| IsActive | BIT | NOT NULL, DEFAULT 1 | Soft delete flag |

### Indexes

1. **IX_FormSubmissions_FormId** - Index on `FormId` column for fast lookups by form
2. **IX_FormSubmissions_SubmittedAt** - Index on `SubmittedAt` column for date-based queries

## How to Apply the Migration

### Option 1: Using Entity Framework CLI

```bash
# Navigate to the solution root
cd D:\SITES\Work\umbraco-test-local

# Apply the migration
dotnet ef database update --project TestUmbraco.Domain --startup-project TestUmbraco --context AppDbContext
```

### Option 2: Automatic Migration on Application Start

The migration will be automatically applied when the application starts if you have automatic migrations enabled in your `Program.cs`:

```csharp
using (var scope = app.Services.CreateScope())
{
    var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    dbContext.Database.Migrate();
}
```

### Option 3: Manual SQL Script

If you prefer to apply the migration manually, you can generate a SQL script:

```bash
dotnet ef migrations script --project TestUmbraco.Domain --startup-project TestUmbraco --context AppDbContext --output migration.sql
```

Then execute the generated SQL script against your database.

## Rollback

To rollback this migration:

```bash
dotnet ef database update 20240208081306_Initial --project TestUmbraco.Domain --startup-project TestUmbraco --context AppDbContext
```

## Verification

After applying the migration, verify the table was created:

```sql
-- Check if table exists
SELECT * FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME = 'FormSubmissions'

-- Check indexes
SELECT * FROM sys.indexes WHERE object_id = OBJECT_ID('FormSubmissions')

-- Check table structure
EXEC sp_help 'FormSubmissions'
```

## Connection String

The migration uses the `TestUmbracoData` connection string from `appsettings.json`:

```json
"ConnectionStrings": {
  "TestUmbracoData": "Server=localhost,1433;Database=TestUmbraco;User Id=sa;Password=Str0ngP@ssw0rd!2024;TrustServerCertificate=true;MultipleActiveResultSets=true"
}
```

## Related Files

- **Entity:** `TestUmbraco.Domain/Models/FormSubmission.cs`
- **DbContext:** `TestUmbraco.Domain/AppDbContext.cs`
- **Repository Interface:** `TestUmbraco.Domain/Contracts/IFormSubmissionRepository.cs`
- **Migration:** `TestUmbraco.Domain/Migrations/20260210120000_AddFormSubmissionsTable.cs`
- **Designer:** `TestUmbraco.Domain/Migrations/20260210120000_AddFormSubmissionsTable.Designer.cs`

## Notes

- The table includes all properties from `EntityBase` (Id, CreatedAt, UpdatedAt, IsActive)
- The `CreatedAt` column has a default value of `GETUTCDATE()` to automatically set the creation timestamp
- The `IsActive` column defaults to `true` for soft delete functionality
- Two indexes are created for optimal query performance on `FormId` and `SubmittedAt` columns
- The `FieldValuesJson` column stores form field data as a JSON string for flexibility

## Requirements Validation

This migration satisfies **Requirement 8.2** from the specification:

> КОГДА сущность FormSubmission создается, ТО Система ДОЛЖНА сохранить следующие данные:
> - ID формы (formBuilderBlock)
> - Дата и время отправки
> - IP адрес отправителя
> - Данные всех полей в структурированном формате (JSON)

All required fields are present with appropriate data types and constraints.
