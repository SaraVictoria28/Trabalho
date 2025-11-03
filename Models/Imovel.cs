using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TrabalhoElvis2.Models
{
    public class Imovel
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "O número/apto é obrigatório.")]
        public string Numero { get; set; } = string.Empty;

        [Required(ErrorMessage = "O bloco é obrigatório.")]
        public string Bloco { get; set; } = string.Empty;

        [Required(ErrorMessage = "A metragem é obrigatória.")]
        public double Metragem { get; set; }

        public string? Observacao { get; set; }

        // FK para Condomino
        [Display(Name = "Condômino")]
        public int? CondominoId { get; set; }
        public Condomino? Condomino { get; set; }
    }
}