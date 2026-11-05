
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

    // Hash do código secreto de recuperação de senha.
    public string? CodigoRecuperacaoSenhaHash { get; set; }

    // Hash do token temporário de redefinição de senha.
public string? TokenRecuperacaoSenhaHash { get; set; }

// Data e horário em que o token temporário expira.
public DateTime? TokenRecuperacaoSenhaExpiraEm { get; set; }
}
