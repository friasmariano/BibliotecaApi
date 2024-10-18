
namespace BibliotecaApi.Models;

public class Usuario
{
    public int Id { get; set; }
    public string Email { get; set; } = String.Empty;
    public string Password { get; set; } = String.Empty;
    public int PersonaId { get; set; }
    public int RoldId { get; set; }
    
    public Persona? Persona { get; set; }
    public Rol? Rol { get; set; }
}