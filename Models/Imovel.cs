using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TrabalhoElvis2.Models
{
    public class Imovel
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "O código do imóvel é obrigatório.")]
        [StringLength(20)]
        public string Codigo { get; set; } = string.Empty;

        [Required(ErrorMessage = "O tipo do imóvel é obrigatório.")]
        [StringLength(50)]
        public string Tipo { get; set; } = string.Empty;

        [Range(1, 20, ErrorMessage = "Informe um número válido de quartos.")]
        public int? Quartos { get; set; } // antes era int

        [Range(1, 10, ErrorMessage = "Informe um número válido de banheiros.")]
        public int? Banheiros { get; set; } // antes era int

        [Required(ErrorMessage = "A metragem é obrigatória.")]
        public double? Metragem { get; set; } // antes era double

        [Column(TypeName = "decimal(10,2)")]
        [Range(0.01, 9999999, ErrorMessage = "Informe um valor de aluguel válido.")]
        public decimal? ValorAluguel { get; set; } // antes era decimal

        public string? Observacoes { get; set; }

        [StringLength(15)]
        public string Status { get; set; } = "Vago"; // Vago ou Ocupado

        // FK com proprietário
        [Required(ErrorMessage = "O proprietário é obrigatório.")]
        [Display(Name = "Proprietário")]
        public int CondominoId { get; set; }
        public Condomino? Condomino { get; set; }
    }
}
