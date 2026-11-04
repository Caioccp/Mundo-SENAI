
namespace StartFin.Models
{
    public class Carteira
    {
        public int Id { get; set; }

        public int UsuarioId { get; set; }

        public decimal Saldo { get; set; }

        public decimal SaldoReservado { get; set; }

        public DateTime CriadaEm { get; set; } = DateTime.Now;

        public decimal SaldoDisponivel
        {
            get { return Saldo - SaldoReservado; }
        }
    }
}