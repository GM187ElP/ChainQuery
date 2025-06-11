using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace _1.FKs;

public class FKs
{
    public static Dictionary<string, (string tableName, Dictionary<(string foreignKey, string foreignKeytableName), string>? foreignKeys)> Fks = new()
    {
        ["City"] = ("Cities", null),
        ["Test1"] = ("Test1s", null),
        ["Employee"] = ("Employees", new Dictionary<(string, string), string>
        {
            [("Test2.Id", "Test2s")] = "Test2Id",
            [("City.Id", "Cities")] = "CityId"
        }),
        ["BankAccount"] = ("BankAccounts", new Dictionary<(string, string), string>
        {
            [("Employee.Id", "Employees")] = "EmployeeId"
        }),
        ["Test2"] = ("Test2s", new Dictionary<(string, string), string>
        {
            [("Test1.Id", "Test1s")] = "Test1Id"
        }),
        ["UnboundClass"] = ("UnboundClasses", new Dictionary<(string, string), string>
        {
            [("Employee.Id", "Employees")] = "EmployeeId"
        })
    };
}

