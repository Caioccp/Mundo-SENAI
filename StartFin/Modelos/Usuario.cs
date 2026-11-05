using Microsoft.AspNetCore.Identity;

namespace StartFin.Modelos;

public class Usuario : IdentityUser
{
    public string NomeCompleto { get; set; } = string.Empty;

    public DateTime? DataNascimento { get; set; }

    public DateTime DataCriacao { get; set; } = DateTime.UtcNow;
}