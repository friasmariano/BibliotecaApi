

public class UsuarioReadResponse {
    public long Id { get; set;}
    public string Nombre { get; set;} = string.Empty;
    public string Email { get; set;} = string.Empty;
    public long RolId { get; set;}
    public string Contrasena { get; set; } = string.Empty; 
}