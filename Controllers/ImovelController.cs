using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using TrabalhoElvis2.Context;
using TrabalhoElvis2.Models;

namespace TrabalhoElvis2.Controllers
{
    public class ImovelController : Controller
    {
        private readonly LoginContext _ctx;
        public ImovelController(LoginContext ctx) => _ctx = ctx;

        public async Task<IActionResult> Index()
        {
            var lista = await _ctx.Imoveis
                .Include(i => i.Condomino)
                .ToListAsync();
            return View(lista);
        }

        [HttpGet]
        public IActionResult Cadastrar()
        {
            ViewBag.Condominos = new SelectList(_ctx.Condominos.OrderBy(c => c.NomeCompleto).ToList(), "Id", "NomeCompleto");
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Cadastrar(Imovel model)
        {
            if (!ModelState.IsValid)
            {
                ViewBag.Condominos = new SelectList(_ctx.Condominos.OrderBy(c => c.NomeCompleto).ToList(), "Id", "NomeCompleto");
                return View(model);
            }
            _ctx.Imoveis.Add(model);
            await _ctx.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        [HttpGet]
        public async Task<IActionResult> Editar(int id)
        {
            var i = await _ctx.Imoveis.FindAsync(id);
            if (i == null) return NotFound();
            ViewBag.Condominos = new SelectList(_ctx.Condominos.OrderBy(c => c.NomeCompleto).ToList(), "Id", "NomeCompleto", i.CondominoId);
            return View(i);
        }

        [HttpPost]
        public async Task<IActionResult> Editar(Imovel model)
        {
            if (!ModelState.IsValid)
            {
                ViewBag.Condominos = new SelectList(_ctx.Condominos.OrderBy(c => c.NomeCompleto).ToList(), "Id", "NomeCompleto", model.CondominoId);
                return View(model);
            }
            _ctx.Update(model);
            await _ctx.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        public async Task<IActionResult> Excluir(int id)
        {
            var i = await _ctx.Imoveis.FindAsync(id);
            if (i != null)
            {
                _ctx.Imoveis.Remove(i);
                await _ctx.SaveChangesAsync();
            }
            return RedirectToAction(nameof(Index));
        }
    }
}