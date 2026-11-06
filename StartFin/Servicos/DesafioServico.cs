
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

    // Catálogo de 30 desafios: 10 diários, 10 semanais e 10 mensais.
    private static readonly List<ModeloDesafio> Modelos = new()
    {
        // DIÁRIOS

        new("guardar_5_dia", "Guardar um pouquinho",
            "Guarde pelo menos R$ 5 em uma de suas metas.",
            "Diário", "Fácil", 5, 5),

        new("guardar_15_dia", "Economizador diário",
            "Guarde pelo menos R$ 15 em suas metas hoje.",
            "Diário", "Médio", 10, 15),

        new("quiz_1_dia", "Aprenda todos os dias",
            "Conclua pelo menos um quiz de educação financeira hoje.",
            "Diário", "Fácil", 10, 1),

        new("guardar_30_dia", "Disciplina financeira",
            "Guarde R$ 30 ou mais em suas metas durante o dia.",
            "Diário", "Difícil", 20, 30),

        new("quizzes_2_dia", "Estudante ativo",
            "Conclua dois quizzes diferentes no mesmo dia.",
            "Diário", "Difícil", 20, 2),

        new("quiz_9_dia", "Conhecimento avançado",
            "Consiga pelo menos 9 acertos em um quiz hoje.",
            "Diário", "Difícil", 25, 1),

        new("guardar_10_dia", "Pequena conquista",
            "Guarde pelo menos R$ 10 em suas metas hoje.",
            "Diário", "Fácil", 8, 10),

        new("guardar_20_dia", "Foco na economia",
            "Guarde pelo menos R$ 20 em suas metas hoje.",
            "Diário", "Médio", 15, 20),

        new("quiz_7_dia", "Reforçando o aprendizado",
            "Consiga pelo menos 7 acertos em um quiz hoje.",
            "Diário", "Médio", 15, 1),

        new("quiz_perfeito_dia", "Dia de excelência",
            "Consiga nota máxima em pelo menos um quiz hoje.",
            "Diário", "Difícil", 30, 1),

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

        new("guardar_200_semana", "Planejamento consistente",
            "Guarde R$ 200 ou mais durante esta semana.",
            "Semanal", "Difícil", 60, 200),

        new("quizzes_3_semana", "Semana de aprendizado",
            "Conclua três quizzes diferentes durante a semana.",
            "Semanal", "Difícil", 50, 3),

        new("quiz_perfeito_semana", "Excelência nos estudos",
            "Consiga nota máxima em pelo menos um quiz nesta semana.",
            "Semanal", "Difícil", 60, 1),

        new("guardar_75_semana", "Economia em evolução",
            "Guarde pelo menos R$ 75 durante esta semana.",
            "Semanal", "Médio", 30, 75),

        new("guardar_150_semana", "Meta semanal reforçada",
            "Guarde pelo menos R$ 150 durante esta semana.",
            "Semanal", "Difícil", 50, 150),

        new("quizzes_5_semana", "Dedicação aos estudos",
            "Conclua cinco quizzes diferentes durante esta semana.",
            "Semanal", "Difícil", 70, 5),

        new("quizzes_perfeitos_2_semana", "Perfeição em dobro",
            "Consiga nota máxima em dois quizzes diferentes nesta semana.",
            "Semanal", "Difícil", 80, 2),

        // MENSAIS

        new("guardar_150_mes", "Planejador financeiro",
            "Guarde R$ 150 durante este mês.",
            "Mensal", "Médio", 50, 150),

        new("guardar_300_mes", "Mestre da economia",
            "Guarde R$ 300 durante este mês.",
            "Mensal", "Difícil", 100, 300),

        new("quizzes_4_mes", "Maratona do conhecimento",
            "Conclua quatro quizzes diferentes neste mês.",
            "Mensal", "Difícil", 100, 4),

        new("guardar_500_mes", "Grande economizador",
            "Guarde R$ 500 ou mais durante este mês.",
            "Mensal", "Difícil", 150, 500),

        new("quizzes_8_mes", "Especialista financeiro",
            "Conclua oito quizzes diferentes durante o mês.",
            "Mensal", "Difícil", 120, 8),

        new("quizzes_perfeitos_3_mes", "Mente brilhante",
            "Consiga nota máxima em três quizzes diferentes neste mês.",
            "Mensal", "Difícil", 180, 3),

        new("guardar_750_mes", "Economizador de elite",
            "Guarde pelo menos R$ 750 durante este mês.",
            "Mensal", "Difícil", 200, 750),

        new("guardar_1000_mes", "Mil reais economizados",
            "Guarde pelo menos R$ 1.000 durante este mês.",
            "Mensal", "Difícil", 250, 1000),

        new("quizzes_5_mes", "Conhecimento em expansão",
            "Conclua cinco quizzes diferentes neste mês.",
            "Mensal", "Médio", 80, 5),

        new("quizzes_perfeitos_5_mes", "Gênio financeiro",
            "Consiga nota máxima em cinco quizzes diferentes neste mês.",
            "Mensal", "Difícil", 250, 5)
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
            // DESAFIOS DE ECONOMIA

            case "guardar_5_dia":
            case "guardar_10_dia":
            case "guardar_15_dia":
            case "guardar_20_dia":
            case "guardar_30_dia":
            case "guardar_50_semana":
            case "guardar_75_semana":
            case "guardar_100_semana":
            case "guardar_150_semana":
            case "guardar_200_semana":
            case "guardar_150_mes":
            case "guardar_300_mes":
            case "guardar_500_mes":
            case "guardar_750_mes":
            case "guardar_1000_mes":

                return await _context.Movimentacoes
                    .Where(m =>
                        m.Carteira.UsuarioId == usuarioId &&
                        m.Tipo == "Saída" &&
                        m.Descricao.StartsWith(
                            "Valor guardado na meta:") &&
                        m.Data >= inicio &&
                        m.Data < fim)
                    .SumAsync(m => (decimal?)m.Valor) ?? 0;

            // QUIZZES CONCLUÍDOS NO DIA

            case "quiz_1_dia":
            case "quiz_7_dia":
            case "quiz_9_dia":
            case "quiz_perfeito_dia":

                return await _context.ResultadosQuiz
                    .Where(r =>
                        r.UsuarioId == usuarioId &&
                        r.DataConclusao >= inicio &&
                        r.DataConclusao < fim &&
                        (
                            desafio.Codigo == "quiz_1_dia" ||
                            (desafio.Codigo == "quiz_7_dia" &&
                             r.TotalPerguntas == 10 &&
                             r.Pontos >= 7) ||
                            (desafio.Codigo == "quiz_9_dia" &&
                             r.TotalPerguntas == 10 &&
                             r.Pontos >= 9) ||
                            (desafio.Codigo == "quiz_perfeito_dia" &&
                             r.TotalPerguntas > 0 &&
                             r.Pontos == r.TotalPerguntas)
                        ))
                    .AnyAsync()
                    ? 1 : 0;

            // DOIS QUIZZES DIFERENTES NO DIA

            case "quizzes_2_dia":

                return await _context.ResultadosQuiz
                    .Where(r =>
                        r.UsuarioId == usuarioId &&
                        r.DataConclusao >= inicio &&
                        r.DataConclusao < fim)
                    .Select(r => r.IdentificadorQuiz)
                    .Distinct()
                    .CountAsync();

            // QUIZZES COM PELO MENOS 8 ACERTOS NA SEMANA

            case "quiz_8_semana":

                return await _context.ResultadosQuiz
                    .AnyAsync(r =>
                        r.UsuarioId == usuarioId &&
                        r.TotalPerguntas == 10 &&
                        r.Pontos >= 8 &&
                        r.DataConclusao >= inicio &&
                        r.DataConclusao < fim)
                    ? 1 : 0;

            // TRÊS OU CINCO QUIZZES DIFERENTES NA SEMANA

            case "quizzes_3_semana":
            case "quizzes_5_semana":

                return await _context.ResultadosQuiz
                    .Where(r =>
                        r.UsuarioId == usuarioId &&
                        r.DataConclusao >= inicio &&
                        r.DataConclusao < fim)
                    .Select(r => r.IdentificadorQuiz)
                    .Distinct()
                    .CountAsync();

            // NOTA MÁXIMA EM UM OU DOIS QUIZZES NA SEMANA

            case "quiz_perfeito_semana":
            case "quizzes_perfeitos_2_semana":

                return await _context.ResultadosQuiz
                    .Where(r =>
                        r.UsuarioId == usuarioId &&
                        r.TotalPerguntas > 0 &&
                        r.Pontos == r.TotalPerguntas &&
                        r.DataConclusao >= inicio &&
                        r.DataConclusao < fim)
                    .Select(r => r.IdentificadorQuiz)
                    .Distinct()
                    .CountAsync();

            // QUIZZES DIFERENTES NO MÊS

            case "quizzes_4_mes":
            case "quizzes_5_mes":
            case "quizzes_8_mes":

                return await _context.ResultadosQuiz
                    .Where(r =>
                        r.UsuarioId == usuarioId &&
                        r.DataConclusao >= inicio &&
                        r.DataConclusao < fim)
                    .Select(r => r.IdentificadorQuiz)
                    .Distinct()
                    .CountAsync();

            // QUIZZES DIFERENTES COM NOTA MÁXIMA NO MÊS

            case "quizzes_perfeitos_3_mes":
            case "quizzes_perfeitos_5_mes":

                return await _context.ResultadosQuiz
                    .Where(r =>
                        r.UsuarioId == usuarioId &&
                        r.TotalPerguntas > 0 &&
                        r.Pontos == r.TotalPerguntas &&
                        r.DataConclusao >= inicio &&
                        r.DataConclusao < fim)
                    .Select(r => r.IdentificadorQuiz)
                    .Distinct()
                    .CountAsync();

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
