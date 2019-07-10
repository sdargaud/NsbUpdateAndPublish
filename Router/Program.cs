using System;
using System.Data.SqlClient;
using System.Threading.Tasks;
using NServiceBus;
using NServiceBus.Router;
using Shared;

static class Program
{
    static async Task Main()
    {
        Console.Title = "Router";

        #region RouterConfig

        var routerConfig = new RouterConfiguration("DomainA-B-Router");

        var domainAInterface = routerConfig.AddInterface<SqlServerTransport>("DomainA", t =>
        {
            t.ConnectionString(ConnectionStrings.DomainA);
            t.Transactions(TransportTransactionMode.SendsAtomicWithReceive);
        });
        var domainASqlSubscriptionStorage = new SqlSubscriptionStorage(() => new SqlConnection(ConnectionStrings.Router), "DomainA-", new SqlDialect.MsSqlServer(), null);
        domainAInterface.EnableMessageDrivenPublishSubscribe(domainASqlSubscriptionStorage);

        var domainBInterface = routerConfig.AddInterface<SqlServerTransport>("DomainB", t => {
            t.ConnectionString(ConnectionStrings.DomainB);
            t.Transactions(TransportTransactionMode.SendsAtomicWithReceive);
        });
        var domainBSqlSubscriptionStorage = new SqlSubscriptionStorage(() => new SqlConnection(ConnectionStrings.Router), "DomainB-", new SqlDialect.MsSqlServer(), null);
        domainBInterface.EnableMessageDrivenPublishSubscribe(domainBSqlSubscriptionStorage);

        var staticRouting = routerConfig.UseStaticRoutingProtocol();
        staticRouting.AddForwardRoute("DomainA", "DomainB");
        staticRouting.AddForwardRoute("DomainB", "DomainA");

        routerConfig.AutoCreateQueues();

        #endregion

        SqlHelper.EnsureDatabaseExists(ConnectionStrings.DomainA);
        SqlHelper.EnsureDatabaseExists(ConnectionStrings.DomainB);
        SqlHelper.EnsureDatabaseExists(ConnectionStrings.Router);

        domainASqlSubscriptionStorage.Install().GetAwaiter().GetResult();
        domainBSqlSubscriptionStorage.Install().GetAwaiter().GetResult();

        var router = Router.Create(routerConfig);

        await router.Start().ConfigureAwait(false);

        Console.WriteLine("Press <enter> to exit");
        Console.ReadLine();

        await router.Stop().ConfigureAwait(false);
    }
}