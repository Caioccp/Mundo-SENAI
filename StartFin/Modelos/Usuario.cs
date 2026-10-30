using Microsoft.AspNetCore.Identity;

namespace StartFin.Modelos;

public class Usuario : IdentityUser
{
    public string NomeCompleto { get; set; } = string.Empty;
}