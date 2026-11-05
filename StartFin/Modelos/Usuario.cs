
using Microsoft.AspNetCore.Identity;

namespace StartFin.Modelos;

public class Usuario : IdentityUser
{
    public string NomeCompleto { get; set; } = string.Empty;

    public DateTime? DataNascimento { get; set; }

    public DateTime DataCriacao { get; set; } = DateTime.UtcNow;

    // Código que o responsável poderá fornecer para vincular familiares.
    public string? CodigoVinculoFamiliar { get; set; }

    // Identificação do responsável desta conta.
    public string? ResponsavelId { get; set; }

    public Usuario? Responsavel { get; set; }

    // Pessoas vinculadas a esta conta como responsável.
    public ICollection<Usuario> Dependentes { get; set; } = new List<Usuario>();
}
