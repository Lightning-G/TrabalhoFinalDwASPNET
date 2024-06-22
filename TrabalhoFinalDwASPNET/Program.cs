using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using TrabalhoFinalDwASPNET.Data;

var builder = WebApplication.CreateBuilder(args);

// Adiciona serviços ao contêiner de injeção de dependência.
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(connectionString));
builder.Services.AddDatabaseDeveloperPageExceptionFilter();

// Configura a autenticação com Identity
builder.Services.AddDefaultIdentity<IdentityUser>(options => options.SignIn.RequireConfirmedAccount = true)
    .AddEntityFrameworkStores<ApplicationDbContext>();

// Configura controladores e views MVC
builder.Services.AddControllersWithViews();

var app = builder.Build();

// Configura o pipeline de requisição HTTP
if (app.Environment.IsDevelopment())
{
    app.UseMigrationsEndPoint(); // Endpoint para gerenciar migrações de banco de dados em ambiente de desenvolvimento
}
else
{
    app.UseExceptionHandler("/Home/Error"); // Tratamento de exceções para erros HTTP
    // O valor padrão de HSTS é de 30 dias. Pode ser ajustado para cenários de produção.
    app.UseHsts(); // Adiciona o Strict-Transport-Security HTTP header para segurança avançada.
}

app.UseHttpsRedirection(); // Redireciona todas as requisições HTTP para HTTPS
app.UseStaticFiles(); // Habilita o uso de arquivos estáticos como CSS, imagens, etc.

app.UseRouting(); // Define como as requisições HTTP são roteadas para os endpoints

app.UseAuthentication(); // Habilita a autenticação para a aplicação
app.UseAuthorization(); // Define políticas de autorização para acesso a recursos

// Define o roteamento padrão para controladores e páginas Razor
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.MapRazorPages(); // Mapeia páginas Razor para suporte de páginas MVC Razor

app.Run(); // Inicia a aplicação e executa o pipeline de requisição HTTP
