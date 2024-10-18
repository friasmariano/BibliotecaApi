

namespace BibliotecaApi.Models
{
    public class AutorLibro
    {
        public int Id { get; set; }
        public int AutorId { get; set; }
        public int LibroId { get; set; }

        public Libro? Libro { get; set; }
        public Autor? Autor { get; set; }
    }
}
