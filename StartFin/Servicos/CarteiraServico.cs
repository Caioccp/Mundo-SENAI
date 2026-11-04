using Microsoft.EntityFrameworkCore;
using StartFin.Dados;
using StartFin.Modelos;

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

        var movimentacao = new Movimentacao
        {
            CarteiraId = carteira.Id,
            Tipo = tipo,
            Descricao = descricao,
            Valor = valor,
            Data = DateTime.UtcNow
        };

        _context.Movimentacoes.Add(movimentacao);

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
}