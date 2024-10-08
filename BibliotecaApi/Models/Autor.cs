

namespace BibliotecaApi.Models
{
    public class Autor
    {
        public long Id { get; set; }
        public long PersonaId { get; set; }

        public required Persona Persona { get; set; }
    }
}
