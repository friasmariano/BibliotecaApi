
using BibliotecaApi.Interfaces;

namespace BibliotecaApi.Services;

public class UserValidationService: IUserValidationService
{
    private readonly IUserRepository _userRepository;

    public UserValidationService(IUserRepository userRepository)
    {
        _userRepository = userRepository;
    }

    public async Task<bool> IsUserAdminAsync(int userId)
    {
        var user = await _userRepository.GetUserByIdAsync(userId);

        if (user == null)
        {
            throw new Exception("Usuario no encontrado");
        }
        
        return user.Id == 1 && user.Email == "admin@biblioteca.com";
    }
}