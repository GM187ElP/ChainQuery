using _1.Context;
using _1.Methods;
using _1.Models;
using _1.Schema;
using _1.SqlBuilder;
using _1.StaticFiles;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json.Linq;

/*
#region Basic





using var context = new _1.Context.AppLicationContext();

var test1 = new List<Test1>
{
    new(){Name="Test1Name1"},
    new(){Name="Test1Name2"}
};

context.Database.EnsureDeleted();
context.Database.EnsureCreated();

// Seed Test1 first
context.Test1s.AddRange(test1);
context.SaveChanges(); // so IDs are generated for Test1

var test2 = new List<Test2>
{
    new(){Name="test2name1",Test1Id=test1[0].Id},
    new(){Name="test2name2",Test1Id=test1[1].Id},
    new(){Name="test2name3",Test1Id=test1[0].Id},
};



// Seed Test2 next, using Test1 IDs
context.Test2s.AddRange(test2);
context.SaveChanges(); // so IDs are generated for Test2

var unboundClass = new List<UnboundClass>
{
    new(){Name="unboundclassname1",EmployeeId=test2[0].Id},
    new(){Name="unboundclassname2",EmployeeId=test2[1].Id},
    new(){Name="unboundclassname3",EmployeeId=test2[2].Id},
};

// Seed UnboundClass last, using Test2 IDs
context.UnboundClasses.AddRange(unboundClass);
context.SaveChanges();

var cities = new List<City>
{
    new() {  Name = "London" },
    new() {  Name = "Tokyo" },
    new() {  Name = "Shanghai" },
    new() {  Name = "New York" },
    new() {  Name = "Beijing" }
};

// Seed Cities and Employees next
context.Cities.AddRange(cities);
context.SaveChanges();

var employees = new List<Employee>
{
    new() {  Name = "Gabriel", City = cities[0] }, // London
    new() {  Name = "Michael", City = cities[2] }, // Shanghai
    new() {  Name = "Jade", City = cities[1] },    // Tokyo
    new() {  Name = "Ali", City = cities[4] },     // Beijing
    new() {  Name = "Muhammad", City = cities[4] },// Beijing
    new() {  Name = "Alice", City = cities[2] }    // Shanghai
};

context.Employees.AddRange(employees);
context.SaveChanges();

var bankAccounts = new List<BankAccount>
{
    new() {  Name = "England", EmployeeId = employees[0].Id },
    new() {  Name = "Switzerland", EmployeeId = employees[1].Id },
    new() {  Name = "USA", EmployeeId = employees[2].Id },
    new() {  Name = "Japan", EmployeeId = employees[3].Id },
    new() {  Name = "Korea", EmployeeId = employees[4].Id },
    new() {  Name = "China", EmployeeId = employees[5].Id }
};
// Finally, seed BankAccounts (which depend on Employees)
context.BankAccounts.AddRange(bankAccounts);
context.SaveChanges();


#endregion

*/


//    private static void Main(string[] args)
//    {
//        var context = new AppLicationContext();

//        var requestJson = JObject.Parse(@"{
//    ""Cities"": {
//        ""Name"": true,
//        ""Employees"": {
//            ""Name"": true,
//            ""BankAccounts"": {
//                ""Name"": true
//            }
//        }
//    }
//}");


//        var q = SqlM.Query()
//            .Select(x => x
//            .SqlSelectField(SN.BankAccount.EmployeeId, "employeeId"))
//            .Select(x => x
//            .SqlSelectField(SN.Employee.Name, "arsalan"))
//            .From(SN.Employee.Table).Top(10)
//            ;








//Dictionary<string, Dictionary<string, PropertySchema>> schema2 =
//new()
//{
//    ["City"] = new Dictionary<string, PropertySchema>
//    {
//        ["Id"] = new(typeof(int), false),
//        ["Name"] = new(typeof(string), false)
//    },
//    ["Employee"] = new Dictionary<string, PropertySchema>
//    {
//        ["Id"] = new(typeof(int), false),
//        ["Name"] = new(typeof(string), false),
//        ["CityId"] = new(typeof(int), false, new(1, "City.Id"))
//    },
//    ["BankAccount"] = new Dictionary<string, PropertySchema>
//    {
//        ["Id"] = new(typeof(int), false),
//        ["Name"] = new(typeof(string), false),
//        ["EmployeeId"] = new(typeof(int), false, new(2, "Employee.Id"))
//    }
//};

Dictionary<string, Dictionary<(string foreignKey, string tableName), string>?> Fks = new()
{
    ["City"] = null,
    ["Test1"] = null,
    ["Employee"] = new Dictionary<(string, string), string>
    {
        [("Test2.Id", "Test2s")] = "Test2Id",
        [("City.Id", "Cities")] = "CityId"
    },
    ["BankAccount"] = new Dictionary<(string, string), string>
    {
        [("Employee.Id", "Employees")] = "EmployeeId"
    },
    ["Test2"] = new Dictionary<(string, string), string>
    {
        [("Test1.Id", "Test1s")] = "Test1Id"
    },
    ["UnboundClass"] = new Dictionary<(string, string), string>
    {
        [("Employee.Id", "Employess")] = "EmployeeId"
    },
};



