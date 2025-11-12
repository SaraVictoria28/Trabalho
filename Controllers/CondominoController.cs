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
        public IActionResult Index(string tipo = "Proprietario", string termoBusca = "")
        {
            ViewBag.TipoAtual = tipo;
            var query = _context.Condominos.Where(c => c.Tipo == tipo);
            if (!string.IsNullOrEmpty(termoBusca))
            {
                termoBusca = termoBusca.ToLower();
                query = query.Where(c =>
                    (c.NomeCompleto != null && c.NomeCompleto.ToLower().Contains(termoBusca)) ||
                    (c.CPF != null && c.CPF.ToLower().Contains(termoBusca)) ||
                    (c.Email != null && c.Email.ToLower().Contains(termoBusca))
                );
            }
            var condominos = query.ToList();
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
