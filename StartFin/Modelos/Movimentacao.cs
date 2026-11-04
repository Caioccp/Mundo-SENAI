namespace StartFin.Modelos;

public class Movimentacao
{
    public int Id { get; set; }

    public int CarteiraId { get; set; }

    public Carteira Carteira { get; set; } = null!;

    public string Tipo { get; set; } = string.Empty;

    public string Descricao { get; set; } = string.Empty;

    public decimal Valor { get; set; }

    public DateTime Data { get; set; } = DateTime.UtcNow;
}