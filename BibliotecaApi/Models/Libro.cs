

namespace BibliotecaApi.Models
{
    public class Libro
    {
        public long Id { get; set; }
        public string Titulo { get; set; } = null!;
        public string Descripcion { get; set; } = null!;
        public DateTime FechaPublicacion { get; set; }

        public required ICollection<Autor> Autores { get; set; }
        public required ICollection<Categoria> Categorias { get; set; }
    }
}
