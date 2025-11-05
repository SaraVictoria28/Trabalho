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

        public IActionResult Cadastrar()
        {
            return View();
        }

        [HttpPost]
        public IActionResult Cadastrar(Usuario usuario)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    _context.Usuarios.Add(usuario);
                    _context.SaveChanges();

                    // Guarda o tipo de usuário e ID pra redirecionar
                    HttpContext.Session.SetString("TipoUsuario", usuario.TipoUsuario);
                    HttpContext.Session.SetString("NomeUsuario", usuario.NomeAdministrador ?? usuario.NomeCompleto);
                    HttpContext.Session.SetInt32("IdUsuario", usuario.Id);


                    // Redireciona direto pra interface correspondente
                    switch (usuario.TipoUsuario)
                    {
                        case "Administrador":
                            return RedirectToAction("Index", "Dashboard");

                        case "Síndico":
                            return RedirectToAction("InterfaceSindico", "Usuario");

                        case "Morador":
                            return RedirectToAction("InterfaceMorador", "Usuario");

                        default:
                            return RedirectToAction("Login", "Usuario");
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Erro ao salvar: {ex.Message}");
                    ModelState.AddModelError("", "Erro ao salvar no banco de dados.");
                }
            }
            else
            {
                foreach (var erro in ModelState.Values.SelectMany(v => v.Errors))
                {
                    Console.WriteLine($"Erro: {erro.ErrorMessage}");
                }
            }

            return View(usuario);
        }

        [HttpGet]
        public IActionResult Login()
        {
            return View();
        }

        // --- POST: LOGIN ---
        [HttpPost]
        public IActionResult Login(string email, string senha, string tipoUsuario)
        {
            if (string.IsNullOrWhiteSpace(email) || string.IsNullOrWhiteSpace(senha) || string.IsNullOrWhiteSpace(tipoUsuario))
            {
                ViewBag.Erro = "Preencha todos os campos!";
                return View();
            }

            string Normalizar(string texto)
            {
                return new string(texto
                    .Normalize(System.Text.NormalizationForm.FormD)
                    .Where(ch => System.Globalization.CharUnicodeInfo.GetUnicodeCategory(ch) != System.Globalization.UnicodeCategory.NonSpacingMark)
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

            //  Guarda na sessão
            HttpContext.Session.SetString("TipoUsuario", usuario.TipoUsuario);
            HttpContext.Session.SetString("NomeUsuario", usuario.NomeAdministrador ?? usuario.NomeCompleto);
            HttpContext.Session.SetInt32("IdUsuario", usuario.Id);

            // === Redirecionamento por tipo de usuário ===
            switch (usuario.TipoUsuario)
            {
                case "Administrador":
                    return RedirectToAction("Index", "Dashboard");
                case "Síndico":
                    return RedirectToAction("InterfaceSindico");
                case "Morador":
                    return RedirectToAction("InterfaceMorador");
                default:
                    return RedirectToAction("Login");
            }
        }



        // --- INTERFACE ---
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

        // --- LOGOUT ---
        public IActionResult Logout()
        {
            HttpContext.Session.Clear();
            return RedirectToAction("Login");
        }

        public IActionResult InterfaceMorador()
        {
            // Garante que só moradores acessem
            var tipo = HttpContext.Session.GetString("TipoUsuario");
            if (tipo != "Morador")
            {
                TempData["MensagemErro"] = "Acesso restrito a moradores.";
                return RedirectToAction("Login");
            }

            // Recupera informações do usuário logado
            ViewBag.NomeUsuario = HttpContext.Session.GetString("NomeUsuario");
            ViewBag.IdUsuario = HttpContext.Session.GetInt32("IdUsuario");

            return View();
        }

        public IActionResult InterfaceSindico()
        {
            // Permite apenas acesso do síndico
            var tipo = HttpContext.Session.GetString("TipoUsuario");
            if (tipo != "Síndico")
            {
                TempData["MensagemErro"] = "Acesso restrito a síndicos.";
                return RedirectToAction("Login");
            }

            // Envia dados do usuário logado para a View
            ViewBag.NomeUsuario = HttpContext.Session.GetString("NomeUsuario");
            ViewBag.IdUsuario = HttpContext.Session.GetInt32("IdUsuario");

            return View();
        }

    }
}

