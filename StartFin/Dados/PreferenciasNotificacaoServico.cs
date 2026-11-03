using Microsoft.EntityFrameworkCore;
using StartFin.Modelos;

namespace StartFin.Dados;

public class PreferenciasNotificacaoServico
{
    private readonly StartFinContext _context;

    public PreferenciasNotificacaoServico(StartFinContext context)
    {
        _context = context;
    }

    public async Task<PreferenciasNotificacao> ObterAsync(string usuarioId)
    {
        var preferencias = await _context.PreferenciasNotificacoes
            .FirstOrDefaultAsync(p => p.UsuarioId == usuarioId);

        if (preferencias != null)
            return preferencias;

        preferencias = new PreferenciasNotificacao
        {
            UsuarioId = usuarioId
        };

        _context.PreferenciasNotificacoes.Add(preferencias);
        await _context.SaveChangesAsync();

        return preferencias;
    }

    public async Task SalvarAsync(PreferenciasNotificacao preferencias)
    {
        _context.PreferenciasNotificacoes.Update(preferencias);
        await _context.SaveChangesAsync();
    }
}