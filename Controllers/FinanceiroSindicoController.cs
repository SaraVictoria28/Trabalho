using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TrabalhoElvis2.Context;

namespace Trabalho.Controllers
{
    public class FinanceiroSindicoController : Controller
    {
        private readonly LoginContext _ctx;

        public FinanceiroSindicoController(LoginContext ctx)
        {
            _ctx = ctx;
        }

        [HttpGet]
        public async Task<IActionResult> Index()
        {
            var usuarioId = HttpContext.Session.GetInt32("IdUsuario");
            if (usuarioId == null)
                return RedirectToAction("Login", "Usuario");

            // Busca os boletos SOMENTE do condomínio do síndico
            var boletos = await _ctx.Boletos
                .Include(b => b.Contrato)
                    .ThenInclude(c => c.Imovel)
                .Include(b => b.Contrato)
                    .ThenInclude(c => c.Condomino)
                .Select(b => new
                {
                    b.Id,
                    Imovel = b.Contrato.Imovel.Codigo,
                    Morador = b.Contrato.Condomino.NomeCompleto,
                    Vencimento = b.Vencimento,
                    Status = b.Status
                })
                .ToListAsync();

            return View("SindicoIndex", boletos);
        }
    }
}