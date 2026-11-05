
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

    options.User.AllowedUserNameCharacters =
    "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789-._@+ ";

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

// Configuração da conta exclusiva de Desenvolvedor
{
    const string nomeFuncao = "Desenvolvedor";

    var nomeUsuario = app.Configuration["Developer:Username"];
    var senha = app.Configuration["Developer:Password"];

    if (string.IsNullOrWhiteSpace(nomeUsuario) ||
        string.IsNullOrWhiteSpace(senha))
    {
        throw new InvalidOperationException(
            "Configure os dados da conta Desenvolvedor nos User Secrets.");
    }

    using var scope = app.Services.CreateScope();

    var gerenciadorUsuarios = scope.ServiceProvider
        .GetRequiredService<UserManager<Usuario>>();

    var gerenciadorFuncoes = scope.ServiceProvider
        .GetRequiredService<RoleManager<IdentityRole>>();

    if (!await gerenciadorFuncoes.RoleExistsAsync(nomeFuncao))
    {
        var resultadoFuncao = await gerenciadorFuncoes.CreateAsync(
            new IdentityRole(nomeFuncao));

        if (!resultadoFuncao.Succeeded)
        {
            throw new InvalidOperationException(
                "Não foi possível criar a função Desenvolvedor.");
        }
    }

    var usuario = await gerenciadorUsuarios.FindByNameAsync(nomeUsuario);

    var desenvolvedores = await gerenciadorUsuarios
        .GetUsersInRoleAsync(nomeFuncao);

    if (usuario == null)
    {
        if (desenvolvedores.Count > 0)
        {
            throw new InvalidOperationException(
                "Já existe uma conta de Desenvolvedor.");
        }

        usuario = new Usuario
        {
            UserName = nomeUsuario,
            NomeCompleto = "Desenvolvedor"
        };

        var resultadoCriacao = await gerenciadorUsuarios
            .CreateAsync(usuario, senha);

        if (!resultadoCriacao.Succeeded)
        {
            var erros = string.Join(
                "; ",
                resultadoCriacao.Errors.Select(e => e.Description));

            throw new InvalidOperationException(
                "Erro ao criar a conta Desenvolvedor: " + erros);
        }
    }
    else if (!await gerenciadorUsuarios.IsInRoleAsync(usuario, nomeFuncao))
    {
        throw new InvalidOperationException(
            "Esse nome de usuário já pertence a outra conta. Escolha outro.");
    }

    if (desenvolvedores.Any(d => d.Id != usuario.Id))
    {
        throw new InvalidOperationException(
            "Já existe outra conta de Desenvolvedor.");
    }

    if (!await gerenciadorUsuarios.IsInRoleAsync(usuario, nomeFuncao))
    {
        var resultadoFuncaoUsuario = await gerenciadorUsuarios
            .AddToRoleAsync(usuario, nomeFuncao);

        if (!resultadoFuncaoUsuario.Succeeded)
        {
            throw new InvalidOperationException(
                "Não foi possível atribuir a função Desenvolvedor.");
        }
    }
}

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


 // FINALIZAR RECUPERAÇÃO DE SENHA
app.MapPost("/finalizar-recuperacao", async (
    HttpContext context,
    UserManager<Usuario> userManager,
    IPasswordHasher<Usuario> passwordHasher,
    Microsoft.AspNetCore.Antiforgery.IAntiforgery antiforgery) =>
{
    await antiforgery.ValidateRequestAsync(context);

    var falha = Results.Redirect("/nova-senha?erro=1");

    var formulario = await context.Request.ReadFormAsync();

    var novaSenha = formulario["novaSenha"].ToString();
    var confirmarSenha = formulario["confirmarSenha"].ToString();

    if (string.IsNullOrWhiteSpace(novaSenha) ||
        novaSenha != confirmarSenha)
        return falha;

    // Recupera a autorização temporária.
    var cookie = context.Request.Cookies["StartFinRecuperacao"];

    if (string.IsNullOrWhiteSpace(cookie))
        return falha;

    var partes = cookie.Split(':', 2);

    if (partes.Length != 2)
        return falha;

    var usuario = await userManager.FindByIdAsync(partes[0]);
    var token = partes[1];

    if (usuario == null ||
        string.IsNullOrWhiteSpace(usuario.TokenRecuperacaoSenhaHash) ||
        usuario.TokenRecuperacaoSenhaExpiraEm == null)
        return falha;

    // Confere a validade do token temporário.
    if (usuario.TokenRecuperacaoSenhaExpiraEm.Value <= DateTime.UtcNow)
        return falha;

    var verificacao = passwordHasher.VerifyHashedPassword(
        usuario,
        usuario.TokenRecuperacaoSenhaHash,
        token);

    if (verificacao == PasswordVerificationResult.Failed)
        return falha;

    // Gera o token oficial do Identity para redefinir a senha.
    var tokenIdentity = await userManager.GeneratePasswordResetTokenAsync(usuario);

    var resultadoSenha = await userManager.ResetPasswordAsync(
        usuario,
        tokenIdentity,
        novaSenha);

    if (!resultadoSenha.Succeeded)
        return falha;

    // Invalida a autorização temporária após o uso.
    usuario.TokenRecuperacaoSenhaHash = null;
    usuario.TokenRecuperacaoSenhaExpiraEm = null;

    await userManager.UpdateAsync(usuario);

    context.Response.Cookies.Delete(
        "StartFinRecuperacao",
        new CookieOptions
        {
            Path = "/",
            Secure = context.Request.IsHttps,
            SameSite = SameSiteMode.Strict
        });

    return Results.Redirect("/entrar?senhaRedefinida=1");
});

