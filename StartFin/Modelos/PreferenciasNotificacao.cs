using System.ComponentModel.DataAnnotations;

namespace StartFin.Modelos;

public class PreferenciasNotificacao
{
    [Key]
    public int Id { get; set; }

    public string UsuarioId { get; set; } = string.Empty;

    public bool AvisosMetas { get; set; } = true;

    public bool NovidadesEducacao { get; set; } = true;

    public bool LembretesObjetivos { get; set; } = true;

    public bool ComunicadosGerais { get; set; } = true;
}