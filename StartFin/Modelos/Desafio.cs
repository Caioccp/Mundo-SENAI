
namespace StartFin.Modelos;

public class Desafio
{
    public int Id { get; set; }

    public string Titulo { get; set; } = string.Empty;

    public string Descricao { get; set; } = string.Empty;

    // Diário, Semanal ou Mensal.
    public string Tipo { get; set; } = string.Empty;

    // Fácil, Médio ou Difícil.
    public string Dificuldade { get; set; } = "Fácil";

    // Identifica qual ação será verificada.
    public string Codigo { get; set; } = string.Empty;

    // Pontuação recebida ao concluir.
    public int Pontos { get; set; }

    // Progresso necessário para concluir.
    public decimal MetaProgresso { get; set; }

    // Progresso realizado pelo usuário.
    public decimal ProgressoAtual { get; set; }

    public bool Concluido { get; set; }

    public DateTime? DataConclusao { get; set; }

    public DateTime DataInicio { get; set; }

    public DateTime DataFim { get; set; }

    public string UsuarioId { get; set; } = string.Empty;

    public Usuario? Usuario { get; set; }
}
