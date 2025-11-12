using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using TrabalhoElvis2.Context;
using TrabalhoElvis2.Models;

namespace TrabalhoElvis2.Controllers
{
    // gerencia os imóveis do sistema.
    // Inclui A list,cadastro,editar e excluir
    // Acessado apenas pelo Administrador
    public class ImovelController : Controller
    {
        private readonly LoginContext _ctx; 

        public ImovelController(LoginContext ctx)
        {
            _ctx = ctx; 
        }

        //  LISTAR IMÓVEIS 
        // Exibe todos os imóveis cadastrados no sistema.
        // Permite filtrar por status (Vago/Ocupado).
        public async Task<IActionResult> Index(string statusFiltro, string termoBusca)
        {
            // Recupera o tipo do usuário logado pela sessão
            var tipoUsuario = HttpContext.Session.GetString("TipoUsuario");

            // Bloqueia o acesso caso o usuário não seja administrador
            if (tipoUsuario != "Administrador")
            {
                ViewBag.AcessoNegado = true; // Define que o acesso foi negado
                return View(new List<Imovel>()); // Retorna lista vazia se o acesso for negado
            }

            // Cria uma consulta base incluindo o relacionamento com o condômino
            var query = _ctx.Imoveis
                .Include(i => i.Condomino) // Inclui o relacionamento com o "Condomino" (proprietário)
                .AsQueryable();

            // Aplica o filtro de status (Vago/Ocupado)
            if (!string.IsNullOrEmpty(statusFiltro) && statusFiltro != "Todos os Status")
            {
                query = query.Where(i => i.Status == statusFiltro);
            }
            
            // Filtro de busca por código, observação ou proprietário
            if (!string.IsNullOrEmpty(termoBusca))
            {
                termoBusca = termoBusca.ToLower(); // Converte o termo de busca para minúsculo
                query = query.Where(i =>
                    i.Codigo.ToLower().Contains(termoBusca) || // Filtra por código do imóvel
                    i.Observacoes!.ToLower().Contains(termoBusca) || // Filtra por observações
                    (i.Condomino != null && i.Condomino.NomeCompleto.ToLower().Contains(termoBusca)) // Filtra por nome do proprietário
                );
            }

            // Executa a consulta e ordena pelo código do imóvel
            var lista = await query.OrderBy(i => i.Codigo).ToListAsync();

            // Mantém o status selecionado no dropdown
            ViewBag.StatusFiltro = statusFiltro ?? "Todos os Status"; // Define o filtro de status na ViewBag

            // Preenche o dropdown com apenas Proprietários e Locatários
            var listaCondominos = _ctx.Condominos
                .Where(c => c.Tipo == "Proprietario" || c.Tipo == "Locatario")
                .OrderBy(c => c.NomeCompleto)
                .ToList();

            // Envia a lista de proprietários para a View (para o dropdown)
            ViewBag.Condominos = new SelectList(listaCondominos, "Id", "NomeCompleto");

            return View(lista); // Retorna a lista de imóveis filtrados para a View
        }

        //  CADASTRAR IMÓVEL
        /// Cadastra um novo imóvel no sistema, vinculado a um condômino existente.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Cadastrar(Imovel imovel)
        {
            // Verifica se o proprietário foi selecionado
            if (imovel.CondominoId == null || imovel.CondominoId == 0)
            {
                ModelState.AddModelError("CondominoId", "O proprietário é obrigatório."); // Se não for selecionado, adiciona erro
            }

            // Verifica se já existe um imóvel com o mesmo código
            var imovelExistente = await _ctx.Imoveis
                .FirstOrDefaultAsync(i => i.Codigo == imovel.Codigo);

            if (imovelExistente != null)
            {
                ModelState.AddModelError("Codigo", "Já existe um imóvel com esse código."); // Se o código já existir, adiciona erro
            }

            // Se houver erros de validação, recarrega a página com os dados
            if (!ModelState.IsValid)
            {
                // Corrige o dropdown para mostrar apenas Proprietários e Locatários
                ViewBag.Condominos = new SelectList(
                    _ctx.Condominos
                        .Where(c => c.Tipo == "Proprietario" || c.Tipo == "Locatario")
                        .OrderBy(c => c.NomeCompleto)
                        .ToList(),
                    "Id", "NomeCompleto"
                );
                return View(imovel); // Retorna para o formulário de cadastro com os erros
            }

            // Adiciona o novo imóvel ao banco
            _ctx.Imoveis.Add(imovel);
            await _ctx.SaveChangesAsync(); // Salva as alterações no banco de dados

            // Mensagem de sucesso
            TempData["MensagemImovelSucesso"] = "Imóvel cadastrado com sucesso!";

            return RedirectToAction(nameof(Index)); // Redireciona para a página de listagem de imóveis
        }

