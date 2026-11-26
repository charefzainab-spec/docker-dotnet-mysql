using MySqlConnector;

var host   = Environment.GetEnvironmentVariable("DB_HOST") ?? "localhost";
var dbName = Environment.GetEnvironmentVariable("DB_NAME") ?? "myDB";
var dbPwd  = Environment.GetEnvironmentVariable("DB_PWD")  ?? "";

var cs = $"Server={host};Port=3306;Database={dbName};User ID=root;Password={dbPwd};AllowPublicKeyRetrieval=True";

MySqlConnection? conn = null;
for (int i = 1; i <= 10; i++)
{
    try
    {
        conn = new MySqlConnection(cs);
        await conn.OpenAsync();
        Console.WriteLine("Connexion OK a " + host);
        break;
    }
    catch (Exception e)
    {
        Console.WriteLine($"Tentative {i}/10 echouee: {e.Message}");
        conn = null;
        await Task.Delay(3000);
    }
}

if (conn == null)
{
    Console.WriteLine("Impossible de se connecter a MySQL.");
    return 1;
}

await using (conn)
{
    var insert = new MySqlCommand(
        "INSERT INTO emp (id, name, city) SELECT COALESCE(MAX(id), 0) + 1, @name, @city FROM emp", conn);
    insert.Parameters.AddWithValue("@name", "zainab-net");
    insert.Parameters.AddWithValue("@city", "Monastir");
    await insert.ExecuteNonQueryAsync();
    Console.WriteLine("Ligne inseree.");

    var select = new MySqlCommand("SELECT id, name, city FROM emp", conn);
    await using var reader = await select.ExecuteReaderAsync();
    while (await reader.ReadAsync())
        Console.WriteLine($"{reader.GetInt32(0)} | {reader.GetString(1)} | {reader.GetString(2)}");
}
return 0;
