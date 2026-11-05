
using Microsoft.EntityFrameworkCore;
using StartFin.Dados;
using StartFin.Modelos;

namespace StartFin.Servicos;

public class ConquistaServico
{
    private readonly StartFinContext _context;

    public ConquistaServico(StartFinContext context)
    {
        _context = context;
    }

    public async Task VerificarConquistasAsync(
        string usuarioId,
        int pontuacao)
    {
        var conquistasDesbloqueadas = await _context.Conquistas
            .Where(c => c.UsuarioId == usuarioId)
            .Select(c => c.Codigo)
            .ToListAsync();

        var totalDesafiosConcluidos = await _context.Desafios
            .CountAsync(d =>
                d.UsuarioId == usuarioId &&
                d.Concluido);

        var totalGuardado = await _context.Movimentacoes
            .Where(m =>
                m.Carteira.UsuarioId == usuarioId &&
                m.Tipo == "Saída" &&
                m.Descricao.StartsWith(
                    "Valor guardado na meta:"))
            .SumAsync(m => (decimal?)m.Valor) ?? 0;

        var totalQuizzes = await _context.ResultadosQuiz
            .Where(r => r.UsuarioId == usuarioId)
            .Select(r => r.IdentificadorQuiz)
            .Distinct()
            .CountAsync();

        // Primeiros passos
        if (totalDesafiosConcluidos >= 1)
        {
            await DesbloquearAsync(
                usuarioId,
                "primeiros_passos",
                "Primeiros passos",
                "Você concluiu seu primeiro desafio!",
                "🌱",
                conquistasDesbloqueadas);
        }

        // Primeiro investimento
        if (totalGuardado >= 50)
        {
            await DesbloquearAsync(
                usuarioId,
                "primeiro_investimento",
                "Primeiro investimento",
                "Você já guardou R$ 50 no total!",
                "💰",
                conquistasDesbloqueadas);
        }

        // Estudante dedicado
        if (totalQuizzes >= 4)
        {
            await DesbloquearAsync(
                usuarioId,
                "estudante_dedicado",
                "Estudante dedicado",
                "Você concluiu quatro quizzes diferentes!",
                "🧠",
                conquistasDesbloqueadas);
        }

        // Aprendiz
        if (pontuacao >= 50)
        {
            await DesbloquearAsync(
                usuarioId,
                "aprendiz",
                "Aprendiz",
                "Você alcançou o nível 2!",
                "⭐",
                conquistasDesbloqueadas);
        }

        // Mestre financeiro
        if (pontuacao >= 500)
        {
            await DesbloquearAsync(
                usuarioId,
                "mestre_financeiro",
                "Mestre financeiro",
                "Você alcançou o nível máximo!",
                "👑",
                conquistasDesbloqueadas);
        }
    }

    private async Task DesbloquearAsync(
        string usuarioId,
        string codigo,
        string nome,
        string descricao,
        string icone,
        List<string> conquistasDesbloqueadas)
    {
        if (conquistasDesbloqueadas.Contains(codigo))
            return;

        _context.Conquistas.Add(new Conquista
        {
            UsuarioId = usuarioId,
            Codigo = codigo,
            Nome = nome,
            Descricao = descricao,
            Icone = icone,
            DataDesbloqueio = DateTime.UtcNow
        });

        await _context.SaveChangesAsync();

        conquistasDesbloqueadas.Add(codigo);
    }
}
