using System;
using System.IO;
using Microsoft.Data.Sqlite;

var dbPath = Path.Combine(Directory.GetCurrentDirectory(), "..", "..", "ContaCorrente.Api", "contacorrente.db");
var full = Path.GetFullPath(dbPath);
Console.WriteLine($"DB path: {full}");

using var conn = new SqliteConnection($"Data Source={full}");
conn.Open();

var numero = args.Length > 0 ? args[0] : "59709548";
var nome = args.Length > 1 ? args[1] : "Ana123";
var id = args.Length > 2 && int.TryParse(args[2], out var parsedId) ? parsedId : (int?)null;
var ativo = args.Length > 3 && int.TryParse(args[3], out var parsedAtivo) ? parsedAtivo : 1;

var cmd = conn.CreateCommand();
cmd.Parameters.AddWithValue("$n", numero);
cmd.Parameters.AddWithValue("$nm", nome);
cmd.Parameters.AddWithValue("$ativo", ativo);

int rows;
if (id.HasValue)
{
    cmd.Parameters.AddWithValue("$id", id.Value);
    cmd.CommandText = "INSERT OR REPLACE INTO contacorrente (id, numero_conta, nome, ativo) VALUES ($id, $n, $nm, $ativo);";
    rows = cmd.ExecuteNonQuery();
    Console.WriteLine($"Upserted rows: {rows} (id={id.Value}, ativo={ativo})");
}
else
{
    cmd.CommandText = "INSERT OR IGNORE INTO contacorrente (numero_conta, nome, ativo) VALUES ($n, $nm, $ativo);";
    rows = cmd.ExecuteNonQuery();
    Console.WriteLine($"Inserted rows: {rows}");
}

// show inserted row
cmd.CommandText = "SELECT id, numero_conta, nome, ativo FROM contacorrente WHERE numero_conta = $n;";
using var reader = cmd.ExecuteReader();
while(reader.Read()){
    Console.WriteLine($"id={reader.GetInt32(0)} numero_conta={reader.GetString(1)} nome={reader.GetString(2)} ativo={reader.GetInt32(3)}");
}
