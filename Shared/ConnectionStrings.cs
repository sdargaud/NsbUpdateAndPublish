namespace Shared
{
    public class ConnectionStrings
    {
        public const string DomainA = @"Data Source=(local);Initial Catalog=Nsb-DomainA-Endpoint-DB;Integrated Security=True;Max Pool Size=100";
        public const string DomainB = @"Data Source=(localDB)\MSSQLLocalDB;Initial Catalog=Nsb-DomainB-Endpoint-DB;Integrated Security=True;Max Pool Size=100";
        public const string Router = @"Data Source=(local);Initial Catalog=Nsb-DomainA-B-Router-DB;Integrated Security=True;Max Pool Size=100";
    }
}