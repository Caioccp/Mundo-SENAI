
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

        // PRIMEIROS PASSOS: um desafio concluído.
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

        // DESAFIANTE: dez desafios concluídos.
        if (totalDesafiosConcluidos >= 10)
        {
            await DesbloquearAsync(
                usuarioId,
                "desafiante_10",
                "Desafiante",
                "Você já concluiu dez desafios!",
                "🎯",
                conquistasDesbloqueadas);
        }

        // MARATONISTA: trinta desafios concluídos.
        if (totalDesafiosConcluidos >= 30)
        {
            await DesbloquearAsync(
                usuarioId,
                "maratonista_30",
                "Maratonista",
                "Você concluiu trinta desafios!",
                "🏆",
                conquistasDesbloqueadas);
        }

        // PRIMEIRO INVESTIMENTO: cinquenta reais guardados.
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

        // POUPADOR: duzentos e cinquenta reais guardados.
        if (totalGuardado >= 250)
        {
            await DesbloquearAsync(
                usuarioId,
                "poupador_250",
                "Poupador",
                "Você já guardou R$ 250 no total!",
                "🐷",
                conquistasDesbloqueadas);
        }

        // GRANDE ECONOMIZADOR: quinhentos reais guardados.
        if (totalGuardado >= 500)
        {
            await DesbloquearAsync(
                usuarioId,
                "grande_economizador_500",
                "Grande economizador",
                "Você já guardou R$ 500 no total!",
                "💎",
                conquistasDesbloqueadas);
        }

        // ESTUDANTE DEDICADO: quatro quizzes diferentes.
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

        // CONHECIMENTO AMPLO: seis quizzes diferentes.
        if (totalQuizzes >= 6)
        {
            await DesbloquearAsync(
                usuarioId,
                "conhecimento_amplo",
                "Conhecimento amplo",
                "Você concluiu seis quizzes diferentes!",
                "📚",
                conquistasDesbloqueadas);
        }

        // BIBLIOTECA COMPLETA: oito quizzes diferentes.
        if (totalQuizzes >= 8)
        {
            await DesbloquearAsync(
                usuarioId,
                "biblioteca_completa",
                "Biblioteca completa",
                "Você concluiu os oito quizzes disponíveis!",
                "🎓",
                conquistasDesbloqueadas);
        }

        // APRENDIZ: cinquenta pontos.
        if (pontuacao >= 50)
        {
            await DesbloquearAsync(
                usuarioId,
                "aprendiz",
                "Aprendiz",
                "Você alcançou 50 XP!",
                "⭐",
                conquistasDesbloqueadas);
        }

        // ORGANIZADO: cento e cinquenta pontos.
        if (pontuacao >= 150)
        {
            await DesbloquearAsync(
                usuarioId,
                "organizado",
                "Organizado",
                "Você alcançou 150 XP!",
                "🥇",
                conquistasDesbloqueadas);
        }

        // MESTRE FINANCEIRO: quinhentos pontos.
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
