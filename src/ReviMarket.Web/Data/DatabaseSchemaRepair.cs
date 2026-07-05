using System.Data;
using System.Data.Common;
using Microsoft.EntityFrameworkCore;

namespace ReviMarket.Web.Data;

public static class DatabaseSchemaRepair
{
    public static async Task EnsureSqliteSchemaAsync(ApplicationDbContext db)
    {
        if (!db.Database.IsSqlite())
        {
            return;
        }

        var connection = db.Database.GetDbConnection();
        var shouldClose = connection.State != ConnectionState.Open;
        if (shouldClose)
        {
            await connection.OpenAsync();
        }

        try
        {
            if (await TableExistsAsync(connection, "AspNetUsers"))
            {
                var columns = await GetColumnsAsync(connection, "AspNetUsers");
                await AddColumnIfMissingAsync(connection, columns, "AspNetUsers", "DisplayName", "TEXT NOT NULL DEFAULT ''");
                await AddColumnIfMissingAsync(connection, columns, "AspNetUsers", "AccountType", "TEXT NOT NULL DEFAULT 'Customer'");
                await AddColumnIfMissingAsync(connection, columns, "AspNetUsers", "LegalType", "TEXT NOT NULL DEFAULT 'Individual'");
                await AddColumnIfMissingAsync(connection, columns, "AspNetUsers", "OrganizationName", "TEXT NULL");
                await AddColumnIfMissingAsync(connection, columns, "AspNetUsers", "Inn", "TEXT NULL");
                await AddColumnIfMissingAsync(connection, columns, "AspNetUsers", "OgrnOrOgrnip", "TEXT NULL");
                await AddColumnIfMissingAsync(connection, columns, "AspNetUsers", "LegalAddress", "TEXT NULL");
                await AddColumnIfMissingAsync(connection, columns, "AspNetUsers", "Rating", "TEXT NOT NULL DEFAULT '0'");
                await AddColumnIfMissingAsync(connection, columns, "AspNetUsers", "ReviewsCount", "INTEGER NOT NULL DEFAULT 0");
                await AddColumnIfMissingAsync(connection, columns, "AspNetUsers", "CreatedAt", "TEXT NOT NULL DEFAULT '2026-01-01 00:00:00'");
                await AddColumnIfMissingAsync(connection, columns, "AspNetUsers", "LastSeenAt", "TEXT NULL");
            }
        }
        finally
        {
            if (shouldClose)
            {
                await connection.CloseAsync();
            }
        }
    }

    private static async Task<bool> TableExistsAsync(DbConnection connection, string tableName)
    {
        await using var command = connection.CreateCommand();
        command.CommandText = "SELECT 1 FROM sqlite_master WHERE type = 'table' AND name = $tableName LIMIT 1";
        AddParameter(command, "$tableName", tableName);
        return await command.ExecuteScalarAsync() is not null;
    }

    private static async Task<HashSet<string>> GetColumnsAsync(DbConnection connection, string tableName)
    {
        await using var command = connection.CreateCommand();
        command.CommandText = $"PRAGMA table_info(\"{tableName}\")";

        var columns = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        await using var reader = await command.ExecuteReaderAsync();
        while (await reader.ReadAsync())
        {
            columns.Add(reader.GetString(1));
        }

        return columns;
    }

    private static async Task AddColumnIfMissingAsync(
        DbConnection connection,
        HashSet<string> columns,
        string tableName,
        string columnName,
        string definition)
    {
        if (columns.Contains(columnName))
        {
            return;
        }

        await using var command = connection.CreateCommand();
        command.CommandText = $"ALTER TABLE \"{tableName}\" ADD COLUMN \"{columnName}\" {definition}";
        await command.ExecuteNonQueryAsync();
        columns.Add(columnName);
    }

    private static void AddParameter(DbCommand command, string name, object value)
    {
        var parameter = command.CreateParameter();
        parameter.ParameterName = name;
        parameter.Value = value;
        command.Parameters.Add(parameter);
    }
}
