

namespace BibliotecaApi.Models
{
    public class Autor
    {
        public int Id { get; set; }
        public int PersonaId { get; set; }

        public Persona? Persona { get; set; }
    }
}
