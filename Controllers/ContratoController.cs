using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TrabalhoElvis2.Context;
using TrabalhoElvis2.Models;
using Trabalho.Models;
using System.Linq;

namespace TrabalhoElvis2.Controllers
{
    public class ContratoController : Controller
    {
        private readonly LoginContext _context;

        public ContratoController(LoginContext context)
        {
            _context = context;
        }

        public IActionResult Index(string termoBusca, string statusFiltro)
        {
            var tipo = HttpContext.Session.GetString("TipoUsuario");
            if (tipo != "Administrador")
            {
                TempData["MensagemContratoErro"] = "Acesso restrito a administradores.";
                return RedirectToAction("Login", "Usuario");
            }

            var query = _context.Contratos
                .Include(c => c.Imovel)
                .Include(c => c.Condomino)
                .AsQueryable();

            if (!string.IsNullOrEmpty(termoBusca))
            {
                termoBusca = termoBusca.ToLower();
                query = query.Where(c =>
                    (c.NumeroContrato != null && c.NumeroContrato.ToLower().Contains(termoBusca)) ||
                    (c.Imovel != null && c.Imovel.Codigo != null && c.Imovel.Codigo.ToLower().Contains(termoBusca)) ||
                    (c.Condomino != null && c.Condomino.NomeCompleto != null && c.Condomino.NomeCompleto.ToLower().Contains(termoBusca))
                );
            }

            if (!string.IsNullOrEmpty(statusFiltro) && statusFiltro != "Todos")
            {
                query = query.Where(c => c.Status == statusFiltro);
            }

            var contratos = query.ToList();
            ViewBag.Imoveis = _context.Imoveis.ToList();
            ViewBag.Condominos = _context.Condominos.Where(c => c.Tipo == "Proprietario" || c.Tipo == "Locatario").ToList();
            ViewBag.TermoBusca = termoBusca;
            ViewBag.StatusFiltro = statusFiltro;
            return View(contratos);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Cadastrar(Contrato contrato)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    // Recolhe erros do ModelState e envia via TempData para aparecer no _Alertas
                    var errors = string.Join(" | ", ModelState.Values
                        .SelectMany(v => v.Errors)
                        .Select(e => string.IsNullOrWhiteSpace(e.ErrorMessage) ? e.Exception?.Message : e.ErrorMessage));

                    TempData["MensagemContratoErro"] = string.IsNullOrWhiteSpace(errors)
                        ? "Preencha todos os campos obrigatórios." : $"Dados inválidos: {errors}";

                    // Redireciona para Index para que o modal seja fechado e a mensagem apareça
                    return RedirectToAction(nameof(Index));
                }

                _context.Contratos.Add(contrato);
                await _context.SaveChangesAsync();

                TempData["MensagemContratoSucesso"] = "Contrato cadastrado com sucesso!";
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                // Envia a mensagem real do erro para o usuário (útil em dev)
                TempData["MensagemContratoErro"] = $"Erro ao cadastrar o contrato: {ex.Message}";
                return RedirectToAction(nameof(Index));
            }
        }
    }
}
