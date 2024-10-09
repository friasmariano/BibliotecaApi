

namespace BibliotecaApi.Models
{
    public class UsuarioToken
    {
        public long Id { get; set; }
        public long UsuarioId { get; set; }
        public string Token_hash { get; set; } = string.Empty;
        public DateTime CreadoEn { get; set; }
        public DateTime ExpiraEn { get; set; }
        public bool Valido { get; set; }

        public Usuario? Usuario { get; set; }
    }
}
