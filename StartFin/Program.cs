using StartFin.Components;
using StartFin.Dados;
using StartFin.Modelos;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();

builder.Services.AddCascadingAuthenticationState();

builder.Services.AddScoped<ConexaoBanco>();

builder.Services.AddDbContext<StartFinContext>(options =>
    options.UseNpgsql(
        builder.Configuration.GetConnectionString("DefaultConnection")));

// Configuração do sistema de usuários
builder.Services.AddIdentity<Usuario, IdentityRole>(options =>
{
    options.User.RequireUniqueEmail = true;

    options.Password.RequiredLength = 8;
    options.Password.RequireDigit = true;
    options.Password.RequireUppercase = false;
    options.Password.RequireLowercase = true;
    options.Password.RequireNonAlphanumeric = false;
})
.AddEntityFrameworkStores<StartFinContext>()
.AddDefaultTokenProviders();

builder.Services.ConfigureApplicationCookie(options =>
{
    options.LoginPath = "/login";
    options.AccessDeniedPath = "/login";
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

    var email = formulario["email"].ToString();
    var senha = formulario["senha"].ToString();

    var resultado = await signInManager.PasswordSignInAsync(
        email,
        senha,
        isPersistent: true,
        lockoutOnFailure: false);

    if (resultado.Succeeded)
    {
        return Results.Redirect("/");
    }

    return Results.Redirect("/login?erro=1");
});

app.MapPost("/logout", async (
    SignInManager<Usuario> signInManager) =>
{
    await signInManager.SignOutAsync();

    return Results.Redirect("/entrar");
});

app.MapStaticAssets();
app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

app.Run();