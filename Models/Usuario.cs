using System.ComponentModel.DataAnnotations;

namespace TrabalhoElvis2.Models
{
    public class Usuario
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "O tipo é obrigatório.")]
        public string TipoUsuario { get; set; }

        [Required(ErrorMessage = "O e-mail é obrigatório.")]
        [EmailAddress(ErrorMessage = "Digite um e-mail válido.")]
        public string Email { get; set; }

        [Required(ErrorMessage = "A senha é obrigatória.")]
        [StringLength(12, MinimumLength = 6, ErrorMessage = "A senha deve ter entre 6 e 12 caracteres.")]
        public string Senha { get; set; }

        // --- Campos do Administrador ---
        public string? NomeAdministrador { get; set; }
        public string? NomeCondominio { get; set; }
        public string? Cnpj { get; set; }

        // --- Campos do Morador ---
        public string? NomeCompleto { get; set; }
        public string? Apartamento { get; set; }
    }
}