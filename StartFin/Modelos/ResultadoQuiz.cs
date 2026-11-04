
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

    public int Pontos { get; set; }

    public int TotalPerguntas { get; set; }

    public DateTime DataConclusao { get; set; } = DateTime.UtcNow;
}
