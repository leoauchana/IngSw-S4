namespace IngSw_Domain.Entities;

public class Employee : Person
{
    public string? Registration { get; set; }
    public string? PhoneNumber { get; set; }
    public string? TypeEmployee { get; set; }
}
