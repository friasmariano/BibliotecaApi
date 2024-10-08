

namespace BibliotecaApi.Models
{
    public class UsuarioToken
    {
        public long Id { get; set; }
        public long UsuarioId { get; set; }
        public string Token_hash { get; set; } = null!;
        public DateTime CreadoEn { get; set; }
        public DateTime ExpiraEn { get; set; }
        public bool Valido { get; set; }

        public required Usuario Usuario { get; set; }
    }
}
