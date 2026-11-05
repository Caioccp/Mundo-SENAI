
using Microsoft.EntityFrameworkCore;
using StartFin.Dados;
using StartFin.Modelos;

namespace StartFin.Servicos;

public class DesafioServico
{
    private readonly StartFinContext _context;

    public DesafioServico(StartFinContext context)
    {
        _context = context;
    }

    // Catálogo inicial de desafios.
    private static readonly List<ModeloDesafio> Modelos = new()
    {
        // DIÁRIOS
        new("guardar_5_dia", "Guardar um pouquinho",
            "Guarde pelo menos R$ 5 em uma de suas metas.",
            "Diário", "Fácil", 5, 5),

        new("guardar_15_dia", "Economizador diário",
            "Guarde pelo menos R$ 15 em suas metas.",
            "Diário", "Médio", 10, 15),

        new("quiz_1_dia", "Aprenda todos os dias",
            "Conclua pelo menos um quiz de educação financeira.",
            "Diário", "Fácil", 10, 1),

        // SEMANAIS
        new("guardar_50_semana", "Primeiro passo da semana",
            "Guarde R$ 50 ou mais durante esta semana.",
            "Semanal", "Fácil", 20, 50),

        new("guardar_100_semana", "Semana econômica",
            "Guarde R$ 100 durante esta semana.",
            "Semanal", "Médio", 35, 100),

        new("quiz_8_semana", "Conhecimento em prática",
            "Consiga pelo menos 8 acertos em um quiz nesta semana.",
            "Semanal", "Médio", 30, 1),

        // MENSAIS
        new("guardar_150_mes", "Planejador financeiro",
            "Guarde R$ 150 durante este mês.",
            "Mensal", "Médio", 50, 150),

        new("guardar_300_mes", "Mestre da economia",
            "Guarde R$ 300 durante este mês.",
            "Mensal", "Difícil", 100, 300),

        new("quizzes_4_mes", "Maratona do conhecimento",
            "Conclua os quatro quizzes diferentes neste mês.",
            "Mensal", "Difícil", 100, 4)
    };

    public async Task<List<Desafio>> CarregarDesafiosAsync(
        string usuarioId)
    {
        var hoje = DateTime.UtcNow.Date;

        var inicioDiario = hoje;
        var fimDiario = hoje.AddDays(1);

        var diasDesdeSegunda =
            ((int)hoje.DayOfWeek + 6) % 7;

        var inicioSemanal = hoje.AddDays(-diasDesdeSegunda);
        var fimSemanal = inicioSemanal.AddDays(7);

        var inicioMensal = new DateTime(
            hoje.Year, hoje.Month, 1, 0, 0, 0,
            DateTimeKind.Utc);

        var fimMensal = inicioMensal.AddMonths(1);

        foreach (var modelo in Modelos)
        {
            var (inicio, fim) = ObterPeriodo(
                modelo.Tipo,
                inicioDiario,
                fimDiario,
                inicioSemanal,
                fimSemanal,
                inicioMensal,
                fimMensal);

            var existe = await _context.Desafios.AnyAsync(d =>
                d.UsuarioId == usuarioId &&
                d.Codigo == modelo.Codigo &&
                d.DataInicio == inicio);

            if (!existe)
            {
                _context.Desafios.Add(new Desafio
                {
                    UsuarioId = usuarioId,
                    Codigo = modelo.Codigo,
                    Titulo = modelo.Titulo,
                    Descricao = modelo.Descricao,
                    Tipo = modelo.Tipo,
                    Dificuldade = modelo.Dificuldade,
                    Pontos = modelo.Pontos,
                    MetaProgresso = modelo.MetaProgresso,
                    ProgressoAtual = 0,
                    Concluido = false,
                    DataInicio = inicio,
                    DataFim = fim
                });
            }
        }

        await _context.SaveChangesAsync();

        var desafios = await _context.Desafios
            .Where(d =>
                d.UsuarioId == usuarioId &&
                (
                    (d.Tipo == "Diário" &&
                     d.DataInicio == inicioDiario) ||

                    (d.Tipo == "Semanal" &&
                     d.DataInicio == inicioSemanal) ||

                    (d.Tipo == "Mensal" &&
                     d.DataInicio == inicioMensal)
                ))
            .OrderBy(d => d.Tipo)
            .ThenBy(d => d.Id)
            .ToListAsync();

        foreach (var desafio in desafios)
        {
            if (desafio.Concluido)
                continue;

            var progresso = await CalcularProgressoAsync(
                usuarioId,
                desafio);

            desafio.ProgressoAtual = Math.Min(
                progresso,
                desafio.MetaProgresso);

            if (desafio.ProgressoAtual >= desafio.MetaProgresso)
            {
                desafio.Concluido = true;
                desafio.DataConclusao = DateTime.UtcNow;
            }
        }

        await _context.SaveChangesAsync();

        return desafios;
    }

    private async Task<decimal> CalcularProgressoAsync(
        string usuarioId,
        Desafio desafio)
    {
        var inicio = desafio.DataInicio;
        var fim = desafio.DataFim;

        switch (desafio.Codigo)
        {
            case "guardar_5_dia":
            case "guardar_15_dia":
            case "guardar_50_semana":
            case "guardar_100_semana":
            case "guardar_150_mes":
            case "guardar_300_mes":

                return await _context.Movimentacoes
                    .Where(m =>
                        m.Carteira.UsuarioId == usuarioId &&
                        m.Tipo == "Saída" &&
                        m.Descricao.StartsWith(
                            "Valor guardado na meta:") &&
                        m.Data >= inicio &&
                        m.Data < fim)
                    .SumAsync(m => (decimal?)m.Valor) ?? 0;

            case "quiz_1_dia":

                return await _context.ResultadosQuiz
                    .AnyAsync(r =>
                        r.UsuarioId == usuarioId &&
                        r.DataConclusao >= inicio &&
                        r.DataConclusao < fim)
                    ? 1 : 0;

            case "quiz_8_semana":

                return await _context.ResultadosQuiz
                    .AnyAsync(r =>
                        r.UsuarioId == usuarioId &&
                        r.TotalPerguntas == 10 &&
                        r.Pontos >= 8 &&
                        r.DataConclusao >= inicio &&
                        r.DataConclusao < fim)
                    ? 1 : 0;

            case "quizzes_4_mes":

                var identificadores = await _context.ResultadosQuiz
                    .Where(r =>
                        r.UsuarioId == usuarioId &&
                        r.DataConclusao >= inicio &&
                        r.DataConclusao < fim)
                    .Select(r => r.IdentificadorQuiz)
                    .Distinct()
                    .CountAsync();

                return identificadores;

            default:
                return 0;
        }
    }

    private static (DateTime inicio, DateTime fim) ObterPeriodo(
        string tipo,
        DateTime inicioDiario,
        DateTime fimDiario,
        DateTime inicioSemanal,
        DateTime fimSemanal,
        DateTime inicioMensal,
        DateTime fimMensal)
    {
        return tipo switch
        {
            "Diário" => (inicioDiario, fimDiario),
            "Semanal" => (inicioSemanal, fimSemanal),
            "Mensal" => (inicioMensal, fimMensal),
            _ => throw new InvalidOperationException(
                "Tipo de desafio inválido.")
        };
    }

    private record ModeloDesafio(
        string Codigo,
        string Titulo,
        string Descricao,
        string Tipo,
        string Dificuldade,
        int Pontos,
        decimal MetaProgresso);
}
