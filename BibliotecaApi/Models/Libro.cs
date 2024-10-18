

namespace BibliotecaApi.Models
{
    public class Libro
    {
        public int Id { get; set; }
        public string Titulo { get; set; } = string.Empty;
        public string Descripcion { get; set; } = string.Empty;
        public DateTime FechaPublicacion { get; set; }

        public ICollection<Autor>? Autores { get; set; }
        public ICollection<Categoria>? Categorias { get; set; }
    }
}
