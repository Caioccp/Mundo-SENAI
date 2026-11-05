
using StartFin.Components;
using StartFin.Dados;
using StartFin.Modelos;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using StartFin.Servicos;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddScoped<CarteiraServico>();
builder.Services.AddScoped<DesafioServico>();

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

app.MapStaticAssets();

app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

app.Run();
