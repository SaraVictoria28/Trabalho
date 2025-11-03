using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TrabalhoElvis2.Context;
using TrabalhoElvis2.Models;

namespace TrabalhoElvis2.Controllers
{
    public class FuncionarioController : Controller
    {
        private readonly LoginContext _ctx;
        public FuncionarioController(LoginContext ctx) => _ctx = ctx;

        public async Task<IActionResult> Index()
        {
            var lista = await _ctx.Funcionarios.ToListAsync();
            return View(lista);
        }

        [HttpGet]
        public IActionResult Cadastrar() => View();

        [HttpPost]
        public async Task<IActionResult> Cadastrar(Funcionario model)
        {
            if (!ModelState.IsValid) return View(model);
            _ctx.Funcionarios.Add(model);
            await _ctx.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        [HttpGet]
        public async Task<IActionResult> Editar(int id)
        {
            var f = await _ctx.Funcionarios.FindAsync(id);
            if (f == null) return NotFound();
            return View(f);
        }

        [HttpPost]
        public async Task<IActionResult> Editar(Funcionario model)
        {
            if (!ModelState.IsValid) return View(model);
            _ctx.Update(model);
            await _ctx.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        public async Task<IActionResult> Excluir(int id)
        {
            var f = await _ctx.Funcionarios.FindAsync(id);
            if (f != null)
            {
                _ctx.Funcionarios.Remove(f);
                await _ctx.SaveChangesAsync();
            }
            return RedirectToAction(nameof(Index));
        }
    }
}