var schema = new Schema(Fks).SchemaStructure;
//------------------------------------front and then back------------------------
var front = Query
    .From(SN.City.Table)
    .Select((SN.Employee.Name, "e.name"), (SN.Test1.Table, "T1"), (SN.City.Name, "c.name"))
    .Where(SqlCondition
    .Or(SqlCondition
    .And(SqlCondition
    .Single(SN.Employee.Id, Operator.Equal, SN.City.Name), SqlCondition
    .Single(SN.City.Name, Operator.Like, "A%"))))
    .Join(SN.Test2.Table, JoinType.Inner)
    .Join(SN.UnboundClass.Table, JoinType.Right)
    .Distinct()
    .Page(10, 20)
    .OrderBy(SN.City.Id).ToTransport();


var BacksideFromFront = Query
    .ParseFromJson(front)
    .ToExecutable(schema);



//----------------------------------backonly----------------------------------------
var back = Query
    .From(SN.City.Table)
    .Select((SN.Employee.Name, "e.name"), (SN.Test1.Table, "T1"), (SN.City.Name, "c.name"))
    .Where(SqlCondition
    .Or(SqlCondition
    .And(SqlCondition
    .Single(SN.Employee.Id, Operator.Equal, SN.City.Name), SqlCondition
    .Single(SN.City.Name, Operator.Like, "A%"))))
    .Join(SN.Test2.Table, JoinType.Inner)
    .Join(SN.UnboundClass.Table, JoinType.Right)
    .Distinct()
    .Page(10, 20)
    .OrderBy(SN.City.Id)
    .ToExecutable(schema);




Console.WriteLine(schema.ToString());

//var builder = new SqlBuilder(schema, context.Model);
//var query = builder.BuildQuery(requestJson);
//Console.WriteLine($"query: {query}");


//using var conn = context.Database.GetDbConnection();
//conn.Open();

//using var command = conn.CreateCommand();
//command.CommandText = query;
//Console.WriteLine();

//using var reader = command.ExecuteReader();

//while (reader.Read())
//{
//    for (int i = 0; i < reader.FieldCount; i++)
//    {
//        Console.Write($"{reader.GetName(i)} = {reader.GetValue(i)}; ");
//    }
//    Console.WriteLine();
//}





//        HashSet<string> sqlKeywords = new()
//{
//    "MUTATION", "QUERY", "SELECT", "DISTINCT", "TOP", "FROM",
//    "JOIN", "WHERE", "AND", "OR", "BETWEEN", "IN", "LIKE", "IS NULL", "IS NOT NULL", "EXISTS",
//    "GROUP BY", "HAVING",
//    "SUM", "AVG", "MIN", "MAX", "COUNT",
//    "ORDER BY", "OFFSET", "FETCH",
//    "UNION", "INTERSECT",
//    "CASE", "ISNULL",
//    "INSERT", "UPDATE", "DELETE", "EXEC",
//    "CREATE", "ALTER", "DROP", "TRUNCATE", "RENAME", "COMMENT"
//};

//        var requestJson2 = JObject.Parse(@"
//{
//  ""query"": {
//    ""select"": [""city.name"", ""bankaccount.name""],
//    ""top"": 10,
//    ""orderby"": ""employee.phonenumber"",
//    ""join"": [""left"", ""right""],
//    ""where"": {
//      ""or"": [
//        {
//          ""and"": [
//            { ""like"": [""A%"", ""bankaccount.name""] },
//            { ""like"": [""1%"", ""bankaccount.BankNumer""] }
//          ]
//        },
//        {
//          ""and"": [
//            { ""like"": [""A%"", ""employee.name""] },
//            { ""like"": [""1%"", ""employee.phonenumber""] }
//          ]
//        }
//      ]
//    }
//  }
//}
//");


//Json.JsonReader(requestJson2, sqlKeywords);
//var matches = Json.matches;

//foreach (var item in matches)
//{
//    foreach (var item2 in item.Keys)
//    {
//        Console.WriteLine($"key: {item2.key}    path: {item2.path}");
//    }
//}

//var entities = Json.GetAllEntitiesFromMatches();
//var matchedKey = Json.FindExactSecondLevelKey(schema.SchemaStructure, entities);



//Console.WriteLine(matchedKey);





//    }
//}

//class JsonDictionary
//{
//    public string Type { get; set; }  // query or ...
//    public string[] Select { get; set; }
//    public int? Top { get; set; }
//    //public Join[]? Join { get; set; }
//    //public OrderBy[]? OrderBies { get; set; }
//}



