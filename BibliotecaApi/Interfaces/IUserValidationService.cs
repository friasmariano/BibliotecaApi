
namespace BibliotecaApi.Interfaces;

public interface IUserValidationService
{
    Task<bool> IsUserAdminAsync(int userId);
}