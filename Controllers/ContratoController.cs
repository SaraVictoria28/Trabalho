using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TrabalhoElvis2.Context;
using TrabalhoElvis2.Models;
using Trabalho.Models;

namespace TrabalhoElvis2.Controllers
{
    public class ContratoController : Controller
    {
        private readonly LoginContext _context;

        public ContratoController(LoginContext context)
        {
            _context = context;
        }

        public IActionResult Index()
        {
            var tipo = HttpContext.Session.GetString("TipoUsuario");
            if (tipo != "Administrador")
            {
                TempData["MensagemErro"] = "Acesso restrito a administradores.";
                return RedirectToAction("Login", "Usuario");
            }

            var contratos = _context.Contratos
                .Include(c => c.Imovel)
                .Include(c => c.Condomino)
                .ToList();

            ViewBag.Imoveis = _context.Imoveis.ToList();
            ViewBag.Condominos = _context.Condominos.ToList();

            return View(contratos);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Cadastrar(Contrato contrato)
        {
            // var tipo = HttpContext.Session.GetString("TipoUsuario");
            // if (tipo != "Administrador")
            // {
            //     TempData["MensagemErro"] = "Acesso restrito a administradores.";
            //     return RedirectToAction("Login", "Usuario");
            // }

            if (ModelState.IsValid)
            {
                _context.Contratos.Add(contrato);
                _context.SaveChanges();

                TempData["MensagemSucesso"] = "Contrato cadastrado com sucesso!";
                return RedirectToAction("Index");
            }

            TempData["MensagemErro"] = "Erro ao cadastrar o contrato.";
            return RedirectToAction("Index");
        }
    }
}
