using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using TrabalhoElvis2.Models;
using Trabalho.Models;

namespace TrabalhoElvis2.Models
{
    public class Boleto
    {
        [Key]
        public int Id { get; set; }

        // Ligação com o contrato (que conecta ao imóvel e ao morador)
        [Required]
        public int ContratoId { get; set; }

        [ForeignKey(nameof(ContratoId))]
        public Contrato? Contrato { get; set; }

        // Valor do boleto
        [Required(ErrorMessage = "O valor é obrigatório.")]
        [Column(TypeName = "decimal(10,2)")]
        public decimal Valor { get; set; }

        // Data de vencimento
        [Required(ErrorMessage = "A data de vencimento é obrigatória.")]
        [DataType(DataType.Date)]
        public DateTime Vencimento { get; set; }

        // Data do pagamento (quando o administrador confirma)
        [DataType(DataType.Date)]
        public DateTime? Pagamento { get; set; }

        // Status atual do boleto
        [Required, StringLength(30)]
        public string Status { get; set; } = "Pendente"; 
        // Valores possíveis: "Pendente", "Pago", "Vencido", "Aguardando Confirmação"

        // Caminho da imagem do QR Code (gerado automaticamente)
        public string? QrCodePix { get; set; }

        // Caminho da imagem do comprovante de pagamento (enviado pelo morador)
        public string? ComprovantePagamento { get; set; }
    }
}
