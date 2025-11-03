using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TrabalhoElvis2.Context;
using TrabalhoElvis2.Models;

namespace TrabalhoElvis2.Controllers
{
    public class CondominoController : Controller
    {
        private readonly LoginContext _ctx;
        public CondominoController(LoginContext ctx) => _ctx = ctx;

        public async Task<IActionResult> Index()
        {
            var lista = await _ctx.Condominos
                .Include(c => c.Imoveis)
                .ToListAsync();
            return View(lista);
        }

        [HttpGet]
        public IActionResult Cadastrar() => View();

        [HttpPost]
        public async Task<IActionResult> Cadastrar(Condomino model)
        {
            if (!ModelState.IsValid) return View(model);
            _ctx.Condominos.Add(model);
            await _ctx.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        [HttpGet]
        public async Task<IActionResult> Editar(int id)
        {
            var c = await _ctx.Condominos.FindAsync(id);
            if (c == null) return NotFound();
            return View(c);
        }

        [HttpPost]
        public async Task<IActionResult> Editar(Condomino model)
        {
            if (!ModelState.IsValid) return View(model);
            _ctx.Update(model);
            await _ctx.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        public async Task<IActionResult> Excluir(int id)
        {
            var c = await _ctx.Condominos.FindAsync(id);
            if (c != null)
            {
                _ctx.Condominos.Remove(c);
                await _ctx.SaveChangesAsync();
            }
            return RedirectToAction(nameof(Index));
        }
    }
}