

namespace BibliotecaApi.Requests
{
    public class CELibroRequest
    {
        public int? Id { get; set; }
        public string Titulo { get; set; } = string.Empty;
        public string Descripcion {  get; set; } = string.Empty;
        public DateTime FechaPublicacion { get; set; }
        public int AutorId { get; set; }
        public int CategoriaId { get; set; }
    }
}
