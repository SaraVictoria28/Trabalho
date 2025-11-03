using Microsoft.AspNetCore.Mvc;
using System.Globalization;
using System.Text;
using TrabalhoElvis2.Context;
using TrabalhoElvis2.Models;

namespace TrabalhoElvis2.Controllers
{
    public class UsuarioController : Controller
    {
        private readonly LoginContext _context;

        public UsuarioController(LoginContext context)
        {
            _context = context;
        }

        // --- GET: CADASTRAR ---
        public IActionResult Cadastrar()
        {
            return View();
        }

        // --- POST: CADASTRAR ---
        [HttpPost]
        public IActionResult Cadastrar(Usuario usuario)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    _context.Usuarios.Add(usuario);
                    int registros = _context.SaveChanges();

                    Console.WriteLine($"✅ Usuário cadastrado com sucesso! Registros: {registros}");
                    TempData["MensagemSucesso"] = "Cadastro realizado com sucesso! Faça login para continuar.";

                    return RedirectToAction("Login", "Usuario");
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"❌ Erro ao salvar: {ex.Message}");
                    ModelState.AddModelError("", "Erro ao salvar no banco de dados.");
                }
            }
            else
            {
                foreach (var erro in ModelState.Values.SelectMany(v => v.Errors))
                {
                    Console.WriteLine($"⚠️ Erro: {erro.ErrorMessage}");
                }
            }

            return View(usuario);
        }

        // --- GET: LOGIN ---
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

            TempData["TipoUsuario"] = usuario.TipoUsuario;
            TempData["Nome"] = usuario.NomeAdministrador ?? usuario.NomeCompleto;
            TempData["IdUsuario"] = usuario.Id;

            return RedirectToAction("Interface");
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
            return RedirectToAction("Login");
        }
    }
}