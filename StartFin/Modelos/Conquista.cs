
namespace StartFin.Modelos;

public class Conquista
{
    public int Id { get; set; }

    // Identificador único da conquista.
    public string Codigo { get; set; } = string.Empty;

    // Informações exibidas na interface.
    public string Nome { get; set; } = string.Empty;
    public string Descricao { get; set; } = string.Empty;
    public string Icone { get; set; } = string.Empty;

    // Data em que o usuário desbloqueou a conquista.
    public DateTime DataDesbloqueio { get; set; } = DateTime.UtcNow;

    // Usuário que conquistou a medalha.
    public string UsuarioId { get; set; } = string.Empty;
    public Usuario? Usuario { get; set; }
}
