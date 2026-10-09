
using Microsoft.EntityFrameworkCore;
using StartFin.Modelos;

namespace StartFin.Dados;

public class StartFinContext : DbContext
{
    public StartFinContext(
        DbContextOptions<StartFinContext> options)
        : base(options)
    {
    }

    public DbSet<Objetivo> Objetivos => Set<Objetivo>();
}