using System.Net;
using System.Text;
using MySqlConnector;

var host   = Environment.GetEnvironmentVariable("DB_HOST") ?? "localhost";
var dbName = Environment.GetEnvironmentVariable("DB_NAME") ?? "myDB";
var dbPwd  = Environment.GetEnvironmentVariable("DB_PWD")  ?? "";
var cs = $"Server={host};Port=3306;Database={dbName};User ID=root;Password={dbPwd};AllowPublicKeyRetrieval=True";

var app = WebApplication.CreateBuilder(args).Build();

app.MapGet("/", async () =>
{
    var rows = new StringBuilder();
    await using var conn = new MySqlConnection(cs);
    await conn.OpenAsync();
    await using var cmd = new MySqlCommand("SELECT id, name, city FROM emp ORDER BY id", conn);
    await using var r = await cmd.ExecuteReaderAsync();
    while (await r.ReadAsync())
        rows.Append($"<tr><td>{r.GetInt32(0)}</td><td>{WebUtility.HtmlEncode(r.GetString(1))}</td><td>{WebUtility.HtmlEncode(r.GetString(2))}</td></tr>");

    var html = $@"<!DOCTYPE html>
<html lang='fr'><head><meta charset='utf-8'><title>EmpWeb</title>
<style>
body{{font-family:Arial,sans-serif;max-width:640px;margin:40px auto;padding:0 16px;color:#222}}
table{{border-collapse:collapse;width:100%;margin:16px 0}}
th,td{{border:1px solid #c9d3dd;padding:8px;text-align:left}}
th{{background:#1f3a5f;color:#fff}}
input,button{{padding:8px;margin:4px 4px 4px 0}}
</style></head><body>
<h1>Application .NET + MySQL</h1>
<p>Page web connectée au conteneur MySQL (hôte : <b>{WebUtility.HtmlEncode(host)}</b>).</p>
<table><tr><th>id</th><th>name</th><th>city</th></tr>{rows}</table>
<form method='post' action='/add'>
<input name='name' placeholder='Nom' required>
<input name='city' placeholder='Ville' required>
<button type='submit'>Ajouter</button>
</form></body></html>";
    return Results.Content(html, "text/html; charset=utf-8");
});

app.MapPost("/add", async (HttpContext ctx) =>
{
    var form = await ctx.Request.ReadFormAsync();
    await using var conn = new MySqlConnection(cs);
    await conn.OpenAsync();
    await using var cmd = new MySqlCommand(
        "INSERT INTO emp (id, name, city) SELECT COALESCE(MAX(id), 0) + 1, @name, @city FROM emp", conn);
    cmd.Parameters.AddWithValue("@name", form["name"].ToString());
    cmd.Parameters.AddWithValue("@city", form["city"].ToString());
    await cmd.ExecuteNonQueryAsync();
    return Results.Redirect("/");
});

app.Run("http://0.0.0.0:8080");
