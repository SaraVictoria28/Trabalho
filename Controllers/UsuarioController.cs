using Microsoft.AspNetCore.Mvc;
using System.Globalization;
using System.Text;
using TrabalhoElvis2.Context;
using TrabalhoElvis2.Models;
using Microsoft.AspNetCore.Http;

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
                return View(usuario);

            try
            {
                // Normaliza e remove espaços do tipo
                usuario.TipoUsuario = usuario.TipoUsuario?.Trim() ?? string.Empty;

                // === Preenche automaticamente dados conforme o tipo ===
                if (usuario.TipoUsuario.Equals("Administrador", StringComparison.OrdinalIgnoreCase))
                {
                    if (string.IsNullOrEmpty(usuario.NomeAdministrador))
                        usuario.NomeAdministrador = "Administrador do sistema";

                    // Grava o usuário no banco
                    _context.Usuarios.Add(usuario);
                    _context.SaveChanges();

                    // Armazena na sessão
                    HttpContext.Session.SetString("TipoUsuario", usuario.TipoUsuario);
                    HttpContext.Session.SetString("NomeUsuario", usuario.NomeAdministrador);
                    HttpContext.Session.SetInt32("UsuarioId", usuario.Id);

                    return RedirectToAction("Index", "Dashboard");
                }
                else if (usuario.TipoUsuario.Equals("Síndico", StringComparison.OrdinalIgnoreCase))
                {
                    if (string.IsNullOrEmpty(usuario.NomeCompleto))
                        throw new Exception("Nome completo obrigatório para Síndico");

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
                    HttpContext.Session.SetInt32("UsuarioId", usuario.Id);
                    HttpContext.Session.SetInt32("CondominoId", condomino.Id);

                    return RedirectToAction("InterfaceSindico", "Usuario");
                }
                else if (usuario.TipoUsuario.Equals("Morador", StringComparison.OrdinalIgnoreCase))
                {
                    if (string.IsNullOrEmpty(usuario.NomeCompleto))
                        throw new Exception("Nome completo obrigatório para Morador");

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
                    HttpContext.Session.SetInt32("UsuarioId", usuario.Id);
                    HttpContext.Session.SetInt32("CondominoId", condomino.Id);

                    return RedirectToAction("InterfaceMorador", "Usuario");
                }
                else
                {
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

            ViewBag.NomeUsuario = HttpContext.Session.GetString("NomeUsuario");
            ViewBag.IdUsuario = HttpContext.Session.GetInt32("IdUsuario");

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