# Database schema

The application runs on **MS SQL Server**

- `schema-mssql.sql` — authoritative DDL, generated from EF Core migrations
  (`dotnet ef migrations script --idempotent`).
- `schema-mysql.sql` — MySQL-equivalent schema for opening in MySQL Workbench
  (per task requirements). Types are mapped; structure, indexes and foreign key
  mirror the EF Core model.

## Regenerate MS SQL script
```
cd backend/dZENcode.API

dotnet ef migrations script --idempotent -o ../../db/schema-mssql.sql
```