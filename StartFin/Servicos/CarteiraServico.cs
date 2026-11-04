
using Microsoft.EntityFrameworkCore;
using StartFin.Dados;
using StartFin.Modelos;
using Npgsql;
using Microsoft.EntityFrameworkCore.Storage;

namespace StartFin.Servicos;

public class CarteiraServico
{
    private readonly StartFinContext _context;

    public CarteiraServico(StartFinContext context)
    {
        _context = context;
    }

    public async Task<Carteira> ObterOuCriarCarteiraAsync(string usuarioId)
    {
        var carteira = await _context.Carteiras
            .FirstOrDefaultAsync(c => c.UsuarioId == usuarioId);

        if (carteira != null)
            return carteira;

        carteira = new Carteira
        {
            UsuarioId = usuarioId,
            Saldo = 0,
            SaldoReservado = 0
        };

        _context.Carteiras.Add(carteira);
        await _context.SaveChangesAsync();

        return carteira;
    }

    public async Task RegistrarMovimentacaoAsync(
        string usuarioId,
        string tipo,
        string descricao,
        decimal valor)
    {
        if (valor <= 0)
            throw new Exception("O valor deve ser maior que zero.");

        if (tipo != "Entrada" && tipo != "Saída")
            throw new Exception("Tipo de movimentação inválido.");

        var carteira = await ObterOuCriarCarteiraAsync(usuarioId);

        if (tipo == "Saída")
        {
            if (valor > carteira.SaldoDisponivel)
                throw new Exception("Saldo disponível insuficiente.");

            carteira.Saldo -= valor;
        }
        else
        {
            carteira.Saldo += valor;
        }

        _context.Movimentacoes.Add(new Movimentacao
        {
            CarteiraId = carteira.Id,
            Tipo = tipo,
            Descricao = descricao,
            Valor = valor,
            Data = DateTime.UtcNow
        });

        await _context.SaveChangesAsync();
    }

    public async Task<List<Movimentacao>> ListarMovimentacoesAsync(
        string usuarioId)
    {
        return await _context.Movimentacoes
            .Where(m => m.Carteira.UsuarioId == usuarioId)
            .OrderByDescending(m => m.Data)
            .ToListAsync();
    }

    public async Task GuardarNaMetaAsync(
        string usuarioId,
        int metaId,
        decimal valor)
    {
        if (valor <= 0)
            throw new Exception("Informe um valor maior que zero.");

        await using var transacao = await _context.Database.BeginTransactionAsync();

        try
        {
            var carteira = await _context.Carteiras
                .FirstOrDefaultAsync(c => c.UsuarioId == usuarioId);

            if (carteira == null)
                throw new Exception("Sua carteira ainda não foi criada.");

            if (valor > carteira.SaldoDisponivel)
                throw new Exception("Saldo disponível insuficiente.");

            var conexao = (NpgsqlConnection)_context.Database.GetDbConnection();

            await using var comando = new NpgsqlCommand(
                @"UPDATE metas_economia
                  SET valor_guardado = valor_guardado + @valor
                  WHERE id = @metaId
                    AND usuario_id = @usuarioId
                    AND valor_guardado + @valor <= valor_objetivo
                  RETURNING nome;",
                conexao,
                (NpgsqlTransaction)transacao.GetDbTransaction());

            comando.Parameters.AddWithValue("@valor", valor);
            comando.Parameters.AddWithValue("@metaId", metaId);
            comando.Parameters.AddWithValue("@usuarioId", usuarioId);

            var resultado = await comando.ExecuteScalarAsync();

            if (resultado == null)
                throw new Exception(
                    "Meta não encontrada ou valor maior que o necessário para completá-la.");

            var nomeMeta = resultado.ToString()!;

            carteira.Saldo -= valor;

            _context.Movimentacoes.Add(new Movimentacao
            {
                CarteiraId = carteira.Id,
                Tipo = "Saída",
                Descricao = $"Valor guardado na meta: {nomeMeta}",
                Valor = valor,
                Data = DateTime.UtcNow
            });

            await _context.SaveChangesAsync();
            await transacao.CommitAsync();
        }
        catch
        {
            await transacao.RollbackAsync();
            throw;
        }
    }
}
