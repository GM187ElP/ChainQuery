using _1.FKs;
using _1.Schema;
using _1.StaticFiles;

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


internal class Program
{
    private static void Main(string[] args)
    {
        SchemaStore.Initialize(FKs.Fks);



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


        //var BacksideFromFront = Query
        //    .ParseFromJson(front)
        //    .ToExecutable(SchemaStore.Instance.SchemaStructure);

        var schema = new SchemaBuilder(FKs.Fks);

        var backQuery = Query
             .From(SN.Employee.Table)
             .Select((SN.BankAccount.Name, "b.name"), (SN.City.Name, "c.name"))
             .Join(SN.City.Table, JoinType.Left, SN.Employee.Table)
             .Join(SN.BankAccount.Table, JoinType.Left, SN.Employee.Table)
             .ToExecutable(schema);



        Console.WriteLine(backQuery);

        //var tablename = schema.FindTableName("Employee");

        Console.WriteLine();
        Console.WriteLine(SchemaStore.Instance.ToString());

    }
}
    //    var result = SchemaStore.Instance.FindSchemaByPathMatch("City_Employee_BankAccount");
    //    Console.WriteLine($"{result.Property}       {result.ReferenceTable}   {result.Path}    {result.ReferenceProperty}   {result.IsNullable}  {result.Type}");

    //    result = SchemaStore.Instance.FindSchemaByPathMatch(new List<string> { "Employee", "City", "BankAccount" });
    //    Console.WriteLine($"{result.Property}       {result.ReferenceTable}   {result.Path}    {result.ReferenceProperty}   {result.IsNullable}  {result.Type}");
    //}
