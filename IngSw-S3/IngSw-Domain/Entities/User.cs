using IngSw_Domain.Common;

namespace IngSw_Domain.Entities;

public class User : EntityBase
{
    public string? Email { get; set; }
    public string? Password { get; set; }
    public Guid EmployeeId { get; set; }
    public Employee Employee { get; set; } = null!;
}
