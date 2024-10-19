
using BibliotecaApi.Models;

namespace BibliotecaApi.Interfaces;

public interface IUserRepository
{
    Task<Usuario> GetUserByIdAsync(int id);    
}