        // EDITAR
        /// Exibe o formulário de edição de um imóvel específico.
        [HttpGet]
        public async Task<IActionResult> Editar(int id)
        {
            // Busca o imóvel no banco
            var imovel = await _ctx.Imoveis.FindAsync(id);
            if (imovel == null)
                return NotFound(); // Retorna erro se não encontrar o imóvel

            // Preenche o dropdown apenas com Proprietários e Locatários
            var listaCondominos = _ctx.Condominos
                .Where(c => c.Tipo == "Proprietario" || c.Tipo == "Locatario")
                .OrderBy(c => c.NomeCompleto)
                .ToList();

            // Define o item selecionado no dropdown
            ViewBag.Condominos = new SelectList(listaCondominos, "Id", "NomeCompleto", imovel.CondominoId);

            return View(imovel); // Retorna a View com os dados do imóvel para edição
        }

        //  EDITAR
        /// Atualiza os dados do imóvel no banco de dados.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Editar(Imovel imovel)
        {
            if (!ModelState.IsValid)
            {
                // Recupera a lista de condomínios (proprietários)
                ViewBag.Condominos = new SelectList(
                    _ctx.Condominos
                        .Where(c => c.Tipo == "Proprietario" || c.Tipo == "Locatario")
                        .OrderBy(c => c.NomeCompleto)
                        .ToList(),
                    "Id",
                    "NomeCompleto",
                    imovel.CondominoId
                );
                return View(imovel); // Retorna para o formulário de edição se houver erros
            }

            // Verificar se o código do imóvel já existe no banco (exceto para o próprio imóvel sendo editado)
            var existingImovel = await _ctx.Imoveis
                .Where(i => i.Codigo == imovel.Codigo && i.Id != imovel.Id)
                .FirstOrDefaultAsync();

            if (existingImovel != null)
            {
                ModelState.AddModelError("Codigo", "Já existe um imóvel com este código.");
                ViewBag.Condominos = new SelectList(
                    _ctx.Condominos
                        .Where(c => c.Tipo == "Proprietario" || c.Tipo == "Locatario")
                        .OrderBy(c => c.NomeCompleto)
                        .ToList(),
                    "Id",
                    "NomeCompleto",
                    imovel.CondominoId
                );
                return View(imovel); // Se o código já existir, retorna com erro
            }

            // Verificar se o proprietário já está associado a outro imóvel
            var existingProprietario = await _ctx.Imoveis
                .Where(i => i.CondominoId == imovel.CondominoId && i.Id != imovel.Id)
                .FirstOrDefaultAsync();

            if (existingProprietario != null)
            {
                ModelState.AddModelError("CondominoId", "Este proprietário já está associado a outro imóvel.");
                ViewBag.Condominos = new SelectList(
                    _ctx.Condominos
                        .Where(c => c.Tipo == "Proprietario" || c.Tipo == "Locatario")
                        .OrderBy(c => c.NomeCompleto)
                        .ToList(),
                    "Id",
                    "NomeCompleto",
                    imovel.CondominoId
                );
                return View(imovel); // Se o proprietário já estiver associado a outro imóvel, retorna com erro
            }

            // Atualiza o imóvel no banco de dados
            _ctx.Imoveis.Update(imovel);
            await _ctx.SaveChangesAsync(); // Salva as alterações no banco

            TempData["MensagemImovelSucesso"] = "Imóvel atualizado com sucesso!";

            return RedirectToAction(nameof(Index)); // Redireciona para a página de listagem de imóveis
        }

        // EXCLUIR IMÓVEL
        /// Remove definitivamente um imóvel do sistema.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Excluir(int id)
        {
            // Busca o imóvel
            var imovel = await _ctx.Imoveis.FindAsync(id);
            if (imovel == null)
                return NotFound(); // Retorna erro se o imóvel não for encontrado

            // Remove o registro e salva no banco
            _ctx.Imoveis.Remove(imovel);
            await _ctx.SaveChangesAsync(); // Salva as alterações no banco

            // Mensagem de sucesso
            TempData["MensagemImovelSucesso"] = "Imóvel excluído com sucesso!";

            return RedirectToAction(nameof(Index)); // Redireciona para a página de listagem de imóveis
        }
    }
}
