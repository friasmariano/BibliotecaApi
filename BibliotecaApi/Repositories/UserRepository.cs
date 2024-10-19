
using BibliotecaApi.Interfaces;
using BibliotecaApi.Models;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;

namespace BibliotecaApi.Repositories;

public class UserRepository: IUserRepository
{
    private readonly BibliotecaContext _context;

    public UserRepository(BibliotecaContext context)
    {
        _context = context;
    }

    public async Task<Usuario> GetUserByIdAsync(int userId)
    {
        var user = await _context.Usuarios.FindAsync(userId);
        
        if (user == null)
        {
            throw new KeyNotFoundException($"El usuario con el Id# {userId} no fue encontrado.");
        }

        return user;
    }
}