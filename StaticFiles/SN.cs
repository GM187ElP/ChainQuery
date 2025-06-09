using System;

namespace _1.StaticFiles;

public static class SN   // static names for ui side
{
    public static class City
    {
        public const string Table = "City";
        public const string Id = "City.Id";
        public const string Name = "City.Name";
    }

    public static class Test1
    {
        public const string Table = "Test1";
        public const string Id = "Test1.Id";
    }

    public static class Employee
    {
        public const string Table = "Employee";
        public const string Id = "Employee.Id";
        public const string Name = "Employee.Name";
        public const string Test2Id = "Employee.Test2Id";
        public const string CityId = "Employee.CityId";
    }

    public static class BankAccount
    {
        public const string Table = "BankAccount";
        public const string Id = "BankAccount.Id";
        public const string Name = "BankAccount.Name";
        public const string EmployeeId = "BankAccount.EmployeeId";
    }

    public static class Test2
    {
        public const string Table = "Test2";
        public const string Id = "Test2.Id";
        public const string Name = "Test2.Name";
        public const string Test1Id = "Test2.Test1Id";
    }

    public static class UnboundClass
    {
        public const string Table = "UnboundClass";
        public const string Id = "UnboundClass.Id";
        public const string Name = "UnboundClass.Name";
        public const string EmployeeId = "UnboundClass.EmployeeId";
    }
}