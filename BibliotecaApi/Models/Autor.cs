

namespace BibliotecaApi.Models
{
    public class Autor
    {
        public long Id { get; set; }
        public long PersonaId { get; set; }

        public Persona? Persona { get; set; }
    }
}
