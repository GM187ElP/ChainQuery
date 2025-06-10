using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace _1.FKs;

public class FKs
{
    public static Dictionary<string, Dictionary<(string foreignKey, string foreignKeytableName), (string property, string table)>?> Fks = new()
    {
        ["City"] = null,
        ["Test1"] = null,
        ["Employee"] = new Dictionary<(string, string), (string, string)>
        {
            [("Test2.Id", "Test2s")] = ("Test2Id", "Employees"),
            [("City.Id", "Cities")] = ("CityId", "Employees")
        },
        ["BankAccount"] = new Dictionary<(string, string), (string, string)>
        {
            [("Employee.Id", "Employees")] = ("EmployeeId", "BankAccounts")
        },
        ["Test2"] = new Dictionary<(string, string), (string, string)>
        {
            [("Test1.Id", "Test1s")] = ("Test1Id", "Test2s")
        },
        ["UnboundClass"] = new Dictionary<(string, string), (string, string)>
        {
            [("Employee.Id", "Employees")] = ("EmployeeId", "UnboundClasses")
        },
    };
}
