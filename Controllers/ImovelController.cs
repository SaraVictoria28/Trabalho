using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using TrabalhoElvis2.Context;
using TrabalhoElvis2.Models;

namespace TrabalhoElvis2.Controllers
{
    // Controlador responsável pelo CRUD (Criar, Ler, Atualizar e Excluir) de Imóveis
    public class ImovelController : Controller
    {
        private readonly LoginContext _ctx;// Context do banco de dados
        public ImovelController(LoginContext ctx)
        {
            _ctx = ctx;
        }

        // LISTAR 
        // Exibe a lista de imóveis cadastrados no sistema, com opção de filtrar por status
        // Só o Administrador pode acessar esta tela de cadastro de imovel
        public async Task<IActionResult> Index(string statusFiltro)
        {
            // Recupera o tipo de usuário logado pela sessão
            var tipoUsuario = HttpContext.Session.GetString("TipoUsuario");

            // Se não for administrador, mostra aviso de acesso negado
            if (tipoUsuario != "Administrador")
            {
                ViewBag.AcessoNegado = true;
                return View(new List<Imovel>());
            }

            // Cria a query base
            var query = _ctx.Imoveis
                .Include(i => i.Condomino)
                .AsQueryable();

            //  Aplica filtro se foi selecionado um status
            if (!string.IsNullOrEmpty(statusFiltro) && statusFiltro != "Todos os Status")
            {
                query = query.Where(i => i.Status == statusFiltro);
            }

            // Ordena e busca os dados
            var lista = await query.OrderBy(i => i.Codigo).ToListAsync();

            // Mantém o status selecionado no filtro 
            ViewBag.StatusFiltro = statusFiltro ?? "Todos os Status";

            // Lista apenas os condôminos que são proprietários ou locatários
            var listaCondominos = _ctx.Condominos
                .Where(c => c.Tipo == "Proprietário" || c.Tipo == "Locatário")
                .OrderBy(c => c.NomeCompleto)
                .ToList();

            ViewBag.Condominos = new SelectList(listaCondominos, "Id", "NomeCompleto");

            return View(lista);
        }

        // CADASTRAR 
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Cadastrar(Imovel imovel)
        {
            // Se o formulário estiver inválido, recarrega a página com os erros
            if (!ModelState.IsValid)
            {
                ViewBag.Condominos = new SelectList(_ctx.Condominos.OrderBy(c => c.NomeCompleto).ToList(), "Id", "NomeCompleto");
                var lista = await _ctx.Imoveis.Include(i => i.Condomino).ToListAsync();
                return View("Index", lista);
            }
            // Adiciona o imóvel ao context e salva as alterações no banco
            _ctx.Imoveis.Add(imovel);
            await _ctx.SaveChangesAsync();

            TempData["MensagemSucesso"] = "Imóvel cadastrado com sucesso!";
            return RedirectToAction(nameof(Index));
        }

        // EDITAR
        // Abre o formulário de edição com os dados do imóvel selecionado
        [HttpGet]
        public async Task<IActionResult> Editar(int id)
        {
            var imovel = await _ctx.Imoveis.FindAsync(id);
            if (imovel == null)
                return NotFound();

            // Preenche dropdown de condômino
            var listaCondominos = _ctx.Condominos
     .Where(c => c.Tipo == "Proprietário" || c.Tipo == "Locatário")
     .OrderBy(c => c.NomeCompleto)
     .ToList();

            ViewBag.Condominos = new SelectList(listaCondominos, "Id", "NomeCompleto", imovel.CondominoId);


            return View(imovel);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Editar(Imovel imovel)
        {
            // Caso o modelo tenha erro de validação, retorna à view com dados preenchidos
            if (!ModelState.IsValid)
            {
                ViewBag.Condominos = new SelectList(_ctx.Condominos.OrderBy(c => c.NomeCompleto).ToList(), "Id", "NomeCompleto", imovel.CondominoId);
                return View(imovel);
            }
            // Atualiza o imóvel no banco
            _ctx.Imoveis.Update(imovel);
            await _ctx.SaveChangesAsync();

            TempData["MensagemSucesso"] = "Imóvel atualizado com sucesso!";
            return RedirectToAction(nameof(Index));
        }

        // EXCLUIR 
        // Remove um imóvel do banco de dados
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Excluir(int id)
        {
            var imovel = await _ctx.Imoveis.FindAsync(id);
            if (imovel == null)
                return NotFound();
        // Remove o registro e salva no banco
            _ctx.Imoveis.Remove(imovel);
            await _ctx.SaveChangesAsync();

            TempData["MensagemSucesso"] = "Imóvel excluído com sucesso!";
            return RedirectToAction(nameof(Index));
        }
    }
}