// INICIAR RECUPERAÇÃO DE SENHA
app.MapPost("/iniciar-recuperacao", async (
    HttpContext context,
    UserManager<Usuario> userManager,
    IPasswordHasher<Usuario> passwordHasher,
    Microsoft.AspNetCore.Antiforgery.IAntiforgery antiforgery) =>
{
    await antiforgery.ValidateRequestAsync(context);

    var formulario = await context.Request.ReadFormAsync();

    var nomeUsuario = formulario["usuario"].ToString().Trim();
    var nomeCompleto = formulario["nomeCompleto"].ToString().Trim();
    var nascimentoTexto = formulario["dataNascimento"].ToString();
    var codigo = formulario["codigoRecuperacao"].ToString().Trim();

    // Todas as falhas levam à mesma resposta.
    var falha = Results.Redirect("/recuperar-senha?erro=1");

    if (string.IsNullOrWhiteSpace(nomeUsuario) ||
        string.IsNullOrWhiteSpace(nomeCompleto) ||
        string.IsNullOrWhiteSpace(codigo))
        return falha;

    if (!DateTime.TryParseExact(
        nascimentoTexto,
        "yyyy-MM-dd",
        System.Globalization.CultureInfo.InvariantCulture,
        System.Globalization.DateTimeStyles.None,
        out var nascimento))
        return falha;

        var usuario = await userManager.FindByNameAsync(nomeUsuario);

    if (usuario == null ||
        string.IsNullOrWhiteSpace(usuario.NomeCompleto) ||
        usuario.DataNascimento == null)
        return falha;

    // Confere nome completo e data de nascimento.
    if (!string.Equals(
        usuario.NomeCompleto.Trim(),
        nomeCompleto,
        StringComparison.OrdinalIgnoreCase))
        return falha;

    if (usuario.DataNascimento.Value.Date != nascimento.Date)
        return falha;

    // Calcula a idade usando a data cadastrada na conta.
    var hoje = DateTime.UtcNow.Date;
    var nascimentoConta = usuario.DataNascimento.Value.Date;

    if (nascimentoConta > hoje)
        return falha;

    var idade = hoje.Year - nascimentoConta.Year;

    if (nascimentoConta > hoje.AddYears(-idade))
        idade--;

    // CONTAS DE MAIORES DE 18 ANOS
    if (idade >= 18)
    {
        if (string.IsNullOrWhiteSpace(usuario.CodigoRecuperacaoSenhaHash))
            return falha;

        var verificacao = passwordHasher.VerifyHashedPassword(
            usuario,
            usuario.CodigoRecuperacaoSenhaHash,
            codigo);

        if (verificacao == PasswordVerificationResult.Failed)
            return falha;
    }
    // CONTAS DE MENORES DE 18 ANOS
    else
    {
        // Verifica se existe um responsável vinculado.
        if (string.IsNullOrWhiteSpace(usuario.ResponsavelId))
            return falha;

        var responsavel = await userManager.FindByIdAsync(usuario.ResponsavelId);

        if (responsavel == null ||
            responsavel.DataNascimento == null ||
            string.IsNullOrWhiteSpace(responsavel.CodigoVinculoFamiliar))
            return falha;

        // Confere se o responsável realmente tem 18 anos ou mais.
        var nascimentoResponsavel = responsavel.DataNascimento.Value.Date;
        var idadeResponsavel = hoje.Year - nascimentoResponsavel.Year;

        if (nascimentoResponsavel > hoje.AddYears(-idadeResponsavel))
            idadeResponsavel--;

        if (idadeResponsavel < 18 || nascimentoResponsavel > hoje)
            return falha;

        // Confere o código familiar do responsável vinculado.
        if (!string.Equals(
            responsavel.CodigoVinculoFamiliar,
            codigo,
            StringComparison.Ordinal))
            return falha;
    }
    // Gera um token temporário aleatório.
    var token = Convert.ToBase64String(
        System.Security.Cryptography.RandomNumberGenerator.GetBytes(32))
        .TrimEnd('=')
        .Replace('+', '-')
        .Replace('/', '_');

    usuario.TokenRecuperacaoSenhaHash =
        passwordHasher.HashPassword(usuario, token);

    usuario.TokenRecuperacaoSenhaExpiraEm =
        DateTime.UtcNow.AddMinutes(10);

    var resultado = await userManager.UpdateAsync(usuario);

    if (!resultado.Succeeded)
        return falha;

    // O token original fica somente em um cookie protegido contra JavaScript.
    context.Response.Cookies.Append(
        "StartFinRecuperacao",
        usuario.Id + ":" + token,
        new CookieOptions
        {
            HttpOnly = true,
            Secure = context.Request.IsHttps,
            SameSite = SameSiteMode.Strict,
            Expires = DateTimeOffset.UtcNow.AddMinutes(10),
            Path = "/"
        });

    return Results.Redirect("/nova-senha");
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
