namespace _1.Models;

public class Employee
{
    public int Id { get; set; }
    public string Name { get; set; }
    public int Test2Id { get; set; }
    public Test2 Test2 { get; set; }
    public int? CityId { get; set; }
    public City City { get; set; }
}

