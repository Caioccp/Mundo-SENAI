namespace StartFin.Modelos;

public class Desafio
{
    public int Id { get; set; }

    public string Titulo { get; set; } = string.Empty;

    public string Descricao { get; set; } = string.Empty;

    public string Tipo { get; set; } = string.Empty;

    public int Pontos { get; set; }

    public bool Concluido { get; set; }

    public DateTime? DataConclusao { get; set; }

    public DateTime DataInicio { get; set; }

    public DateTime DataFim { get; set; }

    public string UsuarioId { get; set; } = string.Empty;

    public Usuario? Usuario { get; set; }
}