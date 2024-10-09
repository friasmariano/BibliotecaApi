

namespace BibliotecaApi.Requests
{
    public class EditarUsuarioRequest
    {
        public long UsuarioId { get; set; }
        public string Nombre { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
        public long RoldId { get; set; }

    }
}
