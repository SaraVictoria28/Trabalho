using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QRCoder;
using System.Drawing;
using System.Drawing.Imaging;
using TrabalhoElvis2.Context;
using TrabalhoElvis2.Models;

namespace TrabalhoElvis2.Controllers
{
    public class FinanceiroAdmController : Controller
    {
        private readonly LoginContext _context;
        private readonly IWebHostEnvironment _env;

        public FinanceiroAdmController(LoginContext context, IWebHostEnvironment env)
        {
            _context = context;
            _env = env;
        }

        // === PÁGINA PRINCIPAL DO FINANCEIRO (ADM) ===
        [HttpGet]
        public async Task<IActionResult> Administrador(string filtro = "Todos")
        {
            var boletos = _context.Boletos
                .Include(b => b.Contrato)
                    .ThenInclude(c => c.Imovel)
                .Include(b => b.Contrato)
                    .ThenInclude(c => c.Condomino)
                .AsQueryable();

            if (filtro == "Pago")
                boletos = boletos.Where(b => b.Status == "Pago");
            else if (filtro == "Pendente")
                boletos = boletos.Where(b => b.Status == "Pendente");
            else if (filtro == "Vencido")
                boletos = boletos.Where(b => b.Status == "Vencido");

            ViewBag.ReceitaPrevista = await _context.Boletos.SumAsync(b => b.Valor);
            ViewBag.ReceitaRecebida = await _context.Boletos.Where(b => b.Status == "Pago").SumAsync(b => b.Valor);
            ViewBag.Inadimplencia = await _context.Boletos.Where(b => b.Status != "Pago").SumAsync(b => b.Valor);

            return View("~/Views/Financeiro/Administrador.cshtml", await boletos.ToListAsync());
        }

        // === CRIAR NOVO BOLETO (GERADO PELO ADMINISTRADOR) ===
        [HttpPost]
        public async Task<IActionResult> CriarBoleto(int contratoId, decimal valor, DateTime vencimento)
        {
            // Validações básicas
            if (contratoId <= 0)
            {
                TempData["MensagemErro"] = "Contrato inválido. Selecione um contrato válido.";
                return RedirectToAction("Administrador");
            }

            if (valor <= 0)
            {
                TempData["MensagemErro"] = "Valor inválido. O valor deve ser maior que zero.";
                return RedirectToAction("Administrador");
            }

            if (vencimento < DateTime.Today)
            {
                TempData["MensagemErro"] = "Data de vencimento inválida. A data não pode ser no passado.";
                return RedirectToAction("Administrador");
            }

            var contrato = await _context.Contratos
                .Include(c => c.Condomino)
                .Include(c => c.Imovel)
                .FirstOrDefaultAsync(c => c.Id == contratoId);

            if (contrato == null)
            {
                TempData["MensagemErro"] = "Contrato não encontrado.";
                return RedirectToAction("Administrador");
            }

            try
            {
                var boleto = new Boleto
                {
                    ContratoId = contratoId,
                    Valor = valor,
                    Vencimento = vencimento,
                    Status = "Pendente"
                };

                _context.Boletos.Add(boleto);
                await _context.SaveChangesAsync();

                // 🔹 Gera o QR Code PIX automaticamente com CNPJ do condomínio
                // Pega o primeiro condominio disponível (em produção, seria baseado no imóvel/condomínio)
                var condominio = await _context.Condominios.FirstOrDefaultAsync();
                string chavePix = condominio?.Cnpj ?? "vizinapp@pix.com";
                
                // Remove formatação do CNPJ se houver (00.000.000/0000-00 → 00000000000000)
                chavePix = System.Text.RegularExpressions.Regex.Replace(chavePix, @"[^\d@.]", "");
                
                string payload = $"pix://{chavePix}?valor={valor}&nome={contrato.Condomino?.NomeCompleto ?? "Morador"}&imovel={contrato.Imovel?.Codigo ?? "Imóvel"}";

                using (var qrGen = new QRCoder.QRCodeGenerator())
                {
                    var data = qrGen.CreateQrCode(payload, QRCoder.QRCodeGenerator.ECCLevel.Q);
                    using var qrCode = new QRCoder.QRCode(data);
                    using var bitmap = qrCode.GetGraphic(20);

                    string path = Path.Combine(_env.WebRootPath, "qrcodes");
                    if (!Directory.Exists(path))
                        Directory.CreateDirectory(path);

                    string filePath = Path.Combine(path, $"qrcode_{boleto.Id}.png");
                    bitmap.Save(filePath, System.Drawing.Imaging.ImageFormat.Png);

                    boleto.QrCodePix = $"/qrcodes/qrcode_{boleto.Id}.png";
                    await _context.SaveChangesAsync();
                }

                TempData["MensagemSucesso"] = "Boleto gerado com sucesso! QR Code PIX gerado automaticamente.";
                return RedirectToAction("Administrador");
            }
            catch (Exception ex)
            {
                TempData["MensagemErro"] = $"Erro ao gerar boleto: {ex.Message}";
                return RedirectToAction("Administrador");
            }
        }


