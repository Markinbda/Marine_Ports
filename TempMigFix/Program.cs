using Npgsql;

var connStr = "Host=aws-1-us-west-2.pooler.supabase.com;Port=5432;Database=postgres;Username=postgres.xfdlknhredtfohdyhbzx;Password=W@rw1ckacad62!;SSL Mode=Require;Trust Server Certificate=true";

await using var conn = new NpgsqlConnection(connStr);
await conn.OpenAsync();
Console.WriteLine("Connected to Supabase.");

// First check what columns exist
Console.WriteLine("\nCurrent columns in Users table:");
await using (var cmd = new NpgsqlCommand("""
    SELECT column_name, data_type 
    FROM information_schema.columns 
    WHERE table_name = 'Users' 
    ORDER BY ordinal_position
    """, conn))
await using (var reader = await cmd.ExecuteReaderAsync())
    while (await reader.ReadAsync())
        Console.WriteLine($"  {reader.GetString(0)} ({reader.GetString(1)})");

// Fix: ensure FirstName/LastName exist and FullName is gone
var statements = new[]
{
    """ALTER TABLE "Users" ADD COLUMN IF NOT EXISTS "FirstName" text NOT NULL DEFAULT ''""",
    """ALTER TABLE "Users" ADD COLUMN IF NOT EXISTS "LastName" text NOT NULL DEFAULT ''""",
    """ALTER TABLE "Users" DROP COLUMN IF EXISTS "FullName" """,
};

foreach (var sql in statements)
{
    await using var cmd = new NpgsqlCommand(sql, conn);
    var affected = await cmd.ExecuteNonQueryAsync();
    Console.WriteLine($"OK: {sql[..Math.Min(70, sql.Length)]}");
}

Console.WriteLine("All done.");


