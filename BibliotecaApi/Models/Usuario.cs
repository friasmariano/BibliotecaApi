

namespace BibliotecaApi.Models
{
    public class Usuario
    {
        public long Id { get; set; }
        public string Email { get; set; } = null!;
        public long PersonaId { get; set; }
        public long RolId { get; set; }

        public required Persona Persona { get; set; }
        public required Rol Rol { get; set; }
        public ICollection<UsuarioToken>? Tokens { get; set; }
    }
}
