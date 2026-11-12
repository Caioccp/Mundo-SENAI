
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace StartFin.Modelos;

public class ResultadoQuiz
{
    [Key]
    public int Id { get; set; }

    [Required]
    public string UsuarioId { get; set; } = string.Empty;

    [ForeignKey(nameof(UsuarioId))]
    public Usuario Usuario { get; set; } = null!;

    [Required]
    public string IdentificadorQuiz { get; set; } = "geral";

    // Pontuação alcançada na tentativa.
    public int Pontos { get; set; }

    public int TotalPerguntas { get; set; }

    // Em andamento ou Concluída.
    [Required]
    public string Status { get; set; } = "Concluída";

    // Índice da pergunta atual.
    public int IndicePerguntaAtual { get; set; }

    // Respostas salvas em formato JSON.
    public string? RespostasJson { get; set; }

    public DateTime DataInicio { get; set; } = DateTime.UtcNow;

    // Só recebe uma data quando o quiz é concluído.
    public DateTime? DataConclusao { get; set; }
}
