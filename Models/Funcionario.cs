using System.ComponentModel.DataAnnotations;

namespace TrabalhoElvis2.Models
{
    public class Funcionario
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "O nome é obrigatório.")]
        public string Nome { get; set; } = string.Empty;

        [Required(ErrorMessage = "O cargo é obrigatório.")]
        public string Cargo { get; set; } = "Síndico"; // Síndico, Zelador, Porteiro, Limpeza...

        [Phone(ErrorMessage = "Telefone inválido.")]
        public string? Telefone { get; set; }

        [EmailAddress(ErrorMessage = "E-mail inválido.")]
        public string? Email { get; set; }

        [DataType(DataType.Date)]
        public DateTime DataContratacao { get; set; } = DateTime.Today;
    }
}