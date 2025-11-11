namespace IngSw_Application.DTOs;

public class UserDto
{
    public record Request(string? email, string? password);
    public record Response(string email,string name, string lastName, string cuil,
        string licence, string phoneNumber, string typeEmployee/*, string token*/);
    public record Register(string email, string password,string name, string lastName, string cuil,
        string licence, string phoneNumber, string typeEmployee);
}
