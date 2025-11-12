using IngSw_Domain.Entities;
using IngSw_Infraestructure.Data.Connection;
using MySql.Data.MySqlClient;

namespace IngSw_Infraestructure.Data.DAOs;

public class AuthDao : DaoBase
{
    protected AuthDao(SqlConnection connection) : base(connection){ }
    public async Task<List<Dictionary<string, object>>?> Login(string userEmail)
    {
        var query = @"
        SELECT u.email, u.password, 
               e.name AS employee_name,
               e.lastname AS employee_lastname,
               e.cuil AS employee_cuil,
               e.phone AS employee_phone,
               e.email AS employee_email,
               e.registration AS employee_registration
        FROM users u
        INNER JOIN employees e ON u.employee = e.id
        WHERE u.email = @Email";

        var parameters = new MySqlParameter("@Email", userEmail);
        return await ExecuteReader(query, parameters);
    }
    public async Task<User?> Register(User newUser)
    {
        var insertEmployeeQuery = """
        INSERT INTO employees (id, name, last_name, cuil, phone_number, email, registration, type_employee)
        VALUES (@Id, @Name, @LastName, @Cuil, @PhoneNumber, @Email, @Registration, @TypeEmployee)
        """;

        var employeeParams = new[]
        {
        new MySqlParameter("@Id", newUser.Employee!.Id),
        new MySqlParameter("@Name", newUser.Employee.Name),
        new MySqlParameter("@LastName", newUser.Employee.LastName),
        new MySqlParameter("@Cuil", newUser.Employee.Cuil!.Value),
        new MySqlParameter("@PhoneNumber", newUser.Employee.PhoneNumber),
        new MySqlParameter("@Email", newUser.Employee.Email),
        new MySqlParameter("@Registration", newUser.Employee.Registration),
        new MySqlParameter("@TypeEmployee", newUser.Employee.TypeEmployee)
    };

        await ExecuteNonQuery(insertEmployeeQuery, employeeParams);

        var insertUserQuery = """
        INSERT INTO users (id, email, password, employee_id)
        VALUES (@Id, @Email, @Password, @EmployeeId)
        """;

        var userParams = new[]
        {
        new MySqlParameter("@Id", newUser.Id),
        new MySqlParameter("@Email", newUser.Email),
        new MySqlParameter("@Password", newUser.Password),
        new MySqlParameter("@EmployeeId", newUser.Employee.Id),
    };

        await ExecuteNonQuery(insertUserQuery, userParams);

        return newUser;
    }
}
