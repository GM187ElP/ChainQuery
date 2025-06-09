namespace _1.Models;

public class BankAccount
{
    public int Id { get; set; }
    public string Name { get; set; }
    public int EmployeeId { get; set; }
    public Employee Employee { get; set; }
}