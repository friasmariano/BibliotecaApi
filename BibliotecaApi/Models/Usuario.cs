

namespace BibliotecaApi.Models
{
    public class Usuario
    {
        public long Id { get; set; }
        public string Email { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
        public long PersonaId { get; set; }
        public long RolId { get; set; }

        public Persona? Persona { get; set; }
        public Rol? Rol { get; set; }
        public ICollection<UsuarioToken>? Tokens { get; set; }
    }
}
