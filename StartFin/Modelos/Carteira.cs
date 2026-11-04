namespace StartFin.Modelos;

public class Carteira
{
    public int Id { get; set; }

    public string UsuarioId { get; set; } = string.Empty;

    public Usuario Usuario { get; set; } = null!;

    public decimal Saldo { get; set; }

    public decimal SaldoReservado { get; set; }

    public DateTime CriadaEm { get; set; } = DateTime.UtcNow;

    public decimal SaldoDisponivel
    {
        get { return Saldo - SaldoReservado; }
    }
}