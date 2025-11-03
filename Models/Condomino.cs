using System.ComponentModel.DataAnnotations;

namespace TrabalhoElvis2.Models
{
    public class Condomino
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "O nome é obrigatório.")]
        public string NomeCompleto { get; set; } = string.Empty;

        [Required(ErrorMessage = "O CPF é obrigatório.")]
        public string Cpf { get; set; } = string.Empty;

        [Required(ErrorMessage = "O e-mail é obrigatório.")]
        [EmailAddress(ErrorMessage = "E-mail inválido.")]
        public string Email { get; set; } = string.Empty;

        [Phone(ErrorMessage = "Telefone inválido.")]
        public string? Telefone { get; set; }

        [Required(ErrorMessage = "O tipo é obrigatório.")]
        public string Tipo { get; set; } = "Proprietário"; // Proprietário | Locatário

        [DataType(DataType.Date)]
        public DateTime DataContrato { get; set; } = DateTime.Today;

        public string? Observacoes { get; set; }

        // Navegação: 1 condômino -> N imóveis
        public ICollection<Imovel>? Imoveis { get; set; }
    }
}