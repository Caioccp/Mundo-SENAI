
using StartFin.Components;
using StartFin.Dados;
using StartFin.Modelos;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using StartFin.Servicos;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddScoped<CarteiraServico>();
builder.Services.AddScoped<DesafioServico>();
builder.Services.AddScoped<ConquistaServico>();

builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();

builder.Services.AddCascadingAuthenticationState();

builder.Services.AddScoped<ConexaoBanco>();
builder.Services.AddScoped<PreferenciasNotificacaoServico>();

builder.Services.AddDbContext<StartFinContext>(options =>
    options.UseNpgsql(
        builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.AddIdentity<Usuario, IdentityRole>(options =>
{
    options.User.RequireUniqueEmail = false;

    options.Password.RequiredLength = 8;
    options.Password.RequireDigit = true;
    options.Password.RequireUppercase = true;
    options.Password.RequireLowercase = true;
    options.Password.RequireNonAlphanumeric = true;
})
.AddEntityFrameworkStores<StartFinContext>()
.AddDefaultTokenProviders()
.AddErrorDescriber<DescritorErrosIdentity>();

builder.Services.ConfigureApplicationCookie(options =>
{
    options.LoginPath = "/entrar";
    options.AccessDeniedPath = "/entrar";
});

var app = builder.Build();

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    app.UseHsts();
}

app.UseStatusCodePagesWithReExecute("/not-found");
app.UseHttpsRedirection();

app.UseAuthentication();

app.Use(async (context, next) =>
{
    var caminhosProtegidos = new[]
    {
        "/",
        "/metas",
        "/educacao-financeira"
    };

    if (caminhosProtegidos.Contains(context.Request.Path.Value)
        && context.User.Identity?.IsAuthenticated != true)
    {
        context.Response.Redirect("/entrar");
        return;
    }

    await next();
});

app.UseAuthorization();

app.UseAntiforgery();

app.MapPost("/login", async (
    HttpContext context,
    SignInManager<Usuario> signInManager,
    Microsoft.AspNetCore.Antiforgery.IAntiforgery antiforgery) =>
{
    await antiforgery.ValidateRequestAsync(context);

    var formulario = await context.Request.ReadFormAsync();

    var usuario = formulario["usuario"].ToString().Trim();
    var senha = formulario["senha"].ToString();

    await signInManager.SignOutAsync();
    
    var resultado = await signInManager.PasswordSignInAsync(
        usuario,
        senha,
        isPersistent: true,
        lockoutOnFailure: false);

    if (resultado.Succeeded)
    {
        return Results.Redirect("/");
    }

    return Results.Redirect("/entrar?erro=1");
});

// GERENCIAR SESSÃO
app.MapPost("/logout", async (
    HttpContext context,
    SignInManager<Usuario> signInManager) =>
{
    await signInManager.SignOutAsync();

    var destino = context.Request.Query["destino"].ToString();

    if (destino == "cadastro")
    {
        return Results.Redirect("/cadastro");
    }

    return Results.Redirect("/entrar");
});

// EXCLUIR CONTA
app.MapPost("/excluir-conta", async (
    HttpContext context,
    UserManager<Usuario> userManager,
    SignInManager<Usuario> signInManager,
    StartFinContext db,
    Microsoft.AspNetCore.Antiforgery.IAntiforgery antiforgery) =>
{
    await antiforgery.ValidateRequestAsync(context);

    if (context.User.Identity?.IsAuthenticated != true)
        return Results.Redirect("/entrar");

    var formulario = await context.Request.ReadFormAsync();

    var senha = formulario["senha"].ToString();
    var confirmacao = formulario["confirmacao"].ToString();

    if (confirmacao != "EXCLUIR" || string.IsNullOrWhiteSpace(senha))
        return Results.Redirect("/minha-conta?exclusao=confirmacao");

    var usuarioId = context.User.FindFirst(
        System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;

    if (string.IsNullOrWhiteSpace(usuarioId))
        return Results.Redirect("/minha-conta?exclusao=erro");

    var usuario = await userManager.FindByIdAsync(usuarioId);

    if (usuario == null)
        return Results.Redirect("/entrar");

    var senhaCorreta = await userManager.CheckPasswordAsync(usuario, senha);

    if (!senhaCorreta)
        return Results.Redirect("/minha-conta?exclusao=senha");

    await using var transacao = await db.Database.BeginTransactionAsync();

    try
    {
        // Desvincula os dependentes, sem excluir suas contas.
        var dependentes = await db.Users
            .Where(u => u.ResponsavelId == usuario.Id)
            .ToListAsync();

        foreach (var dependente in dependentes)
        {
            dependente.ResponsavelId = null;
        }

        await db.SaveChangesAsync();

        // Localiza as carteiras do usuário.
        var carteiras = await db.Carteiras
            .Where(c => c.UsuarioId == usuario.Id)
            .ToListAsync();

        var idsCarteiras = carteiras.Select(c => c.Id).ToList();

        // Exclui as movimentações antes das carteiras.
        var movimentacoes = await db.Movimentacoes
            .Where(m => idsCarteiras.Contains(m.CarteiraId))
            .ToListAsync();

        db.Movimentacoes.RemoveRange(movimentacoes);
        db.Carteiras.RemoveRange(carteiras);

        // Exclui os demais dados da conta.
        db.PreferenciasNotificacoes.RemoveRange(
            await db.PreferenciasNotificacoes
                .Where(p => p.UsuarioId == usuario.Id)
                .ToListAsync());

        db.ResultadosQuiz.RemoveRange(
            await db.ResultadosQuiz
                .Where(r => r.UsuarioId == usuario.Id)
                .ToListAsync());

        db.Desafios.RemoveRange(
            await db.Desafios
                .Where(d => d.UsuarioId == usuario.Id)
                .ToListAsync());

        db.Conquistas.RemoveRange(
            await db.Conquistas
                .Where(c => c.UsuarioId == usuario.Id)
                .ToListAsync());

        // Exclui somente as metas pertencentes a esta conta.
        await db.Database.ExecuteSqlInterpolatedAsync(
            $"DELETE FROM metas_economia WHERE usuario_id = {usuario.Id}");

        await db.SaveChangesAsync();

        // Exclui a conta do Identity.
        var resultado = await userManager.DeleteAsync(usuario);

        if (!resultado.Succeeded)
        {
            await transacao.RollbackAsync();
            return Results.Redirect("/minha-conta?exclusao=erro");
        }

        await transacao.CommitAsync();

        await signInManager.SignOutAsync();

        return Results.Redirect("/entrar?contaExcluida=1");
    }
    catch
    {
        await transacao.RollbackAsync();
        return Results.Redirect("/minha-conta?exclusao=erro");
    }
});

app.MapStaticAssets();

app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

app.Run();
