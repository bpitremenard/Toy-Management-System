## Migration History

> **Note:** This project did not use Entity Framework migrations. Instead, all database schema changes were applied directly using SQL scripts in SQL Server Management Studio (SSMS).

| Change Name | Date Applied | Description |
|------------|-------------|------------|
| Initial Database Creation | 2025-12-05 | Created the full database schema including all tables, constraints, and relationships using a SQL script |
| Schema Updates | Various | Modifications to tables (columns, constraints, defaults) were applied directly through SSMS as needed during development |

### Explanation

Instead of using Entity Framework migrations, the team managed database changes manually through SQL scripts executed in SSMS. This approach allowed for direct control over schema design, constraints, and triggers at the database level.

All changes were applied directly to the production database, and the final schema reflects the current state of the system. While this method provides flexibility, it does not maintain an automatic versioned migration history like Entity Framework.