namespace _1.Models;

public class City
{
    public int? Id { get; set; }
    public string Name { get; set; }
    public ICollection<Employee> Employess { get; set; }
}