        // === MÉTODO INTERNO (NÃO ROTEÁVEL) PARA GERAR QR CODE ===
        private async Task GerarQrCodeInterno(int id)
        {
            var boleto = await _context.Boletos
                .Include(b => b.Contrato)
                .ThenInclude(c => c.Condomino)
                .FirstOrDefaultAsync(b => b.Id == id);

            if (boleto == null) return;

            string chavePix = "vizinapp@pix.com";
            string payload = $"pix://{chavePix}?valor={boleto.Valor}&nome={boleto.Contrato?.Condomino?.NomeCompleto}";

            using var qrGen = new QRCodeGenerator();
            var data = qrGen.CreateQrCode(payload, QRCodeGenerator.ECCLevel.Q);
            using var qrCode = new QRCode(data);
            using var bitmap = qrCode.GetGraphic(20);

            string pasta = Path.Combine(_env.WebRootPath, "qrcodes");
            if (!Directory.Exists(pasta))
                Directory.CreateDirectory(pasta);

            string caminho = Path.Combine(pasta, $"qrcode_{id}.png");
            bitmap.Save(caminho, ImageFormat.Png);

            boleto.QrCodePix = $"/qrcodes/qrcode_{id}.png";
            await _context.SaveChangesAsync();
        }

        // === GERAR QRCODE MANUALMENTE (POR BOTÃO NA VIEW) ===
        [HttpPost]
        public async Task<IActionResult> GerarQrCodePublico(int id)
        {
            await GerarQrCodeInterno(id);
            TempData["MensagemFinanceiroSucesso"] = "QR Code gerado com sucesso!";
            return RedirectToAction(nameof(Administrador));
        }


        // === CONFIRMAR PAGAMENTO (ADM) ===
        [HttpPost]
        public async Task<IActionResult> ConfirmarPagamento(int id)
        {
            var boleto = await _context.Boletos.FindAsync(id);
            if (boleto == null) return NotFound();

            boleto.Status = "Pago";
            boleto.Pagamento = DateTime.Now;

            _context.Update(boleto);
            await _context.SaveChangesAsync();

            TempData["MensagemSucesso"] = "Pagamento confirmado com sucesso!";
            return RedirectToAction(nameof(Administrador));
        }

        // === EXCLUIR BOLETO (ADM) ===
        [HttpPost]
        public async Task<IActionResult> ExcluirBoleto(int id)
        {
            var boleto = await _context.Boletos.FindAsync(id);
            if (boleto == null)
            {
                TempData["MensagemErro"] = "Boleto não encontrado.";
                return RedirectToAction(nameof(Administrador));
            }

            // Deleta o arquivo QR Code se existir
            if (!string.IsNullOrEmpty(boleto.QrCodePix))
            {
                string filePath = Path.Combine(_env.WebRootPath, boleto.QrCodePix.TrimStart('/'));
                if (System.IO.File.Exists(filePath))
                {
                    try
                    {
                        System.IO.File.Delete(filePath);
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"Erro ao deletar arquivo: {ex.Message}");
                    }
                }
            }

            _context.Boletos.Remove(boleto);
            await _context.SaveChangesAsync();

            TempData["MensagemSucesso"] = "Boleto excluído com sucesso!";
            return RedirectToAction(nameof(Administrador));
        }

        [HttpGet]
        public async Task<IActionResult> SugerirContratos(string termo)
        {
            if (string.IsNullOrWhiteSpace(termo))
                return Json(new List<object>());

            var contratos = await _context.Contratos
                .Include(c => c.Condomino)
                .Include(c => c.Imovel)
                .Where(c => c.NumeroContrato.Contains(termo))
                .Select(c => new
                {
                    // Usar nomes em camelCase para coincidir com o JS que consome o JSON
                    id = c.Id,
                    numeroContrato = c.NumeroContrato ?? "—",
                    locatario = c.Condomino != null ? c.Condomino.NomeCompleto : "Não informado",
                    imovel = c.Imovel != null ? c.Imovel.Codigo : "Não informado",
                    // Retorna o valor como número (para o input) e também formatado para exibição
                    valorMensal = c.Valor,
                    valorFormatado = c.Valor.ToString("C")
                })
                .Take(5)
                .ToListAsync();

            return Json(contratos);
        }

    }
}