
using Npgsql;

namespace StartFin.Dados;

public class ConexaoBanco
{
    private readonly string _connectionString;

    public ConexaoBanco(IConfiguration configuration)
    {
        _connectionString = configuration.GetConnectionString("DefaultConnection")
            ?? throw new InvalidOperationException(
                "A conexão com o banco não foi configurada.");
    }

    public async Task<bool> TestarConexaoAsync()
    {
        await using var conexao = new NpgsqlConnection(_connectionString);
        await conexao.OpenAsync();

        return conexao.State == System.Data.ConnectionState.Open;
    }
}