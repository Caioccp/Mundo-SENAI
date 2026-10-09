
namespace StartFin.Modelos;

public class Objetivo
{
    public int Id { get; set; }

    public string Nome { get; set; } = string.Empty;

    public decimal ValorMeta { get; set; }

    public decimal ValorGuardado { get; set; }

    public DateTime DataCriacao { get; set; } = DateTime.UtcNow;
}