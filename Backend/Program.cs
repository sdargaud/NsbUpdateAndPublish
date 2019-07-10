using System;
using System.Data.SqlClient;
using System.Threading.Tasks;
using NServiceBus;
using NServiceBus.Persistence.Sql;
using Shared;

static class Program
{
    static async Task Main()
    {
        Console.Title = "Backend";
        var endpointConfiguration = new EndpointConfiguration("DomainB-Endpoint");
        var transport = endpointConfiguration.UseTransport<SqlServerTransport>();
        transport.ConnectionString(ConnectionStrings.DomainB);
        transport.Transactions(TransportTransactionMode.SendsAtomicWithReceive);

        var persistence = endpointConfiguration.UsePersistence<SqlPersistence>();
        persistence.SqlDialect<SqlDialect.MsSqlServer>();
        persistence.ConnectionBuilder(
            connectionBuilder: () =>
            {
                return new SqlConnection(ConnectionStrings.DomainB);
            });
        var subscriptions = persistence.SubscriptionSettings();
        subscriptions.DisableCache();
        endpointConfiguration.EnableInstallers();

        #region Routing

        var routerConnector = transport.Routing().ConnectToRouter("DomainA-B-Router");
        routerConnector.RegisterPublisher(
            eventType: typeof(OrderAccepted),
            publisherEndpointName: "DomainA-Endpoint");

        #endregion

        var endpointInstance = await Endpoint.Start(endpointConfiguration)
            .ConfigureAwait(false);
        Console.WriteLine("Press any key to exit");
        Console.ReadKey();
        await endpointInstance.Stop()
            .ConfigureAwait(false);
    }
}