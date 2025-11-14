using IngSw_Domain.Entities;
namespace IngSw_Domain.Interfaces;

public interface IEmployeeRepository
{
    Task<Employee?> Register(Employee employee);
    Task<Employee?> GetByEmail(string userEmail);
}
