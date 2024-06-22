using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;
using TrabalhoFinalDwASPNET.Models;

namespace TrabalhoFinalDwASPNET.Controllers
{
    // Controlador responsável por gerenciar as ações relacionadas às páginas principais e erros da aplicação
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;

        // Construtor que injeta o ILogger para logging
        public HomeController(ILogger<HomeController> logger)
        {
            _logger = logger;
        }

        // Ação para a página inicial (Index)
        public IActionResult Index()
        {
            return View(); // Retorna a view correspondente à página inicial
        }

        // Ação para a página de privacidade (Privacy)
        public IActionResult Privacy()
        {
            return View(); // Retorna a view correspondente à página de privacidade
        }

        // Ação para a página Sobre (Sobre)
        public IActionResult Sobre()
        {
            return View(); // Retorna a view correspondente à página Sobre
        }

        // Ação para lidar com erros na aplicação (Error)
        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            // Retorna a view de erro, passando um objeto ErrorViewModel com o ID do request ou um identificador de trace
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
