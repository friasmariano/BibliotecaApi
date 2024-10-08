

namespace BibliotecaApi.Models
{
    public class Usuario
    {
        public long Id { get; set; }
        public string Email { get; set; } = null!;
        public long PersonaId { get; set; }

        public required Persona Persona { get; set; }
        public ICollection<UsuarioToken>? Tokens { get; set; }
    }
}
