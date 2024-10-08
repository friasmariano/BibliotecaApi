

namespace BibliotecaApi.Models
{
    public class AutorLibro
    {
        public long Id { get; set; }
        public long AutorId { get; set; }
        public long LibroId { get; set; }

        public required Libro Libro { get; set; }
        public required Autor Autor { get; set; }
    }
}
