using Microsoft.AspNetCore.Mvc;
using TrabalhoElvis2.Context;
using TrabalhoElvis2.Models;

namespace TrabalhoElvis2.Controllers
{
    public class CondominoController : Controller
    {
        private readonly LoginContext _context;

        public CondominoController(LoginContext context)
        {
            _context = context;
        }

        // === LISTAGEM COM FILTRO ===
        public IActionResult Index(string tipo = "Proprietario")
        {
            ViewBag.TipoAtual = tipo;
            var condominos = _context.Condominos
                .Where(c => c.Tipo == tipo)
                .ToList();

            ViewBag.Imoveis = _context.Imoveis.ToList(); // puxa imóveis pro modal
            return View(condominos);
        }

        [HttpPost]
        public IActionResult Cadastrar(Condomino cond)
        {
            if (ModelState.IsValid)
            {
                _context.Condominos.Add(cond);
                _context.SaveChanges();

                // Atualiza imóveis se for proprietário
                if (cond.Tipo == "Proprietario")
                {
                    cond.QtdeImoveis = _context.Imoveis.Count(i => i.CondominoId == cond.Id);
                    _context.SaveChanges();
                }

                return RedirectToAction("Index", new { tipo = cond.Tipo });
            }

            ViewBag.Imoveis = _context.Imoveis.ToList();
            return View("Index", _context.Condominos.ToList());
        }
    }
}
