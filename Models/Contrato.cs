using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using TrabalhoElvis2.Models;

namespace Trabalho.Models
{
    public class Contrato
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [Display(Name = "Número do Contrato")]
        public string NumeroContrato { get; set; } = string.Empty;

        [Display(Name = "Imóvel")]
        public int ImovelId { get; set; } // FK
        public Imovel? Imovel { get; set; } // Navegação

        [Display(Name = "Condômino")]
        public int? CondominoId { get; set; } // FK opcional
        public Condomino? Condomino { get; set; } // Navegação

        [Display(Name = "Valor do Aluguel")]
        [DataType(DataType.Currency)]
        public decimal Valor { get; set; }

        [Display(Name = "Data de Início")]
        [DataType(DataType.Date)]
        public DateTime DataInicio { get; set; }

        [Display(Name = "Data de Término")]
        [DataType(DataType.Date)]
        public DateTime DataTermino { get; set; }

        [Display(Name = "Status")]
        public string Status { get; set; } = "Ativo";
    }
}
