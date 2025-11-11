using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TrabalhoElvis2.Context;
using TrabalhoElvis2.Models;
using TrabalhoElvis2.Services;

namespace TrabalhoElvis2.Controllers
{
    public class FinanceiroAdmController : Controller
    {
        private readonly LoginContext _ctx;
        private readonly PixService _pixService;

        public FinanceiroAdmController(LoginContext ctx, PixService pixService)
        {
            _ctx = ctx;
            _pixService = pixService;
        }

        // Página principal do financeiro
        public async Task<IActionResult> Index(string filtro = "Todos")
        {
            var boletos = await _ctx.Boletos
                .Include(b => b.Contrato)
                    .ThenInclude(c => c.Imovel)
                .Include(b => b.Contrato)
                    .ThenInclude(c => c.Condomino)
                .ToListAsync();

            // Filtro de status
            if (filtro != "Todos")
                boletos = boletos.Where(b => b.Status == filtro).ToList();

            // Cálculos de valores
            var receitaPrevista = boletos.Sum(b => b.Valor);
            var receitaRecebida = boletos.Where(b => b.Status == "Pago").Sum(b => b.Valor);
            var inadimplencia = boletos.Where(b => b.Status == "Vencido").Sum(b => b.Valor);

            ViewBag.ReceitaPrevista = receitaPrevista;
            ViewBag.ReceitaRecebida = receitaRecebida;
            ViewBag.Inadimplencia = inadimplencia;

            return View(boletos);
        }

        // Gera QR Code PIX para um boleto específico
        [HttpPost]
        public async Task<IActionResult> GerarQrCode(int id)
        {
            var boleto = await _ctx.Boletos
                .Include(b => b.Contrato)
                    .ThenInclude(c => c.Condomino)
                .FirstOrDefaultAsync(b => b.Id == id);

            if (boleto == null)
                return NotFound();

            var nome = boleto.Contrato?.Condomino?.NomeCompleto ?? "Condomino";
            var valor = boleto.Valor;
            var descricao = $"Pagamento condomínio - {nome} - {DateTime.Now:MM/yyyy}";

            var qrPath = _pixService.GerarQrCodePix(nome, valor, descricao);
            boleto.QrCodePix = qrPath;

            await _ctx.SaveChangesAsync();

            TempData["MensagemSucesso"] = "QR Code PIX gerado com sucesso!";
            return RedirectToAction("Index");
        }
    }
}
