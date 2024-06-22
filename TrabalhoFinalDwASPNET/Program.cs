using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using TrabalhoFinalDwASPNET.Data;

var builder = WebApplication.CreateBuilder(args);

// Adiciona servi�os ao cont�iner de inje��o de depend�ncia.
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(connectionString));
builder.Services.AddDatabaseDeveloperPageExceptionFilter();

// Configura a autentica��o com Identity
builder.Services.AddDefaultIdentity<IdentityUser>(options => options.SignIn.RequireConfirmedAccount = true)
    .AddEntityFrameworkStores<ApplicationDbContext>();

// Configura controladores e views MVC
builder.Services.AddControllersWithViews();

var app = builder.Build();

// Configura o pipeline de requisi��o HTTP
if (app.Environment.IsDevelopment())
{
    app.UseMigrationsEndPoint(); // Endpoint para gerenciar migra��es de banco de dados em ambiente de desenvolvimento
}
else
{
    app.UseExceptionHandler("/Home/Error"); // Tratamento de exce��es para erros HTTP
    // O valor padr�o de HSTS � de 30 dias. Pode ser ajustado para cen�rios de produ��o.
    app.UseHsts(); // Adiciona o Strict-Transport-Security HTTP header para seguran�a avan�ada.
}

app.UseHttpsRedirection(); // Redireciona todas as requisi��es HTTP para HTTPS
app.UseStaticFiles(); // Habilita o uso de arquivos est�ticos como CSS, imagens, etc.

app.UseRouting(); // Define como as requisi��es HTTP s�o roteadas para os endpoints

app.UseAuthentication(); // Habilita a autentica��o para a aplica��o
app.UseAuthorization(); // Define pol�ticas de autoriza��o para acesso a recursos

// Define o roteamento padr�o para controladores e p�ginas Razor
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.MapRazorPages(); // Mapeia p�ginas Razor para suporte de p�ginas MVC Razor

app.Run(); // Inicia a aplica��o e executa o pipeline de requisi��o HTTP
