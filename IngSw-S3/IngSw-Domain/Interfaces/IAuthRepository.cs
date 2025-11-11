using IngSw_Domain.Entities;
namespace IngSw_Domain.Interfaces;

public interface IAuthRepository
{
    Task<User?> Login(string userEmail);
    Task<User?> Register(User user);
    Task<User?> GetByEmail(string userEmail);
}
