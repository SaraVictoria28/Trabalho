using Microsoft.AspNetCore.Mvc;
using System.Globalization;
using System.Text;
using TrabalhoElvis2.Context;
using TrabalhoElvis2.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;

namespace TrabalhoElvis2.Controllers
{
    public class UsuarioController : Controller
    {
        private readonly LoginContext _context;

        public UsuarioController(LoginContext context)
        {
            _context = context;
        }

        // === CADASTRAR ===
        public IActionResult Cadastrar()
        {
            return View();
        }

        [HttpPost]
        public IActionResult Cadastrar(Usuario usuario)
        {
            if (!ModelState.IsValid)
            {
                // Log dos erros
                var errors = ModelState.Values.SelectMany(v => v.Errors);
                foreach (var error in errors)
                {
                    Console.WriteLine($"Erro de ModelState: {error.ErrorMessage}");
                }
                return View(usuario);
            }

            try
            {
                // Normaliza e remove espaços do tipo
                usuario.TipoUsuario = usuario.TipoUsuario?.Trim() ?? string.Empty;

                // === Preenche automaticamente dados conforme o tipo ===
                if (usuario.TipoUsuario.Equals("Administrador", StringComparison.OrdinalIgnoreCase))
                {
                    if (string.IsNullOrEmpty(usuario.NomeAdministrador))
                    {
                        Console.WriteLine("Erro: NomeAdministrador vazio");
                        throw new Exception("Nome do administrador obrigatório");
                    }

                    // Grava o usuário no banco
                    _context.Usuarios.Add(usuario);
                    _context.SaveChanges();

                    // Armazena na sessão
                    HttpContext.Session.SetString("TipoUsuario", usuario.TipoUsuario);
                    HttpContext.Session.SetString("NomeUsuario", usuario.NomeAdministrador);
                    HttpContext.Session.SetInt32("IdUsuario", usuario.Id);

                    return RedirectToAction("Index", "Dashboard");
                }
                else if (usuario.TipoUsuario.Equals("Síndico", StringComparison.OrdinalIgnoreCase))
                {
                    if (string.IsNullOrWhiteSpace(usuario.NomeCompleto))
                    {
                        Console.WriteLine("Erro: NomeCompleto vazio para Síndico");
                        ModelState.AddModelError(nameof(usuario.NomeCompleto), "Nome completo obrigatório para Síndico");
                        return View(usuario);
                    }

                    // Cria registro em Condomino
                    var condomino = new Condomino
                    {
                        NomeCompleto = usuario.NomeCompleto,
                        Email = usuario.Email,
                        Telefone = usuario.Telefone,
                        Tipo = "Síndico",
                        Ativo = true
                    };
                    _context.Condominos.Add(condomino);
                    _context.SaveChanges();

                    // Grava o usuário no banco
                    usuario.NomeCompleto = condomino.NomeCompleto;
                    _context.Usuarios.Add(usuario);
                    _context.SaveChanges();

                    // Armazena na sessão
                    HttpContext.Session.SetString("TipoUsuario", usuario.TipoUsuario);
                    HttpContext.Session.SetString("NomeUsuario", usuario.NomeCompleto);
                    HttpContext.Session.SetInt32("IdUsuario", usuario.Id);
                    HttpContext.Session.SetInt32("CondominoId", condomino.Id);

                    return RedirectToAction("InterfaceSindico", "Usuario");
                }
                else if (usuario.TipoUsuario.Equals("Morador", StringComparison.OrdinalIgnoreCase))
                {
                    if (string.IsNullOrWhiteSpace(usuario.NomeCompleto))
                    {
                        Console.WriteLine("Erro: NomeCompleto vazio para Morador");
                        ModelState.AddModelError(nameof(usuario.NomeCompleto), "Nome completo obrigatório para Morador");
                        return View(usuario);
                    }

                    // Cria registro em Condomino
                    var condomino = new Condomino
                    {
                        NomeCompleto = usuario.NomeCompleto,
                        Email = usuario.Email,
                        Telefone = usuario.Telefone,
                        Tipo = "Locatário",
                        Ativo = true
                    };
                    _context.Condominos.Add(condomino);
                    _context.SaveChanges();

                    // Grava o usuário no banco
                    usuario.NomeCompleto = condomino.NomeCompleto;
                    _context.Usuarios.Add(usuario);
                    _context.SaveChanges();

                    // Armazena na sessão
                    HttpContext.Session.SetString("TipoUsuario", usuario.TipoUsuario);
                    HttpContext.Session.SetString("NomeUsuario", usuario.NomeCompleto);
                    HttpContext.Session.SetInt32("IdUsuario", usuario.Id);
                    HttpContext.Session.SetInt32("CondominoId", condomino.Id);

                    return RedirectToAction("InterfaceMorador", "Usuario");
                }
                else
                {
                    Console.WriteLine($"Erro: Tipo de usuário inválido: {usuario.TipoUsuario}");
                    throw new Exception("Tipo de usuário inválido");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Erro ao salvar: {ex.Message}");
                ModelState.AddModelError("", $"Erro ao salvar: {ex.Message}");
            }

            return View(usuario);
        }

        // === LOGIN ===
        [HttpGet]
        public IActionResult Login()
        {
            return View();
        }

        [HttpPost]
        public IActionResult Login(string email, string senha, string tipoUsuario)
        {
            if (string.IsNullOrWhiteSpace(email) || string.IsNullOrWhiteSpace(senha) || string.IsNullOrWhiteSpace(tipoUsuario))
            {
                ViewBag.Erro = "Preencha todos os campos!";
                return View();
            }

            // Normaliza texto (remove acentos e coloca em minúsculo)
            string Normalizar(string texto)
            {
                return new string(texto
                    .Normalize(NormalizationForm.FormD)
                    .Where(ch => CharUnicodeInfo.GetUnicodeCategory(ch) != UnicodeCategory.NonSpacingMark)
                    .ToArray())
                    .ToLower();
            }

            string tipoNormalizado = Normalizar(tipoUsuario);
            var usuarios = _context.Usuarios.ToList();

            var usuario = usuarios.FirstOrDefault(u =>
                u.Email.Equals(email, StringComparison.OrdinalIgnoreCase) &&
                u.Senha == senha &&
                Normalizar(u.TipoUsuario) == tipoNormalizado
            );

            if (usuario == null)
            {
                ViewBag.Erro = "E-mail, senha ou tipo de usuário incorretos!";
                return View();
            }

            // Guarda sessão
            HttpContext.Session.SetString("TipoUsuario", usuario.TipoUsuario);
            HttpContext.Session.SetString("NomeUsuario", usuario.NomeAdministrador ?? usuario.NomeCompleto);
            HttpContext.Session.SetInt32("IdUsuario", usuario.Id);

            // Redireciona conforme o tipo
            switch (usuario.TipoUsuario)
            {
                case "Administrador":
                    return RedirectToAction("Index", "Dashboard"); // Dashboard do Admin

                case "Síndico":
                    return RedirectToAction("InterfaceSindico", "Usuario"); // Dashboard do Síndico

                case "Morador":
                    return RedirectToAction("InterfaceMorador", "Usuario"); // Dashboard do Morador

                default:
                    return RedirectToAction("Login", "Usuario");
            }
        }

        // === INTERFACE ===
        public IActionResult Interface()
        {
            var tipo = TempData["TipoUsuario"]?.ToString();
            var idUsuario = TempData["IdUsuario"]?.ToString();

            if (tipo == null || idUsuario == null)
                return RedirectToAction("Login");

            int id = int.Parse(idUsuario);

            switch (tipo)
            {
                case "Administrador":
                    return View("InterfaceAdministrador");
                case "Síndico":
                    return View("InterfaceSindico");
                case "Morador":
                    return View("InterfaceMorador");
                default:
                    return RedirectToAction("Login");
            }
        }

        // === LOGOUT ===
        public IActionResult Logout()
        {
            HttpContext.Session.Clear();
            return RedirectToAction("Login");
        }

        // === INTERFACES ESPECÍFICAS ===
        public IActionResult InterfaceMorador()
        {
            var tipo = HttpContext.Session.GetString("TipoUsuario");
            if (tipo != "Morador")
            {
                TempData["MensagemErro"] = "Acesso restrito a moradores.";
                return RedirectToAction("Login");
            }

            var usuarioId = HttpContext.Session.GetInt32("IdUsuario");
            var nomeUsuario = HttpContext.Session.GetString("NomeUsuario");

            ViewBag.NomeUsuario = nomeUsuario;
            ViewBag.IdUsuario = usuarioId;

            // Buscar dados do morador (Condomino)
            var condomino = _context.Condominios
                .FirstOrDefault(c => c.Id == usuarioId);

            if (condomino != null)
            {
                ViewBag.Condomino = condomino;
            }

            // Buscar imóvéis do morador
            var imoveis = _context.Imoveis
                .Where(i => i.CondominoId == usuarioId)
                .ToList();

            ViewBag.Imoveis = imoveis;

            // Buscar boletos pendentes/em dia
            var boletos = _context.Boletos
                .Include(b => b.Contrato)
                    .ThenInclude(c => c.Imovel)
                .Where(b => b.Contrato.Imovel.CondominoId == usuarioId)
                .OrderByDescending(b => b.Vencimento)
                .ToList();

            ViewBag.BoletosPendentes = boletos.Where(b => b.Status == "Pendente").ToList();
            ViewBag.Boletos = boletos;

            // Estatísticas
            ViewBag.TotalBoletos = boletos.Count;
            ViewBag.BoletosPagos = boletos.Count(b => b.Status == "Pago");
            ViewBag.BoletosPendentes = boletos.Count(b => b.Status == "Pendente");
            ViewBag.ValorTotal = boletos.Sum(b => b.Valor);
            ViewBag.ValorPago = boletos.Where(b => b.Status == "Pago").Sum(b => b.Valor);
            ViewBag.ValorPendente = boletos.Where(b => b.Status != "Pago").Sum(b => b.Valor);

            return View();
        }

        public IActionResult InterfaceSindico()
        {
            var tipo = HttpContext.Session.GetString("TipoUsuario");
            if (tipo != "Síndico")
            {
                TempData["MensagemErro"] = "Acesso restrito a síndicos.";
                return RedirectToAction("Login");
            }

            ViewBag.NomeUsuario = HttpContext.Session.GetString("NomeUsuario");
            ViewBag.IdUsuario = HttpContext.Session.GetInt32("IdUsuario");

            return View();
        }
    }
}