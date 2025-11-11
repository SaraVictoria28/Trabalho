using Microsoft.EntityFrameworkCore;
using Trabalho.Models;
using TrabalhoElvis2.Models;

namespace TrabalhoElvis2.Context
{
    public class LoginContext : DbContext
    {
        public LoginContext(DbContextOptions<LoginContext> options) : base(options) { }

        public DbSet<Contrato> Contratos { get; set; }

        public DbSet<Usuario> Usuarios { get; set; }
        public DbSet<Condominio> Condominios { get; set; } // se já existir no teu projeto
        public DbSet<Condomino> Condominos { get; set; }
        public DbSet<Imovel> Imoveis { get; set; }
        public DbSet<Funcionario> Funcionarios { get; set; }
        public DbSet<Boleto> Boletos { get; set; }
    }
}