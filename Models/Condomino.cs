using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TrabalhoElvis2.Models
{
    public class Condomino
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [Display(Name = "Nome Completo")]
        public string NomeCompleto { get; set; }

        [Display(Name = "CPF")]
        public string? CPF { get; set; }

        [EmailAddress]
        public string? Email { get; set; }

        [Phone]
        public string? Telefone { get; set; }

        [Display(Name = "Tipo de Condômino")]
        public string Tipo { get; set; } // Proprietario, Locatario, Funcionario

        // Campos extras (para funcionários)
        public string? Cargo { get; set; }
        public string? Turno { get; set; }

        // Campos extras (para locatários)
        public DateTime? InicioLocacao { get; set; }
        public DateTime? FimLocacao { get; set; }

        [Column(TypeName = "decimal(10,2)")]
        public decimal? ValorAluguel { get; set; }

        // === Campos para exibição e associação ===
        [NotMapped]
        public string? ImovelNome { get; set; } // usado na view para exibir o nome/código do imóvel

        [NotMapped]
        public int QtdeImoveis { get; set; } // usado para exibir quantos imóveis o proprietário possui

        public bool Ativo { get; set; } = true;
    }
}
