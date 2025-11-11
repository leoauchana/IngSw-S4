using IngSw_Application.DTOs;
using IngSw_Domain.Entities;
using IngSw_Domain.Interfaces;
using IngSw_Domain.ValueObjects;
using IngSw_Infraestructure.Data.DAOs;

namespace IngSw_Infraestructure.Data.Repository;

public class AuthRepository : IAuthRepository
{
    private readonly AuthDao _authDao;

    public AuthRepository(AuthDao authDao)
    {
        _authDao = authDao;
    }

    public Task<User?> GetByEmail(string userEmail)
    {
        throw new NotImplementedException();
    }
    
    public async Task<User?> Login(string userEmail)
    {
        var userFound = await _authDao.Login(userEmail);
        return userFound?[0] != null ? MapEntity(userFound![0]) : null;
    }

    public async Task<User?> Register(User newUser) => await _authDao.Register(newUser);

    private User MapEntity(Dictionary<string, object>? reader)
    {
        return new User
        {
            Id = (Guid)reader!["id"],
            Email = reader["email"].ToString(),
            Password = reader["password"].ToString(),
            Employee = new Employee
            {
                Name = reader["employee_name"].ToString(),
                LastName = reader["employee_lastname"].ToString(),
                Cuil = Cuil.Create(reader["employee_cuil"].ToString()!),
                PhoneNumber = reader["employee_phone"].ToString(),
                Email = reader["employee_email"].ToString(),
                Registration = reader["employee_registration"].ToString()
            }
        };
    }

    //private Nurse MapEntity(Dictionary<string, object>? reader) where TEntity : EntityBase
    //{
    //    return new Patient
    //    {
    //        Id = (Guid)reader["id"],
    //        Name = reader["name"]?.ToString(),
    //        LastName = reader["last_name"].ToString(),
    //        Cuil = Cuil.Create(reader["cuil"].ToString()),
    //        Domicilie = new Domicilie
    //        {
    //            Number = Convert.ToInt32(reader["number"]),
    //            Street = reader["street"].ToString(),
    //            Locality = reader["locality"].ToString()
    //        }
    //    };
    //}
}
