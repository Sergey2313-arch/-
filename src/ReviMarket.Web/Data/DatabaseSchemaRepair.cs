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
            await EnsureTableAsync(connection, "MarketItems", """
                CREATE TABLE "MarketItems" (
                    "Id" INTEGER NOT NULL CONSTRAINT "PK_MarketItems" PRIMARY KEY AUTOINCREMENT,
                    "Title" TEXT NOT NULL,
                    "Description" TEXT NOT NULL,
                    "Price" TEXT NOT NULL DEFAULT '0',
                    "Category" TEXT NOT NULL DEFAULT '',
                    "Type" TEXT NOT NULL DEFAULT 'Product',
                    "ImagePath" TEXT NULL,
                    "ReviewStatus" TEXT NOT NULL DEFAULT 'Pending',
                    "OrderStatus" TEXT NOT NULL DEFAULT 'Open',
                    "OwnerId" TEXT NULL,
                    "AssignedExecutorId" TEXT NULL,
                    "CreatedAt" TEXT NOT NULL DEFAULT '2026-01-01 00:00:00',
                    "AssignedAt" TEXT NULL
                )
                """);

            await EnsureTableAsync(connection, "ChatMessages", """
                CREATE TABLE "ChatMessages" (
                    "Id" INTEGER NOT NULL CONSTRAINT "PK_ChatMessages" PRIMARY KEY AUTOINCREMENT,
                    "SenderId" TEXT NOT NULL,
                    "ReceiverId" TEXT NOT NULL,
                    "Text" TEXT NOT NULL,
                    "CreatedAt" TEXT NOT NULL DEFAULT '2026-01-01 00:00:00'
                )
                """);

            await EnsureTableAsync(connection, "SupportRequests", """
                CREATE TABLE "SupportRequests" (
                    "Id" INTEGER NOT NULL CONSTRAINT "PK_SupportRequests" PRIMARY KEY AUTOINCREMENT,
                    "Title" TEXT NOT NULL,
                    "Text" TEXT NOT NULL,
                    "Status" TEXT NOT NULL DEFAULT 'Open',
                    "Priority" TEXT NOT NULL DEFAULT 'Normal',
                    "UserId" TEXT NULL,
                    "AgentId" TEXT NULL,
                    "CreatedAt" TEXT NOT NULL DEFAULT '2026-01-01 00:00:00'
                )
                """);

            await EnsureTableAsync(connection, "UserCases", """
                CREATE TABLE "UserCases" (
                    "Id" INTEGER NOT NULL CONSTRAINT "PK_UserCases" PRIMARY KEY AUTOINCREMENT,
                    "Title" TEXT NOT NULL,
                    "Text" TEXT NOT NULL,
                    "Status" TEXT NOT NULL DEFAULT 'Open',
                    "CreatedById" TEXT NULL,
                    "TargetUserId" TEXT NULL,
                    "AgentId" TEXT NULL,
                    "CreatedAt" TEXT NOT NULL DEFAULT '2026-01-01 00:00:00'
                )
                """);

            await EnsureTableAsync(connection, "OrderCases", """
                CREATE TABLE "OrderCases" (
                    "Id" INTEGER NOT NULL CONSTRAINT "PK_OrderCases" PRIMARY KEY AUTOINCREMENT,
                    "Title" TEXT NOT NULL,
                    "Text" TEXT NOT NULL,
                    "Status" TEXT NOT NULL DEFAULT 'Open',
                    "MarketItemId" INTEGER NULL,
                    "CreatedById" TEXT NULL,
                    "AgentId" TEXT NULL,
                    "CreatedAt" TEXT NOT NULL DEFAULT '2026-01-01 00:00:00'
                )
                """);

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

            await EnsureMarketItemColumnsAsync(connection);
            await EnsureChatMessageColumnsAsync(connection);
            await EnsureSupportRequestColumnsAsync(connection);
            await EnsureUserCaseColumnsAsync(connection);
            await EnsureOrderCaseColumnsAsync(connection);
        }
        finally
        {
            if (shouldClose)
            {
                await connection.CloseAsync();
            }
        }
    }

    private static async Task EnsureMarketItemColumnsAsync(DbConnection connection)
    {
        if (!await TableExistsAsync(connection, "MarketItems")) return;
        var columns = await GetColumnsAsync(connection, "MarketItems");
        await AddColumnIfMissingAsync(connection, columns, "MarketItems", "Title", "TEXT NOT NULL DEFAULT ''");
        await AddColumnIfMissingAsync(connection, columns, "MarketItems", "Description", "TEXT NOT NULL DEFAULT ''");
        await AddColumnIfMissingAsync(connection, columns, "MarketItems", "Price", "TEXT NOT NULL DEFAULT '0'");
        await AddColumnIfMissingAsync(connection, columns, "MarketItems", "Category", "TEXT NOT NULL DEFAULT ''");
        await AddColumnIfMissingAsync(connection, columns, "MarketItems", "Type", "TEXT NOT NULL DEFAULT 'Product'");
        await AddColumnIfMissingAsync(connection, columns, "MarketItems", "ImagePath", "TEXT NULL");
        await AddColumnIfMissingAsync(connection, columns, "MarketItems", "ReviewStatus", "TEXT NOT NULL DEFAULT 'Pending'");
        await AddColumnIfMissingAsync(connection, columns, "MarketItems", "OrderStatus", "TEXT NOT NULL DEFAULT 'Open'");
        await AddColumnIfMissingAsync(connection, columns, "MarketItems", "OwnerId", "TEXT NULL");
        await AddColumnIfMissingAsync(connection, columns, "MarketItems", "AssignedExecutorId", "TEXT NULL");
        await AddColumnIfMissingAsync(connection, columns, "MarketItems", "CreatedAt", "TEXT NOT NULL DEFAULT '2026-01-01 00:00:00'");
        await AddColumnIfMissingAsync(connection, columns, "MarketItems", "AssignedAt", "TEXT NULL");
    }

    private static async Task EnsureChatMessageColumnsAsync(DbConnection connection)
    {
        if (!await TableExistsAsync(connection, "ChatMessages")) return;
        var columns = await GetColumnsAsync(connection, "ChatMessages");
        await AddColumnIfMissingAsync(connection, columns, "ChatMessages", "SenderId", "TEXT NOT NULL DEFAULT ''");
        await AddColumnIfMissingAsync(connection, columns, "ChatMessages", "ReceiverId", "TEXT NOT NULL DEFAULT ''");
        await AddColumnIfMissingAsync(connection, columns, "ChatMessages", "Text", "TEXT NOT NULL DEFAULT ''");
        await AddColumnIfMissingAsync(connection, columns, "ChatMessages", "CreatedAt", "TEXT NOT NULL DEFAULT '2026-01-01 00:00:00'");
    }

    private static async Task EnsureSupportRequestColumnsAsync(DbConnection connection)
    {
        if (!await TableExistsAsync(connection, "SupportRequests")) return;
        var columns = await GetColumnsAsync(connection, "SupportRequests");
        await AddColumnIfMissingAsync(connection, columns, "SupportRequests", "Title", "TEXT NOT NULL DEFAULT ''");
        await AddColumnIfMissingAsync(connection, columns, "SupportRequests", "Text", "TEXT NOT NULL DEFAULT ''");
        await AddColumnIfMissingAsync(connection, columns, "SupportRequests", "Status", "TEXT NOT NULL DEFAULT 'Open'");
        await AddColumnIfMissingAsync(connection, columns, "SupportRequests", "Priority", "TEXT NOT NULL DEFAULT 'Normal'");
        await AddColumnIfMissingAsync(connection, columns, "SupportRequests", "UserId", "TEXT NULL");
        await AddColumnIfMissingAsync(connection, columns, "SupportRequests", "AgentId", "TEXT NULL");
        await AddColumnIfMissingAsync(connection, columns, "SupportRequests", "CreatedAt", "TEXT NOT NULL DEFAULT '2026-01-01 00:00:00'");
    }

    private static async Task EnsureUserCaseColumnsAsync(DbConnection connection)
    {
        if (!await TableExistsAsync(connection, "UserCases")) return;
        var columns = await GetColumnsAsync(connection, "UserCases");
        await AddColumnIfMissingAsync(connection, columns, "UserCases", "Title", "TEXT NOT NULL DEFAULT ''");
        await AddColumnIfMissingAsync(connection, columns, "UserCases", "Text", "TEXT NOT NULL DEFAULT ''");
        await AddColumnIfMissingAsync(connection, columns, "UserCases", "Status", "TEXT NOT NULL DEFAULT 'Open'");
        await AddColumnIfMissingAsync(connection, columns, "UserCases", "CreatedById", "TEXT NULL");
        await AddColumnIfMissingAsync(connection, columns, "UserCases", "TargetUserId", "TEXT NULL");
        await AddColumnIfMissingAsync(connection, columns, "UserCases", "AgentId", "TEXT NULL");
        await AddColumnIfMissingAsync(connection, columns, "UserCases", "CreatedAt", "TEXT NOT NULL DEFAULT '2026-01-01 00:00:00'");
    }

    private static async Task EnsureOrderCaseColumnsAsync(DbConnection connection)
    {
        if (!await TableExistsAsync(connection, "OrderCases")) return;
        var columns = await GetColumnsAsync(connection, "OrderCases");
        await AddColumnIfMissingAsync(connection, columns, "OrderCases", "Title", "TEXT NOT NULL DEFAULT ''");
        await AddColumnIfMissingAsync(connection, columns, "OrderCases", "Text", "TEXT NOT NULL DEFAULT ''");
        await AddColumnIfMissingAsync(connection, columns, "OrderCases", "Status", "TEXT NOT NULL DEFAULT 'Open'");
        await AddColumnIfMissingAsync(connection, columns, "OrderCases", "MarketItemId", "INTEGER NULL");
        await AddColumnIfMissingAsync(connection, columns, "OrderCases", "CreatedById", "TEXT NULL");
        await AddColumnIfMissingAsync(connection, columns, "OrderCases", "AgentId", "TEXT NULL");
        await AddColumnIfMissingAsync(connection, columns, "OrderCases", "CreatedAt", "TEXT NOT NULL DEFAULT '2026-01-01 00:00:00'");
    }

    private static async Task EnsureTableAsync(DbConnection connection, string tableName, string createSql)
    {
        if (await TableExistsAsync(connection, tableName))
        {
            return;
        }

        await using var command = connection.CreateCommand();
        command.CommandText = createSql;
        await command.ExecuteNonQueryAsync();
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
